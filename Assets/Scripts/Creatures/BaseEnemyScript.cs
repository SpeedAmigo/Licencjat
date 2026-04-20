using System.Collections;
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
    
    public NetworkAnimator Animator => animator;

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
    [HideInInspector] public bool waitingForPath;
    
    [HideInInspector] public bool running;
    private IEnumerator _speedCoroutine;

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
        waitingForPath = false;
    }
    
    public void SetNewPath(Vector3 target)
    {
        ai.destination = PickRandomPoint(target);
        waitingForPath = false;
    }
    
    public Vector3 PickRandomPoint()
    {
        Vector2 random2D = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(random2D.x, 0, random2D.y) + ai.position;
        
        NearestNodeConstraint constraint = NearestNodeConstraint.Walkable;
        var nearest = AstarPath.active.GetNearest(randomPoint, constraint);
        
        return nearest.position;
    }
    
    public Vector3 PickRandomPoint(Vector3 target)
    {
        Vector2 random2D = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(random2D.x, 0, random2D.y) + target;
        
        NearestNodeConstraint constraint = NearestNodeConstraint.Walkable;
        var nearest = AstarPath.active.GetNearest(randomPoint, constraint);
        
        return nearest.position;
    }
    
    public void ChangeSpeed(float newSpeed, float duration)
    {
        if (_speedCoroutine != null)
        {
            StopCoroutine(_speedCoroutine);
        }
        
        StartCoroutine(ChangeSpeedCoroutine(newSpeed, duration));
    }

    public bool ReachedDestination()
    {
        return ai.reachedDestination && ai.reachedEndOfPath && !waitingForPath;
    }

    private IEnumerator ChangeSpeedCoroutine(float newSpeed, float duration)
    {
        float startSpeed = ai.maxSpeed;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            ai.maxSpeed = Mathf.Lerp(startSpeed, newSpeed, time / duration);
            yield return null;
        }

        ai.maxSpeed = newSpeed;
    }
    
    # endregion
}
