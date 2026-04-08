using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShopItemButtonScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    [SerializeField] private bool addToBasket; 
    [SerializeField] private ShopItemUIScript shopItemUIScript;
    
    public void Interact(PlayerRoot playerRoot)
    {
        if (!ShopManagerScript.Instance || !IsController) return;
        
        if (addToBasket)
        {
            ShopManagerScript.Instance.AddItemToBasket(shopItemUIScript.ItemId);
        }
        else
        {
            ShopManagerScript.Instance.RemoveItemFromBasket(shopItemUIScript.ItemId);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
