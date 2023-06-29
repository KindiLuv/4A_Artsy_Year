using UnityEngine;

public class OpeningDoor : Interactable
{
    [SerializeField] private Animator animatorDoor = null;

    public override void Interact(Character character)
    {
        base.Interact(character);        
        interactState = !interactState;
        animatorDoor.SetBool("Swap", interactState);
    }
}
