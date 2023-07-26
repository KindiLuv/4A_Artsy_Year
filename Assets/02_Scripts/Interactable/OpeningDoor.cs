using UnityEngine;
using Unity.Netcode;
public class OpeningDoor : Interactable
{
    [SerializeField] private Animator animatorDoor = null;
    private bool _locked = false;

    #region Getter Setter
    public bool Locked { get { return _locked; } set 
        {             
            if (interactState && !_locked && value)
            {
                LockDoorClientRpc();                
            }
            _locked = value;
            LockClientRpc(_locked);
        }
    }
    #endregion

    [ClientRpc]
    public void LockClientRpc(bool state)
    {
        _locked = state;
    }

    [ClientRpc]
    public void LockDoorClientRpc()
    {
        interactState = false;
        animatorDoor.SetBool("Swap", interactState);
        base.StopInteract();
        OutlinesEffect(false);
    }

    public override void StartInteract()
    {
        if (!_locked)
        {
            base.StartInteract();
            OutlinesEffect(true);
        }
    }
    public override void StopInteract()
    {
        if (!_locked)
        {
            base.StopInteract();
            OutlinesEffect(false);
        }
    }


    public override void Interact(Character character)
    {
        if (!_locked)
        {
            base.Interact(character);
            interactState = !interactState;
            animatorDoor.SetBool("Swap", interactState);
        }
    }
}
