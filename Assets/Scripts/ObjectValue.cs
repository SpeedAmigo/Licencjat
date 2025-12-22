using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectValue : NetworkBehaviour
{
    [Header("Value Settings")]
    [GUIColor("Green")]
    [SerializeField] private Vector2Int minMaxBuyValue;
    
    [GUIColor("Green")]
    [SerializeField] private Vector2Int minMaxSellValue;
    
    [GUIColor("Blue")]
    [AllowMutableSyncType] public SyncVar<int> actualBuyValue;
    [GUIColor("Blue")]
    [AllowMutableSyncType] public SyncVar<int> actualSellValue;

    private void Start()
    {
        if (!IsServerInitialized) return;
        
        actualBuyValue.Value = PickRandomValue(minMaxBuyValue);
        actualSellValue.Value = PickRandomValue(minMaxSellValue);
    }

    private int PickRandomValue(Vector2Int range)
    {
        return Random.Range(range.x, range.y + 1);
    }
}