using System;
using ArtsyNetcode;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Interactable : NetEntity
{
    [Header("Animator")]
    [SerializeField] protected Animator animator = null;
    protected bool useState = false;
    protected bool interactState = false;
    private SphereCollider sphereCollider;
    private GameObject interactableObject;
    [SerializeField] [Range(.5f, 2f)] private float radius;
    
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
        useState = true;
        animator.SetTrigger("Start");
    }
        
    public virtual void Interact(Character character)
    {
        if (useState)
        {
            animator.SetTrigger("Use");
            InteractServerRpc();
        }            
    }

    [ServerRpc(RequireOwnership = false)]
    public void InteractServerRpc()
    {
        Debug.Log("");
    }

    public virtual void StopInteract()
    {
        if (useState)
        {
            animator.SetTrigger("Stop");
            useState = false;
        }
    }

    public override void OnGameStateChanged(GameState newGameState)
    {
        base.OnGameStateChanged(newGameState);
        if(animator != null)
        {
            animator.speed = newGameState == GameState.GamePlay ? 1.0f : 0.0f;
        }
    }


    public virtual void ChangeInteract(){}

    private void OnValidate()
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
    }
}
