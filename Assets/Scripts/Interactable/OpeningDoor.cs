using UnityEngine;

public class OpeningDoor : Interactable
{
    [SerializeField] private Transform leftPart;
    [SerializeField] private Transform rightPart;
    private bool isOpen;
    
    public override void Interact(Character character)
    {
        base.Interact(character);
        interactState = !interactState;
        OpenDoor();
    }

    public void OpenDoor()
    {
        leftPart.position += Vector3.left;
        rightPart.position += Vector3.right;
    }
}
