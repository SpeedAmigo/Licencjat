using FishNet.Object;
using TMPro;
using UnityEngine;

public class ShopUIScript : NetworkBehaviour
{
    [SerializeField] private NetworkObject verticalGroup;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private ShopItemUIScript itemTemplatePrefab;

    private void OnEnable()
    {
        ShopManagerScript.MoneyChanged += OnMoneyChanged;
    }

    private void OnDisable()
    {
        ShopManagerScript.MoneyChanged -= OnMoneyChanged;
    }
    
    private void Start()
    {
        if (IsServerInitialized)
        {
            SpawnTemplate();
        }
    }

    private void SpawnTemplate()
    {
        if (!itemTemplatePrefab) return;
        
        if (ShopManagerScript.Instance == null)
        {
            Debug.LogWarning("The ShopManagerScript instance is null.");
        }

        if (ShopManagerScript.Instance.shopItems.Count < 1)
        {
            Debug.LogWarning("The ShopManagerScript items are empty.");
        } 
        
        /*foreach (var item in ShopManagerScript.Instance.shopItems)
        {
            var spawnedTemplate = Instantiate(itemTemplatePrefab, verticalGroup.transform);
            Spawn(spawnedTemplate.gameObject);
            
            spawnedTemplate.NetworkObject.SetParent(verticalGroup);
            spawnedTemplate.CardSetup(item.itemIcon, item.itemName, item.itemDescription, item.itemPrice, item.ItemID);
            SetupCardClient(spawnedTemplate);
        }*/

        for (int i = 0; i < ShopManagerScript.Instance.shopItems.Count; i++)
        {
            var item = ShopManagerScript.Instance.shopItems[i];
            
            var spawnedTemplate = Instantiate(itemTemplatePrefab, verticalGroup.transform);
            Spawn(spawnedTemplate.gameObject);
            
            spawnedTemplate.NetworkObject.SetParent(verticalGroup);
            spawnedTemplate.CardSetup(item.itemIcon, item.itemName, item.itemDescription, item.itemPrice, item.ItemID);
            SetupCardClient(spawnedTemplate, i);
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void SetupCardClient(NetworkObject nob, int index)
    {
        var itemScript = nob.GetComponent<ShopItemUIScript>();
        var currentIndex = ShopManagerScript.Instance.shopItems[index];
        
        itemScript.CardSetup(currentIndex.itemIcon, currentIndex.itemName, currentIndex.itemDescription, currentIndex.itemPrice, currentIndex.ItemID);
    }
    
    private void OnMoneyChanged(uint moneyValue)
    {
        moneyText.text = moneyValue.ToString();
    }
}
