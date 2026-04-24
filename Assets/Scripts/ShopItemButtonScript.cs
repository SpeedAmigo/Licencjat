using FishNet.Object;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShopItemButtonScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    [SerializeField] private bool addToBasket; 
    [SerializeField] private ShopItemUIScript shopItemUIScript;

    [SerializeField] private EventReference addToBasketSound;
    [SerializeField] private EventReference removeFromBasketSound;
    
    public void Interact(PlayerRoot playerRoot)
    {
        if (!ShopManagerScript.Instance || !IsController) return;
        
        if (addToBasket)
        {
            ShopManagerScript.Instance.AddItemToBasket(shopItemUIScript.ItemId);
            SoundCreator.Instance.PlayOneShot(addToBasketSound, transform.position);
        }
        else
        {
            ShopManagerScript.Instance.RemoveItemFromBasket(shopItemUIScript.ItemId);
            SoundCreator.Instance.PlayOneShot(removeFromBasketSound, transform.position);
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
