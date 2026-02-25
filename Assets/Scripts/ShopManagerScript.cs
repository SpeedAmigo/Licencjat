using System;
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
    
    [AllowMutableSyncType] public SyncVar<uint> currentMoney;
    
    public List<ShopItemData> shopItems;
    
    [SerializeField] private Transform spawnLocation;

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

    public override void OnStartServer()
    {
        MoneyChanged?.Invoke(currentMoney.Value);
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
        MoneyChanged?.Invoke(currentMoney.Value);
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
        MoneyChanged?.Invoke(currentMoney.Value);
    }

    private ShopItemData GetItemById(string id)
    {
        return shopItems.Find(item => item.ItemID == id);
    }
}
