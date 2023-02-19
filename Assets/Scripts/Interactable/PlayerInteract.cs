using ArtsyNetcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInteract : NetEntity
{
    [SerializeField] private List<Interactable> interactable = new List<Interactable>();
    [SerializeField] private float fovInteraction = 60f;
    private Interactable lastInteraction = null;
    private Player player = null;

    #region GetterSetter
    public bool CanInteract
    {
        get
        {
            return interactable.Count > 0;
        }
    }

    public bool isInteract
    {
        get
        {
            return lastInteraction != null ? lastInteraction.InteractState : false;
        }
    }
    #endregion

    private void Start()
    {
        player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (other.GetComponent<Interactable>())
            {
                interactable.Add(other.GetComponent<Interactable>());
                other.GetComponent<Interactable>().StartInteract();
            }
        }
    }
    public void ClearInteractable()
    {
        foreach (Interactable i in interactable)
        {
            if(i!= null)
            {
                i.StopInteract();
            }
        }
        interactable.Clear();
    }

    public List<Interactable> getInteractable()
    {
        return interactable;
    }

    public void Interact()
    {            
        if (false /*condition joueur a mettre*/) return;
        Interactable inter = null;
        float distance = float.MaxValue;
        interactable.RemoveAll(i => i == null);
        List<Interactable> interList = isInView(interactable,fovInteraction);
        foreach (Interactable i in interList)
        {
            if (i.InteractState)
            {
                inter = i;
                break;
            }
            if (Vector3.Distance(i.gameObject.transform.position, transform.position) < distance)
            {
                distance = Vector3.Distance(i.gameObject.transform.position, transform.position);
                inter = i;
            }
        }
        if (inter != null)
        {
            if (inter != lastInteraction)
            {
                if (lastInteraction != null)
                {
                    lastInteraction.ChangeInteract();
                }
            }
            lastInteraction = inter;
            inter.Interact(this.GetComponent<Character>());
        }
    }

    public List<Interactable> isInView(List<Interactable> list, float fov)
    {            
        return list.FindAll(x => x != null &&  Vector3.Angle(x.transform.position - player.transform.position, -player.transform.forward) < fov || x.InteractState);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (other.GetComponent<Interactable>())
            {
                other.GetComponent<Interactable>().StopInteract();
                interactable.Remove(other.GetComponent<Interactable>());
            }
        }
    }
}
