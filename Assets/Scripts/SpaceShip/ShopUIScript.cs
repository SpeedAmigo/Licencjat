using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIScript : NetworkBehaviour
{
    [SerializeField] private NetworkObject verticalGroup;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text basketValueText;
    [SerializeField] private ShopItemUIScript itemTemplatePrefab;
    [SerializeField] private BasketItemUIScript[] basketTemplates;

    private void OnEnable()
    {
        ShopManagerScript.MoneyChanged += OnMoneyChanged;
        ShopManagerScript.BasketChanged += OnUpdateBasketUI;
        ShopManagerScript.BasketValueChanged += OnBasketChanged;
    }

    private void OnDisable()
    {
        ShopManagerScript.MoneyChanged -= OnMoneyChanged;
        ShopManagerScript.BasketChanged -= OnUpdateBasketUI;
        ShopManagerScript.BasketValueChanged -= OnBasketChanged;
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

        List<NetworkObject> spawnedTemplates = new List<NetworkObject>();   
        
        for (int i = 0; i < ShopManagerScript.Instance.shopItems.Count; i++)
        {
            var item = ShopManagerScript.Instance.shopItems[i];
            
            var spawnedTemplate = Instantiate(itemTemplatePrefab, verticalGroup.transform);
            Spawn(spawnedTemplate.gameObject);
            
            spawnedTemplate.NetworkObject.SetParent(verticalGroup);
            spawnedTemplate.CardSetup(item.itemIcon, item.itemName, item.itemDescription, item.itemPrice, item.ItemID);
            spawnedTemplates.Add(spawnedTemplate);
        }
        
        SetupCardClient(spawnedTemplates);
    }
    
    [ObserversRpc(BufferLast = true)]
    private void SetupCardClient(List<NetworkObject> nobs)
    {
        for (int i = 0; i < nobs.Count; i++)
        {
            var itemScript = nobs[i].GetComponent<ShopItemUIScript>();
            
            var currentIndex = ShopManagerScript.Instance.shopItems[i];
            itemScript.CardSetup(currentIndex.itemIcon, currentIndex.itemName, currentIndex.itemDescription, currentIndex.itemPrice, currentIndex.ItemID);
        }
    }
    
    private void OnMoneyChanged(uint moneyValue)
    {
        moneyText.text = moneyValue.ToString();
    }

    private void OnBasketChanged(int basketValue)
    {
        OnBasketChangedObservers(basketValue);
    }

    [ObserversRpc(BufferLast = true)]
    private void OnBasketChangedObservers(int basketValue)
    {
        basketValueText.text = basketValue.ToString();
    }

    private void OnUpdateBasketUI(List<string> basketItems)
    {
        OnUpdateBasketUIObservers(basketItems);
    }
    
    [ObserversRpc(BufferLast = true)]
    private void OnUpdateBasketUIObservers(List<string> basketItems)
    {
        foreach (var basketTemplate in basketTemplates)
        {
            basketTemplate.gameObject.SetActive(false);
        }
        
        int i = 0;

        foreach (var basketItem in basketItems)
        {
            if (i >= basketTemplates.Length) break;

            var itemData = ShopManagerScript.Instance.GetItemById(basketItem);
            
            if (itemData == null)
            {
                Debug.LogWarning($"Missing item for id {basketItem}");
                continue;
            }
            
            basketTemplates[i].gameObject.SetActive(true);
            basketTemplates[i].CardSetup(itemData);

            i++;
        }
    }
}
    