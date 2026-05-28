using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEditor;
using UnityEngine;

public class ShopManagerScript : NetworkBehaviour
{
    public static ShopManagerScript Instance;

    public static event Action<uint> MoneyChanged;
    public static event Action<List<string>> BasketChanged;
    public static event Action<int> BasketValueChanged;
    
    [AllowMutableSyncType] public SyncVar<uint> currentMoney;
    
    public List<ShopItemData> shopItems;
    
    [SerializeField] private Transform spawnLocation;

    [SerializeField] private int maxBasketItems = 4;
    [SerializeField] private float spawnRate = 1f;
    
    private List<string> basketItems = new();
    private int basketValue = 0;
    
    private Coroutine basketCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }    
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void BuyItem(string itemId)
    {
        var pickedItem = GetItemById(itemId);
        Debug.Log(pickedItem);
        
        if (!CanBuyItem(pickedItem.itemPrice)) return;
        
        SpawnItem(pickedItem.itemPrefab, spawnLocation);
        TakeMoney((uint)pickedItem.itemPrice);
    }

    [ServerRpc(RequireOwnership = false)]
    public void BuyItems()
    {
        if (basketCoroutine != null) return;
         
        if (basketItems.Count < 1) return;

        if (!CanBuyItem(basketValue)) return;

        TakeMoney((uint)basketValue);
        basketCoroutine = StartCoroutine(ItemSpawnCoroutine(new List<string>(basketItems)));
    }

    private IEnumerator ItemSpawnCoroutine(List<string> basket)
    {
        foreach (var item in basket)
        {
            var pickedItem = GetItemById(item);

            if (pickedItem == null)
            {
                Debug.LogWarning($"Couldn't find item with id {item}");
                continue;
            }
            
            yield return new WaitForSeconds(spawnRate);
            if (pickedItem.itemPrefab != null)
            {
                SpawnItem(pickedItem.itemPrefab, spawnLocation);
            }
        }
        
        basketItems.Clear();
        basketValue = 0;
        basketCoroutine = null;
        
        BasketValueChanged?.Invoke(basketValue);
        BasketChanged?.Invoke(basketItems);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddItemToBasket(string itemId)
    {
        if (GetTotalBasketItemCount() >= maxBasketItems) return;
        
        var pickedItem = GetItemById(itemId);

        if (pickedItem == null)
        {
            Debug.LogWarning($"Couldn't find item with id {itemId}");
            return;
        }
        
        basketItems.Add(itemId);
        basketValue += pickedItem.itemPrice;
        
        BasketValueChanged?.Invoke(basketValue);
        BasketChanged?.Invoke(basketItems);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemFromBasket(string itemId)
    {
        var pickedItem = GetItemById(itemId);

        if (basketItems.Contains(itemId))
        {
            basketItems.Remove(itemId);
            basketValue -= pickedItem.itemPrice;
        } 
        
        BasketValueChanged?.Invoke(basketValue);
        BasketChanged?.Invoke(basketItems);
    }

    [Server]
    private void SpawnItem(NetworkObject prefab, Transform location)
    {
        var createdItem = Instantiate(prefab, location.position, location.rotation);
        Spawn(createdItem);
    }

    [Server]
    public void UpdateMoney(uint money)
    {
        currentMoney.Value += money;
        UpdateMoneyUIClient(currentMoney.Value);
    }

    [Server]
    private bool CanBuyItem(int itemPrice)
    {
        return currentMoney.Value >= itemPrice;
    }

    [Server]
    private void TakeMoney(uint money)
    {
        currentMoney.Value -= money;
        UpdateMoneyUIClient(currentMoney.Value);
        //MoneyChanged?.Invoke(currentMoney.Value);
    }

    [ObserversRpc]
    private void UpdateMoneyUIClient(uint money)
    {
        MoneyChanged?.Invoke(money);
    }

    public ShopItemData GetItemById(string id)
    {
        return shopItems.Find(item => item.ItemID == id);
    }

    private int GetTotalBasketItemCount()
    {
        return basketItems.Count;
    }
}
