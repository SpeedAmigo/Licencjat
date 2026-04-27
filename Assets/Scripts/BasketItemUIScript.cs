using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasketItemUIScript : NetworkBehaviour
{
    [SerializeField] private Image image;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.SetActive(false);
    }

    public void CardSetup(ShopItemData itemData)
    {
        gameObject.SetActive(true);

        image.sprite = itemData.itemIcon;
    }
}
