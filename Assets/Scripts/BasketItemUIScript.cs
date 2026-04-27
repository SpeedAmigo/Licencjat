using FishNet.Object;
using TMPro;
using UnityEngine;

public class BasketItemUIScript : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.SetActive(false);
    }

    public void CardSetup(ShopItemData itemData, string amount)
    {
        gameObject.SetActive(true);
        
        nameText.text = itemData.itemName;
        quantityText.text = amount;
    }
}
