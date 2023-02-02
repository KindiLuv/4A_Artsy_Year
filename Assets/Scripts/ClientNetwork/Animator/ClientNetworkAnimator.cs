using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Linq;

namespace Unity.Netcode.Components
{
    /// <summary>
    /// ClientNetworkAnimator enables remote synchronization of <see cref="UnityEngine.Animator"/> state for on network objects.
    /// </summary>
    [AddComponentMenu("Netcode/" + nameof(ClientNetworkAnimator))]
    [RequireComponent(typeof(Animator))]
    public class ClientNetworkAnimator : NetworkAnimatorBase
    {
        private ClientRpcParams clientSendServerIgnoreOwnerClientID;//SERVER ONLY
        private NetworkObject owner;
        public override void OnNetworkSpawn()
        {
            if(IsServer)
            {
                owner = GetComponent<NetworkObject>();                
            }
            m_SendMessagesAllowed = IsClient && IsOwner;
            int layers = m_Animator.layerCount;
            m_TransitionHash = new int[layers];
            m_AnimationHash = new int[layers];
            m_LayerWeights = new float[layers]; 
            base.InitAnimator();
        }

        protected override void LateUpdate()
        {
            if (!m_SendMessagesAllowed || !m_Animator || !m_Animator.enabled)
            {
                return;
            }

            for (int layer = 0; layer < m_Animator.layerCount; layer++)
            {
                int stateHash;
                float normalizedTime;
                if (!CheckAnimStateChanged(out stateHash, out normalizedTime, layer))
                {
                    continue;
                }

                var animMsg = new AnimationMessage
                {
                    StateHash = stateHash,
                    NormalizedTime = normalizedTime,
                    Layer = layer,
                    Weight = m_LayerWeights[layer]
                };

                SendAnimStateServerRpc(animMsg);
            }
            if(Time.time-syncTime > syncInterval)
            {
                m_ParameterWriter.Seek(0);
                m_ParameterWriter.Truncate();
                if(WriteParameters(m_ParameterWriter))
                {
                    SendParameterStateServerRpc(m_ParameterWriter.ToArray());
                }
                syncTime = Time.time;
            }
        }

        
        /// <summary>
        /// Internally-called RPC server receiving function to update some animation parameters on a client when
        ///   the client and server wants to update them
        /// </summary>
        /// <param name="animSnapshot">the payload containing the parameters to apply</param>
        /// <param name="clientRpcParams">unused</param>
        [ServerRpc]
        private unsafe void SendAnimStateServerRpc(AnimationMessage animSnapshot)
        {
            clientSendServerIgnoreOwnerClientID = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != owner.OwnerClientId).ToList() },
            };
            SendAnimStateClientRpc(animSnapshot,clientSendServerIgnoreOwnerClientID);
        }

        /// <summary>
        /// Internally-called RPC server receiving function to update some animation value parameter on a server when
        ///   the client and server wants to update them
        /// </summary>
        /// <param name="Parameters">containes parameter value</param>
        [ServerRpc]
        private unsafe void SendParameterStateServerRpc(byte[] Parameters)
        {
            clientSendServerIgnoreOwnerClientID = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != owner.OwnerClientId).ToList() },
            };
            SendParameterStateClientRpc(Parameters,clientSendServerIgnoreOwnerClientID);
        }

        /// <summary>
        /// Internally-called RPC server receiving function to update a trigger when the server wants to forward
        ///   a trigger for a client to play / reset
        /// </summary>
        /// <param name="animSnapshot">the payload containing the trigger data to apply</param>
        /// <param name="clientRpcParams">unused</param>
        [ServerRpc]
        private void SendAnimTriggerServerRpc(AnimationTriggerMessage animSnapshot, ServerRpcParams clientRpcParams = default)
        {
             clientSendServerIgnoreOwnerClientID = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != owner.OwnerClientId).ToList() },
            };
            SendAnimTriggerClientRpc(animSnapshot,clientSendServerIgnoreOwnerClientID);
        }

        /// <inheritdoc cref="SetTrigger(string)" />
        /// <param name="hash">The hash for the trigger to activate</param>
        /// <param name="reset">If true, resets the trigger</param>
        public override void SetTrigger(int hash, bool reset = false)
        {
            var animMsg = new AnimationTriggerMessage();
            animMsg.Hash = hash;
            animMsg.Reset = reset;

            if (IsClient && IsOwner)
            {
                if (reset)
                {
                    m_Animator.ResetTrigger(hash);
                }
                else
                {
                    m_Animator.SetTrigger(hash);
                }

                // ...then tell all the clients to do the same
                SendAnimTriggerClientRpc(animMsg);
            }
            else
            {
                Debug.LogWarning("Trying to call NetworkAnimator.SetTrigger on a client...ignoring");
            }
        }
    }
}

