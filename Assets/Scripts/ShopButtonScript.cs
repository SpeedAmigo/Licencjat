using FishNet.Object;
using UnityEngine;

public class ShopButtonScript : NetworkBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacted");
    }

    public string GetInteractText()
    {
        return "Buy";
    }
}
