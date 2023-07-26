using Unity.Netcode;
using UnityEngine;

public class Chest : Interactable
{
    [SerializeField] private Animator animatorChest = null;
    [SerializeField] private ParticleSystem[] particleSystems;
    private bool _locked = false;
    private bool _opened = false;
    private static readonly int Open = Animator.StringToHash("Open");

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
        base.StopInteract();
        OutlinesEffect(false);
    }

    public override void StartInteract()
    {
        if (!_locked && !_opened)
        {
            base.StartInteract();
            OutlinesEffect(true);
        }
    }
    public override void StopInteract()
    {
        if (!_locked && !_opened)
        {
            base.StopInteract();
            OutlinesEffect(false);
        }
    }
    public override void Interact(Character character)
    {
        if (!_locked && !_opened)
        {
            if (UICoin.Instance.CoinNumber < 5)
            {
                return;
            }
            base.Interact(character);
            interactState = !interactState;
            animatorChest.SetTrigger(Open);
            UICoin.Instance.CoinNumber -= 5;
            character.GetComponent<Player>().AddRandomWeapon();
            foreach (var p in particleSystems)
            {
                p.Play();
            }
            base.StopInteract();
            OutlinesEffect(false);
            _opened = true;
        }
    }
}
