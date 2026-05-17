using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUIScript : NetworkBehaviour
{
    [SerializeField] private TMP_Text itemPriceText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Image itemImage;
    
    [SerializeField] private string _itemId;
    
    public string ItemId {get{return _itemId;}}
    
    public void CardSetup(Sprite itemIcon, string itemName, string itemDesc, int itemPrice, string itemId)
    {
        itemImage.sprite = itemIcon;
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDesc;
        itemPriceText.text = itemPrice.ToString();
        _itemId = itemId;
    }
    
    /*public void Interact()
    {
        if (ShopManagerScript.Instance)
        {
            if (IsController)
            {
                ShopManagerScript.Instance.BuyItem(_itemId); 
            }
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }*/
}
