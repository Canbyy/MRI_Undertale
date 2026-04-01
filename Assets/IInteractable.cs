public interface IInteractable
{
    // Determines if the object can currently be interacted with
    bool CanInteract();
    // Defines the interaction behavior when the player interacts with the object
    void Interact();
}
