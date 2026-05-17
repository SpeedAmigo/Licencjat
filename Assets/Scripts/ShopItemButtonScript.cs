using FishNet.Object;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class ShopItemButtonScript : BaseInteractable
{
    [SerializeField] private ShopItemUIScript shopItemUIScript;

    [Header("Sounds")]
    [SerializeField] private EventReference addToBasketSound;
    
    public override void Interact(PlayerRoot playerRoot)
    {
        if (!ShopManagerScript.Instance || !IsController) return;
        
        ShopManagerScript.Instance.AddItemToBasket(shopItemUIScript.ItemId);
        SoundCreator.Instance.PlayOneShot(addToBasketSound, transform.position);
    }
}