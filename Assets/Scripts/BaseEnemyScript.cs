using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using RaycastPro.Detectors;
using UnityEngine;

public class BaseEnemyScript : NetworkBehaviour
{
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> playersInRange = new();
    // Server-side list of players inside range
    //public List<GameObject> playersInRange = new();
    
    [SerializeField] private RangeDetector rangeDetector;
    
    #region PlayerDetection
    private void OnDetected(Collider other)
    {
        if (other.CompareTag("Player") && !playersInRange.Contains(other.gameObject))
        {
            //playersInRange.Add(other.gameObject);
            AddPlayerToServerList(other.gameObject);
        }
    }

    private void OnLost(Collider other)
    {
        if (other.CompareTag("Player") && playersInRange.Contains(other.gameObject))
        {
            //playersInRange.Remove(other.gameObject);
            RemovePlayerFromServerList(other.gameObject);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToServerList(GameObject obj)
    {
        if (playersInRange.Contains(obj)) return;
        
        playersInRange.Add(obj);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromServerList(GameObject obj)
    {
        if (!playersInRange.Contains(obj)) return;
        
        playersInRange.Remove(obj);
    }

    #endregion
    
    #region Enable/Disable
    
    private void OnEnable()
    {
        rangeDetector.onDetectCollider.AddListener(OnDetected);
        rangeDetector.onLostCollider.AddListener(OnLost);
    }

    private void OnDisable()
    {
        rangeDetector.onDetectCollider.RemoveListener(OnDetected);
        rangeDetector.onLostCollider.RemoveListener(OnLost);
    }
    
    #endregion
}
