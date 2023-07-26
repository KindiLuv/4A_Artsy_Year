using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.Netcode.Components
{
    /// <summary>
    /// NetworkAnimatorBase enables remote synchronization of <see cref="UnityEngine.Animator"/> state for on network objects.
    /// </summary>
    [AddComponentMenu("Netcode/" + nameof(NetworkAnimatorBase))]
    [RequireComponent(typeof(Animator))]
    public class NetworkAnimatorBase : NetworkBehaviour
    {
        protected internal struct AnimationMessage : INetworkSerializable
        {
            // state hash per layer.  if non-zero, then Play() this animation, skipping transitions
            public int StateHash;
            public float NormalizedTime;
            public int Layer;
            public float Weight;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref StateHash);
                serializer.SerializeValue(ref NormalizedTime);
                serializer.SerializeValue(ref Layer);
                serializer.SerializeValue(ref Weight);
            }
        }

        protected internal struct AnimationTriggerMessage : INetworkSerializable
        {
            public int Hash;
            public bool Reset;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Hash);
                serializer.SerializeValue(ref Reset);
            }
        }

        [SerializeField] protected Animator m_Animator;
        [SerializeField][Range(0.01f, 1.0f)] protected float syncInterval = 0.1f;
        protected RuntimeAnimatorController m_Controller;

        public Animator Animator
        {
            get { return m_Animator; }
            set
            {
                m_Animator = value;
            }
        }

        protected bool m_SendMessagesAllowed = false;

        // Animators only support up to 32 params
        public static int K_MaxAnimationParams = 32;

        protected int[] m_TransitionHash;
        protected int[] m_AnimationHash;
        protected float[] m_LayerWeights;
        protected float syncTime = 0.0f;
        protected unsafe struct AnimatorParamCache
        {
            public int Hash;
            public int Type;
            public fixed byte Value[4]; // this is a max size of 4 bytes
        }

        // 128 bytes per Animator
        protected FastBufferWriter m_ParameterWriter = new FastBufferWriter(K_MaxAnimationParams * sizeof(float), Allocator.Persistent);
        private NativeArray<AnimatorParamCache> m_CachedAnimatorParameters;
        private bool[] m_CachedChangeBoolParameters;
        private int[] m_CachedChangeIntParameters;
        private float[] m_CachedChangeFloatParameters;

        // We cache these values because UnsafeUtility.EnumToInt uses direct IL that allows a non-boxing conversion
        private struct AnimationParamEnumWrapper
        {
            public static readonly int AnimatorControllerParameterInt;
            public static readonly int AnimatorControllerParameterFloat;
            public static readonly int AnimatorControllerParameterBool;

            static AnimationParamEnumWrapper()
            {
                AnimatorControllerParameterInt = UnsafeUtility.EnumToInt(AnimatorControllerParameterType.Int);
                AnimatorControllerParameterFloat = UnsafeUtility.EnumToInt(AnimatorControllerParameterType.Float);
                AnimatorControllerParameterBool = UnsafeUtility.EnumToInt(AnimatorControllerParameterType.Bool);
            }
        }

        public override void OnDestroy()
        {
            if (m_CachedAnimatorParameters.IsCreated)
            {
                m_CachedAnimatorParameters.Dispose();
            }

            m_ParameterWriter.Dispose();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                m_SendMessagesAllowed = true;
                int layers = m_Animator.layerCount;

                m_TransitionHash = new int[layers];
                m_AnimationHash = new int[layers];
                m_LayerWeights = new float[layers];
            }
            InitAnimator();
        }

        protected void InitAnimator()
        {
            syncTime = Time.time - Random.Range(0.0f, syncInterval);
            var parameters = m_Animator.parameters;
            m_Controller = m_Animator.runtimeAnimatorController;
            m_CachedAnimatorParameters = new NativeArray<AnimatorParamCache>(parameters.Length, Allocator.Persistent);
            int countint = 0;
            int countfloat = 0;
            int countbool = 0;
            List<int> intlist = new List<int>();
            List<float> floatlist = new List<float>();
            List<bool> boollist = new List<bool>();
            if(parameters.Length > 256)
            {
                Debug.LogError("Animator has too many parameters.  256 is the maximum.");
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (m_Animator.IsParameterControlledByCurve(parameter.nameHash))
                {
                    // we are ignoring parameters that are controlled by animation curves - syncing the layer
                    //  states indirectly syncs the values that are driven by the animation curves
                    continue;
                }

                var cacheParam = new AnimatorParamCache
                {
                    Type = UnsafeUtility.EnumToInt(parameter.type),
                    Hash = parameter.nameHash
                };

                unsafe
                {
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            float value = m_Animator.GetFloat(cacheParam.Hash);
                            UnsafeUtility.WriteArrayElement(cacheParam.Value, 0, value);
                            floatlist.Add(value);
                            countfloat++;
                            break;
                        case AnimatorControllerParameterType.Int:
                            int valueInt = m_Animator.GetInteger(cacheParam.Hash);
                            UnsafeUtility.WriteArrayElement(cacheParam.Value, 0, valueInt);
                            intlist.Add(valueInt);
                            countint++;
                            break;
                        case AnimatorControllerParameterType.Bool:
                            bool valueBool = m_Animator.GetBool(cacheParam.Hash);
                            UnsafeUtility.WriteArrayElement(cacheParam.Value, 0, valueBool);
                            boollist.Add(valueBool);
                            countbool++;
                            break;
                        case AnimatorControllerParameterType.Trigger:
                        default:
                            break;
                    }
                }

                m_CachedAnimatorParameters[i] = cacheParam;
            }
            m_CachedChangeBoolParameters = boollist.ToArray();
            m_CachedChangeIntParameters = intlist.ToArray();
            m_CachedChangeFloatParameters = floatlist.ToArray();
        }

        public override void OnNetworkDespawn()
        {
            m_SendMessagesAllowed = false;
        }

        protected virtual void LateUpdate()
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

                SendAnimStateClientRpc(animMsg);
            }
            if (Time.time - syncTime > syncInterval)
            {
                m_ParameterWriter.Seek(0);
                m_ParameterWriter.Truncate();
                if(WriteParameters(m_ParameterWriter))
                {
                    SendParameterStateClientRpc(m_ParameterWriter.ToArray());
                }
                syncTime = Time.time;
            }
        }

        protected bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layer)
        {
            bool shouldUpdate = false;
            stateHash = 0;
            normalizedTime = 0;

            float layerWeightNow = m_Animator.GetLayerWeight(layer);

            if (!Mathf.Approximately(layerWeightNow, m_LayerWeights[layer]))
            {
                m_LayerWeights[layer] = layerWeightNow;
                shouldUpdate = true;
            }
            if (m_Animator.IsInTransition(layer))
            {
                AnimatorTransitionInfo tt = m_Animator.GetAnimatorTransitionInfo(layer);
                if (tt.fullPathHash != m_TransitionHash[layer])
                {
                    // first time in this transition for this layer
                    m_TransitionHash[layer] = tt.fullPathHash;
                    m_AnimationHash[layer] = 0;
                    shouldUpdate = true;
                }
            }
            else
            {
                AnimatorStateInfo st = m_Animator.GetCurrentAnimatorStateInfo(layer);
                if (st.fullPathHash != m_AnimationHash[layer])
                {
                    // first time in this animation state
                    if (m_AnimationHash[layer] != 0)
                    {
                        // came from another animation directly - from Play()
                        stateHash = st.fullPathHash;
                        normalizedTime = st.normalizedTime;
                    }
                    m_TransitionHash[layer] = 0;
                    m_AnimationHash[layer] = st.fullPathHash;
                    shouldUpdate = true;
                }
            }

            return shouldUpdate;
        }

        protected unsafe bool WriteParameters(FastBufferWriter writer)
        {
            int countint = 0;
            int countbool = 0;
            int countfloat = 0;
            for (int i = 0; i < m_CachedAnimatorParameters.Length; i++)
            {
                ref var cacheValue = ref UnsafeUtility.ArrayElementAsRef<AnimatorParamCache>(m_CachedAnimatorParameters.GetUnsafePtr(), i);
                var hash = cacheValue.Hash;

                if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterFloat)
                {
                    float valueFloat = m_Animator.GetFloat(hash);
                    if (valueFloat != m_CachedChangeFloatParameters[countfloat])
                    {
                        m_CachedChangeFloatParameters[countfloat] = valueFloat;
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, valueFloat);
                            writer.WriteValueSafe((byte)i);
                            writer.WriteValueSafe(valueFloat);
                            countfloat++;
                        }
                    }
                }
                else if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterBool)
                {
                    bool valueBool = m_Animator.GetBool(hash);
                    if (valueBool != m_CachedChangeBoolParameters[countbool])
                    {
                        m_CachedChangeBoolParameters[countbool] = valueBool;
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, valueBool);
                            writer.WriteValueSafe((byte)i);
                            writer.WriteValueSafe(valueBool);
                            countbool++;
                        }
                    }
                }
                else if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterInt)
                {
                    int valueInt = m_Animator.GetInteger(hash);
                    if (valueInt != m_CachedChangeIntParameters[countint])
                    {
                        m_CachedChangeIntParameters[countint] = valueInt;
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, valueInt);
                            writer.WriteValueSafe((byte)i);
                            writer.WriteValueSafe(valueInt);
                            countint++;
                        }
                    }
                }
            }
            return (countint+countbool+countfloat) > 0;
        }

        protected unsafe void ReadParameters(FastBufferReader reader)
        {
            byte indice = 0;
            int hash = 0;
            while (reader.Position < reader.Length)
            {
                reader.ReadValueSafe(out indice);
                if (indice < m_CachedAnimatorParameters.Length)
                {
                    ref var cacheValue = ref UnsafeUtility.ArrayElementAsRef<AnimatorParamCache>(m_CachedAnimatorParameters.GetUnsafePtr(), indice);
                    hash = cacheValue.Hash;
                    if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterFloat)
                    {
                        reader.ReadValueSafe(out float newFloatValue);
                        m_Animator.SetFloat(hash, newFloatValue);
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, newFloatValue);
                        }
                    }
                    else if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterBool)
                    {
                        reader.ReadValueSafe(out bool newBoolValue);
                        m_Animator.SetBool(hash, newBoolValue);
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, newBoolValue);
                        }
                    }
                    else if (cacheValue.Type == AnimationParamEnumWrapper.AnimatorControllerParameterInt)
                    {
                        reader.ReadValueSafe(out int newIntValue);
                        m_Animator.SetInteger(hash, newIntValue);
                        fixed (void* value = cacheValue.Value)
                        {
                            UnsafeUtility.WriteArrayElement(value, 0, newIntValue);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Internally-called RPC client receiving function to update some animation State on a client when
        ///   the server wants to update them
        /// </summary>
        /// <param name="animSnapshot">the payload containing the State to apply</param>
        /// <param name="clientRpcParams">unused</param>
        [ClientRpc]
        protected unsafe void SendAnimStateClientRpc(AnimationMessage animSnapshot, ClientRpcParams clientRpcParams = default)
        {
            if (animSnapshot.StateHash != 0)
            {
                m_Animator.Play(animSnapshot.StateHash, animSnapshot.Layer, animSnapshot.NormalizedTime);
            }
            m_Animator.SetLayerWeight(animSnapshot.Layer, animSnapshot.Weight);
        }


        /// <summary>
        /// Internally-called RPC client receiving function to update some animation value parameter on a client when
        ///   the server wants to update them
        /// </summary>
        /// <param name="Parameters">containes parameter value</param>
        [ClientRpc]
        protected unsafe void SendParameterStateClientRpc(byte[] Parameters, ClientRpcParams clientRpcParams = default)
        {
            if (Parameters != null && Parameters.Length != 0)
            {
                // We use a fixed value here to avoid the copy of data from the byte buffer since we own the data
                fixed (byte* parameters = Parameters)
                {
                    var reader = new FastBufferReader(parameters, Allocator.None, Parameters.Length);
                    ReadParameters(reader);
                }
            }
        }

        /// <summary>
        /// Internally-called RPC client receiving function to update a trigger when the server wants to forward
        ///   a trigger for a client to play / reset
        /// </summary>
        /// <param name="animSnapshot">the payload containing the trigger data to apply</param>
        /// <param name="clientRpcParams">unused</param>
        [ClientRpc]
        protected void SendAnimTriggerClientRpc(AnimationTriggerMessage animSnapshot, ClientRpcParams clientRpcParams = default)
        {
            if (animSnapshot.Reset)
            {
                m_Animator.ResetTrigger(animSnapshot.Hash);
            }
            else
            {
                m_Animator.SetTrigger(animSnapshot.Hash);
            }
        }

        /// <summary>
        /// Sets the trigger for the associated animation
        ///  Note, triggers are special vs other kinds of parameters.  For all the other parameters we watch for changes
        ///  in FixedUpdate and users can just set them normally off of Animator. But because triggers are transitory
        ///  and likely to come and go between FixedUpdate calls, we require users to set them here to guarantee us to
        ///  catch it...then we forward it to the Animator component
        /// </summary>
        /// <param name="triggerName">The string name of the trigger to activate</param>
        public void SetTrigger(string triggerName)
        {
            SetTrigger(Animator.StringToHash(triggerName));
        }

        /// <inheritdoc cref="SetTrigger(string)" />
        /// <param name="hash">The hash for the trigger to activate</param>
        /// <param name="reset">If true, resets the trigger</param>
        public virtual void SetTrigger(int hash, bool reset = false)
        {
            var animMsg = new AnimationTriggerMessage();
            animMsg.Hash = hash;
            animMsg.Reset = reset;

            if (IsServer)
            {
                //  trigger the animation locally on the server...
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

        /// <summary>
        /// Resets the trigger for the associated animation.  See <see cref="SetTrigger(string)">SetTrigger</see> for more on how triggers are special
        /// </summary>
        /// <param name="triggerName">The string name of the trigger to reset</param>
        public void ResetTrigger(string triggerName)
        {
            ResetTrigger(Animator.StringToHash(triggerName));
        }

        /// <inheritdoc cref="ResetTrigger(string)" path="summary" />
        /// <param name="hash">The hash for the trigger to activate</param>
        public void ResetTrigger(int hash)
        {
            SetTrigger(hash, true);
        }
    }
}
