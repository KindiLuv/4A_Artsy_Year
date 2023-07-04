using UnityEngine;

public class Door : Interactable
{
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Interact(Character character)
    {
        base.Interact(character);
        interactState = !interactState;
        meshRenderer.enabled = !interactState;
    }

    public override void ChangeInteract()
    {
        base.ChangeInteract();

    }
}
