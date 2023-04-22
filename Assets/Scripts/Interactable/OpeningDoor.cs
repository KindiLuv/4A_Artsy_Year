using System;
using UnityEngine;

public class OpeningDoor : Interactable
{
    [SerializeField] private Transform leftPart;
    [SerializeField] private Transform rightPart;
    [SerializeField] private AnimationClip doorOpen;
    [SerializeField] private AnimationClip doorClose;
    private Animation animationComponent;
    private bool isOpen;

    private void Start()
    {
        animationComponent = gameObject.AddComponent<Animation>();
        doorOpen.legacy = true;
        doorClose.legacy = true;
        animationComponent.AddClip(doorOpen, "DoorOpen");
        animationComponent.AddClip(doorClose, "DoorClose");
    }

    public override void Interact(Character character)
    {
        base.Interact(character);
        switch (interactState)
        {
            case false:
                interactState = true;
                AnimationOpenDoor();
                break;
            case true:
                interactState = false;
                AnimationCloseDoor();
                break;
        }
    }

    public void AnimationOpenDoor()
    {
        animationComponent.Play("DoorOpen");
    }

    public void AnimationCloseDoor()
    {
        animationComponent.Play("DoorClose");
    }

    public void OpenDoor()
    {
        leftPart.position += Vector3.left;
        rightPart.position += Vector3.right;
    }

    public void CloseDoor()
    {
        leftPart.position -= Vector3.left;
        rightPart.position -= Vector3.right;
    }
}
