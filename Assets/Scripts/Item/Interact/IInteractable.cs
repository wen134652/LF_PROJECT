using UnityEngine;


public interface IInteractable
{
    string Prompt { get; }
    void SetPlayer(Transform player);
    void ClearPlayer(Transform player);
    bool CanInteract(Transform player);
    void Interact(Transform player);
}

