using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    public string ItemID => itemId;
    
    public NetworkObject itemPrefab;
    
    public int itemPrice;
    
    public string itemName;
    public string itemDescription;
    
    public Sprite itemIcon;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
