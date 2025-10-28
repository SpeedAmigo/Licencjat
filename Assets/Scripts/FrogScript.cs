using System.Collections.Generic;
using FishNet.Object;
using Pathfinding;
using RaycastPro.Detectors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIPath))]
public class FrogScript : NetworkBehaviour
{
    [SerializeField] private RangeDetector rangeDetector;
    
    public bool canWalk = true;
    [SerializeField] private float runDistance = 10f;
    [SerializeField] private float range = 10f;

    private AIPath _ai;
    private bool _waitingForPath;
    private bool _running;

    // Server-side list of players inside range
    private readonly List<GameObject> _playersInRange = new();

    private void Awake()
    {
        _ai = GetComponent<AIPath>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (canWalk)
        {
            _ai.destination = PickRandomPoint();
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return; // only server runs logic

        _ai.maxSpeed = _running ? 5f : 2f;

        if (_playersInRange.Count > 0) // someone nearby
        {
            _running = true;
            CancelInvoke(nameof(SetNewPath));

            var target = _playersInRange[0];
            if (target != null)
            {
                SetRunningPath(target.transform, runDistance);
            }

            _waitingForPath = false;
        }
        else if (!_ai.pathPending && (_ai.reachedEndOfPath || !_ai.hasPath) && !_waitingForPath)
        {
            if (!canWalk) return;

            _running = false;
            _waitingForPath = true;
            Invoke(nameof(SetNewPath), 3f);
        }
    }

    private void SetNewPath()
    {
        _ai.destination = PickRandomPoint();
        _waitingForPath = false;
    }

    private void SetRunningPath(Transform player, float runDistance)
    {
        Vector3 direction = (_ai.position - player.position).normalized;
        Vector3 runTarget = _ai.position + direction * runDistance;
        runTarget.y = _ai.position.y;
        
        _ai.destination = runTarget;
    }

    private Vector3 PickRandomPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * range;
        randomPoint.y = 0;
        randomPoint += _ai.position;
        return randomPoint;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToServerList(GameObject obj)
    {
        if (_playersInRange.Contains(obj)) return;
        
        _playersInRange.Add(obj);
        //Debug.Log($"[SERVER] Player added to list: {obj.name}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromServerList(GameObject obj)
    {
        if (!_playersInRange.Contains(obj)) return;
        
        _playersInRange.Remove(obj);
        //Debug.Log($"[SERVER] Player removed from list: {obj.name}");
    }
    
    private void OnDetected(Collider other)
    {
        if (other.CompareTag("Player") && !_playersInRange.Contains(other.gameObject))
        {
            _playersInRange.Add(other.gameObject);
            AddPlayerToServerList(other.gameObject);
            //Debug.Log($"[SERVER] Player entered range: {other.name}");
        }
    }

    private void OnLost(Collider other)
    {
        if (other.CompareTag("Player") && _playersInRange.Contains(other.gameObject))
        {
            _playersInRange.Remove(other.gameObject);
            RemovePlayerFromServerList(other.gameObject);
            //Debug.Log($"[SERVER] Player left range: {other.name}");
        }
    }

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
}
