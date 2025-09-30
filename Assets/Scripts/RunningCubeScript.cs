using System.Collections.Generic;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AIPath))]
public class RunningCubeScript : NetworkBehaviour
{
    public bool canWalk = true;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float runDistance = 10f;
    [SerializeField] private float range = 10f;

    private AIPath _ai;
    private bool _waitingForPath;
    private bool _running;

    // Server-side list of players inside range
    private readonly List<Collider> _playersInRange = new List<Collider>();

    private void Awake()
    {
        _ai = GetComponent<AIPath>();

        // Configure sphere trigger for detection
        SphereCollider sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = detectionRadius;
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

        Debug.Log($"[SERVER] Running away from {player.name} to {runTarget}");
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
        var player = obj.GetComponent<Collider>();
        
        if (!_playersInRange.Contains(player))
        {
            _playersInRange.Add(player);
            Debug.Log($"[SERVER] Player added to list: {player.name}");
            ObserverTest();
        }
    }

    [ObserversRpc]
    private void ObserverTest()
    {
        Debug.Log($"[SERVER] Observer Test");
    }
    
    // Trigger events (server only)
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Client] OnTriggerEnter: {other.name}");
        
        //if (!IsServer) return;

        if (other.CompareTag("Player") && !_playersInRange.Contains(other))
        {
            AddPlayerToServerList(other.gameObject);
            Debug.Log($"[SERVER] Player entered range: {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") && _playersInRange.Contains(other))
        {
            _playersInRange.Remove(other);
            Debug.Log($"[SERVER] Player left range: {other.name}");
        }
    }
}
