using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using RaycastPro.Detectors;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class BaseEnemyScript : NetworkBehaviour
{
    [Header("Dependencies")]
    [SerializeField] protected NetworkAnimator animator;
    [SerializeField] private RangeDetector rangeDetector;
    
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> playersInRange = new();
    
    [Header("AI Movement Settings")]
    [SerializeField] private float range = 10f;
    
    protected AIPath ai;
    protected bool WaitingForPath;
    
    protected virtual void Awake()
    {
        ai = GetComponent<AIPath>();
    }
    
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
    
    protected virtual void OnEnable()
    {
        rangeDetector.onDetectCollider.AddListener(OnDetected);
        rangeDetector.onLostCollider.AddListener(OnLost);
    }

    protected virtual void OnDisable()
    {
        rangeDetector.onDetectCollider.RemoveListener(OnDetected);
        rangeDetector.onLostCollider.RemoveListener(OnLost);
    }
    
    #endregion
    
    #region AI
    
    protected void SetNewPath()
    {
        ai.destination = PickRandomPoint();
        WaitingForPath = false;
    }
    
    protected Vector3 PickRandomPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * range;
        randomPoint.y = 0;
        randomPoint += ai.position;
        return randomPoint;
    }
    
    # endregion
}
