using FishNet.Object;
using UnityEngine;

public class ShopUIScript : NetworkBehaviour
{
    [SerializeField] private Transform verticalGroup;
    [SerializeField] private ShopItemUIScript itemTemplatePrefab;

    public override void OnStartServer()
    {
        if (!IsServerInitialized || !itemTemplatePrefab) return;
        
        if (ShopManagerScript.Instance == null)
        {
            Debug.LogWarning("The ShopManagerScript instance is null.");
        }

        if (ShopManagerScript.Instance.shopItems.Count < 1)
        {
            Debug.LogWarning("The ShopManagerScript items are empty.");
        } 
        
        foreach (var item in ShopManagerScript.Instance.shopItems)
        {
            var spawnedTemplate = Instantiate(itemTemplatePrefab, verticalGroup);
            
            spawnedTemplate.CardSetup(item.itemIcon, item.itemName, item.itemDescription, item.itemPrice, item.ItemID);
            spawnedTemplate.gameObject.transform.SetParent(verticalGroup);
            
            Spawn(spawnedTemplate);
            
            Debug.Log(spawnedTemplate.name);
        }
    }
}
