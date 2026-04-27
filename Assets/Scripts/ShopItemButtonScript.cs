using FishNet.Object;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShopItemButtonScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    [SerializeField] private ShopItemUIScript shopItemUIScript;

    [Header("Sounds")]
    [SerializeField] private EventReference addToBasketSound;
    
    public void Interact(PlayerRoot playerRoot)
    {
        if (!ShopManagerScript.Instance || !IsController) return;
        
        ShopManagerScript.Instance.AddItemToBasket(shopItemUIScript.ItemId);
        SoundCreator.Instance.PlayOneShot(addToBasketSound, transform.position);
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
