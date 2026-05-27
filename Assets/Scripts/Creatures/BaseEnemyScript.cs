using System.Collections;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using RaycastPro.Detectors;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class BaseEnemyScript : NetworkBehaviour
{
    [Header("Dependencies")]
    [SerializeField] protected NetworkAnimator animator;
    [SerializeField] private RangeDetector rangeDetector;
    
    public NetworkAnimator Animator => animator;

    [Header("Damage settings")] 
    public StatusEffect[] damageEffects;
    
    [Header("Speed settings")]
    public float walkSpeed;
    public float runSpeed;
    
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> playersInRange = new();
    
    [Header("AI Movement Settings")]
    [SerializeField] private float radius = 10f;

    [Header("Pathfinding Tags")] 
    [SerializeField] private int[] allowedTags = { 0 };
    
    [HideInInspector] public AIPath ai;
    [HideInInspector] public bool waitingForPath;
    
    [HideInInspector] public bool running;
    private Coroutine _speedCoroutine;

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
    
    private NNConstraint GetConstraint()
    {
        NNConstraint constraint = NNConstraint.Default;

        constraint.constrainWalkability = true;
        constraint.walkable = true;

        constraint.constrainTags = true;

        // Allow only tag 0
        constraint.tags = 1 << 0;

        return constraint;
    }
    
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
        NNConstraint constraint = GetConstraint();
        
        GraphNode startNode = AstarPath.active.GetNearest(ai.position, constraint).node;

        for (int i = 0; i < 20; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(random2D.x, 0, random2D.y) + ai.position;

            var nearest = AstarPath.active.GetNearest(
                randomPoint,
                constraint
            );

            GraphNode targetNode = nearest.node;
            
            if (targetNode == null)
                continue;
            
            if (targetNode == startNode)
                continue;
            
            /*Vector3 finalPos = (Vector3)targetNode.position;
            
            if (Vector3.Distance(ai.position, finalPos) < 2f)
                continue;*/
            
            if (PathUtilities.IsPathPossible(startNode, targetNode))
            {
                return (Vector3)targetNode.position;
            }
        }

        // fallback if nothing valid found
        Debug.LogWarning($"{gameObject.name}: FAILED TO FIND VALID TARGET");
        return ai.position;
    }
    
    public Vector3 PickRandomPoint(Vector3 target)
    {
        NNConstraint constraint = GetConstraint();
        
        GraphNode startNode = AstarPath.active.GetNearest(ai.position).node;

        for (int i = 0; i < 20; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(random2D.x, 0, random2D.y) + target;

            var nearest = AstarPath.active.GetNearest(
                randomPoint,
                constraint
            );

            GraphNode targetNode = nearest.node;

            if (targetNode == null)
                continue;
            
            if (targetNode == startNode)
                continue;
            
            Vector3 finalPos = (Vector3)targetNode.position;

            if (Vector3.Distance(ai.position, finalPos) < 2f)
                continue;

            if (PathUtilities.IsPathPossible(startNode, targetNode))
            {
                return (Vector3)targetNode.position;
            }
        }

        // fallback if nothing valid found
        Debug.LogWarning($"{gameObject.name}: FAILED TO FIND VALID TARGET");
        return ai.position;
    }
    
    public void ChangeSpeed(float newSpeed, float duration)
    {
        if (_speedCoroutine != null)
        {
            StopCoroutine(_speedCoroutine);
        }
        
        _speedCoroutine = StartCoroutine(ChangeSpeedCoroutine(newSpeed, duration));
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
