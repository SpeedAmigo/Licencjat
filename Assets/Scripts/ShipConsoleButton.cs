using FishNet.Object;
using UnityEngine;

public class ShipConsoleButton : NetworkBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interact");
    }
}
