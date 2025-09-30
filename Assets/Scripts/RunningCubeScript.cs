using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using RaycastPro.Detectors;
using UnityEngine;
using Random = UnityEngine.Random;

public class RunningCubeScript : NetworkBehaviour
{
    public bool canWalk = true;
    
    public List<Collider> playersInRange;
    
    [SerializeField] private RangeDetector rangeDetector;
    [SerializeField] private int maxPlayers = 4;
    
    [SerializeField] private float range = 10f;
    
    private AIPath _ai;
    private bool _waitingForPath;
    private bool _running;

    private void Awake()
    {
        _ai = GetComponent<AIPath>();
        rangeDetector.SyncDetection(playersInRange, OnNewDetection, OnLostDetection);
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
        if (!IsServerInitialized) return;
        
        _ai.maxSpeed = _running ? 5 : 2;
        
        if (rangeDetector.DetectedColliders.Count > maxPlayers)
        {
            _running = true;
            
            CancelInvoke("SetNewPath");
            
            if (playersInRange != null) SetRunningPath(playersInRange[0].transform, 10f);
            _waitingForPath = false;
        }
        else if (!_ai.pathPending && (_ai.reachedEndOfPath || !_ai.hasPath) && !_waitingForPath)
        {
            if (!canWalk) return;
            if (!IsServerInitialized) return;
            
            _running = false;
            
            _waitingForPath = true;
            Invoke("SetNewPath", 3f);
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

    private void OnNewDetection(Collider collider) {}

    private void OnLostDetection(Collider collider) {}
}
