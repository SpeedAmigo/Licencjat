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

    [Header("Damage settings")] 
    public StatusEffect damageEffect;
    
    [Header("Speed settings")]
    public float walkSpeed;
    public float runSpeed;
    
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> playersInRange = new();
    
    [Header("AI Movement Settings")]
    [SerializeField] private float radius = 10f;
    
    [HideInInspector] public AIPath ai;
    [HideInInspector] public bool WaitingForPath;
    
    [HideInInspector] public bool running;

    public bool Running
    {
        get => running;
        set => running = value;
    }
    
    protected virtual void Awake()
    {
        ai = GetComponent<AIPath>();
    }
    
    #region PlayerDetection
    protected virtual void OnDetected(Collider other)
    {
        if (other.CompareTag("Player") && !playersInRange.Contains(other.gameObject))
        {
            AddPlayerToServerList(other.gameObject);
        }
    }

    protected virtual void OnLost(Collider other)
    {
        if (other.CompareTag("Player") && playersInRange.Contains(other.gameObject))
        {
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
    
    public virtual void OnEnable()
    {
        rangeDetector.onDetectCollider.AddListener(OnDetected);
        rangeDetector.onLostCollider.AddListener(OnLost);
    }

    public virtual void OnDisable()
    {
        rangeDetector.onDetectCollider.RemoveListener(OnDetected);
        rangeDetector.onLostCollider.RemoveListener(OnLost);
    }
    
    #endregion
    
    #region AI
    
    public void SetNewPath()
    {
        ai.destination = PickRandomPoint();
        WaitingForPath = false;
    }
    
    public void SetNewPath(Vector3 target)
    {
        ai.destination = PickRandomPoint(target);
        WaitingForPath = false;
    }
    
    public Vector3 PickRandomPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * radius;
        randomPoint.y = 0;
        randomPoint += ai.position;
        return randomPoint;
    }
    
    public Vector3 PickRandomPoint(Vector3 target)
    {
        Vector3 randomPoint = Random.insideUnitSphere * radius;
        randomPoint.y = 0;
        randomPoint += target;
        return randomPoint;
    }
    
    # endregion
}
