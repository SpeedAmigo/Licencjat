using FishNet.Object;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasketItemUIScript : BaseInteractable
{
    [SerializeField] private Image image;

    [Header("Sounds")]
    [SerializeField] private EventReference removeSound;
    
    private string _itemID;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.SetActive(false);
    }

    public void CardSetup(ShopItemData itemData)
    {
        gameObject.SetActive(true);

        _itemID = null;
        _itemID = itemData.ItemID;
        image.sprite = itemData.itemIcon;
    }

    public override void Interact(PlayerRoot playerRoot)
    {
        if (!ShopManagerScript.Instance || !IsController) return;
        
        ShopManagerScript.Instance.RemoveItemFromBasket(_itemID);
        SoundCreator.Instance.PlayOneShotAttached(removeSound, gameObject);
    }
}