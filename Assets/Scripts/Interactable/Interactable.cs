﻿using System;
using ArtsyNetcode;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SphereCollider))]
public class Interactable : NetEntity
{
    [Header("Animator")]
    [SerializeField] protected Animator animator = null;
    protected bool interactState = false;
    private SphereCollider sphereCollider;
    private GameObject interactableObject;
    [SerializeField][Range(.5f, 2f)] private float radius;

    #region GetterSetter

    public bool InteractState
    {
        get
        {
            return interactState;
        }
    }
    #endregion

    public virtual void StartInteract()
    {
        animator.SetTrigger("Start");
    }

    public virtual void Interact(Character character)
    {
        animator.SetTrigger("Use");
        if (IsLocalPlayer)
        {
            NetworkObject obj = character.GetComponent<NetworkObject>();
            InteractServerRpc(obj.OwnerClientId, obj.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc(ulong clientId, ulong networkId)
    {
        Debug.Log("Interact Server");
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.ConnectedClientsIds.Where(x => x != clientId).ToArray() }
        };
        InteractClientRpc(networkId, clientRpcParams);
    }

    [ClientRpc]
    public void InteractClientRpc(ulong networkId, ClientRpcParams clientRpcParams = default)
    {
        NetworkObject obj = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out obj))
        {
            Interact(obj.GetComponent<Character>());
        }
    }

    public virtual void StopInteract()
    {
        animator.SetTrigger("Stop");
    }

    public override void OnGameStateChanged(GameState newGameState)
    {
        base.OnGameStateChanged(newGameState);
        if (animator != null)
        {
            animator.speed = newGameState == GameState.GamePlay ? 1.0f : 0.0f;
        }
    }


    public virtual void ChangeInteract() { }

    /*private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
            {
                return;
            }
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.radius = radius;
            sphereCollider.isTrigger = true;
            if (transform.childCount == 0)
            {
                interactableObject = Instantiate(Resources.Load("Animation/Select/SelectInteractable")) as GameObject;
                interactableObject.transform.SetParent(transform);
                interactableObject.transform.localPosition = Vector3.zero;
                animator = interactableObject.GetComponent<Animator>();
            }

            if (CompareTag("Untagged"))
            {
                tag = "Interactable";
            }
        };
#endif
    }*/
}
