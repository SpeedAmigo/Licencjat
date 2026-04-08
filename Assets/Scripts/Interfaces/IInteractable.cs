using FishNet.Connection;
using UnityEngine;

public interface IInteractable
{
    public void Interact(PlayerRoot playerRoot);
    public string GetInteractText();
}
