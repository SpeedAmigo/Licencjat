using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using RaycastPro.Detectors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIPath))]
public class FrogScript : ObjectPickable
{
    #region Variables

    [Header("Dependencies")]
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private RangeDetector rangeDetector;
    
    [Header("General settings")]
    public bool canWalk = true;
    public bool canSpit = true;
    public bool canRunaway = true;
    
    [Header("Speed settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    
    [Header("Run away setting")]
    [SerializeField] private int maxPlayers;
    [SerializeField] private float runDistance = 10f;
    [SerializeField] private float range = 10f;
    
    [Header("PickedUp settings")]
    [AllowMutableSyncType] private SyncVar<bool> pickedUp;
    [AllowMutableSyncType] private SyncVar<float> spitTime = new(5f);
    
    [Header("Players in range list")]
    // Server-side list of players inside range
    public List<GameObject> playersInRange = new();

    private AIPath _ai;
    private bool _waitingForPath;
    private bool _running;
    
    private PlayerInventoryScript _playerInventory;
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
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

        _ai.maxSpeed = _running ? runSpeed : walkSpeed;
        
        bool canRun = canRunaway && playersInRange.Count > maxPlayers;
        
        if (canSpit && pickedUp.Value)
        {
            spitTime.Value -= Time.deltaTime;
            if (spitTime.Value <= 0f)
            {
                Debug.Log("Frog Spitted on you");
                spitTime.Value = 5f;
                animator.Animator.Play("Spit");
                _playerInventory.RequestRemoveItem(this, _playerInventory);
            }
        }

        if (canRun)
        {
            RunMethod();
        }
        else
        {
            WalkMethod();
        }
        
        animator.Animator.SetFloat("Speed", _ai.velocity.magnitude);
        animator.Animator.SetBool("Running", _running);
    }
    
    [Server]
    private void RunMethod()
    {
        if (pickedUp.Value) return;
        
        _running = true;
        CancelInvoke(nameof(SetNewPath));

        var target = playersInRange[0];
        if (target != null)
        {
            SetRunningPath(target.transform, runDistance);
        }

        _waitingForPath = false;
    }

    [Server]
    private void WalkMethod()
    {
        if (pickedUp.Value) return;
        
        _running = false;
            
        if (!_ai.pathPending && (_ai.reachedEndOfPath || !_ai.hasPath) && !_waitingForPath)
        {
            if (!canWalk) return;
                
            _waitingForPath = true;
            Invoke(nameof(SetNewPath), 3f);
        }
    }
    
    #region PickUpRegion
    protected override void PickupLogic(NetworkObject holder)
    {
        base.PickupLogic(holder);

        _ai.enabled = false;
        _running = false;
        ChangePickupValue(true);
        
        _playerInventory = holder.transform.root.gameObject.GetComponent<PlayerInventoryScript>();
    }

    protected override void DropLogic()
    {
        base.DropLogic();
        
        _ai.enabled = true;
        _running = true;
        ChangePickupValue(false);
        
        _playerInventory = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePickupValue(bool value)
    {
        pickedUp.Value = value;
    }
    
    #endregion

    #region HelperMethods
    
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
    
    #endregion
    
    #region PlayerDetection

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToServerList(GameObject obj)
    {
        if (playersInRange.Contains(obj)) return;
        
        playersInRange.Add(obj);
        //Debug.Log($"[SERVER] Player added to list: {obj.name}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromServerList(GameObject obj)
    {
        if (!playersInRange.Contains(obj)) return;
        
        playersInRange.Remove(obj);
        //Debug.Log($"[SERVER] Player removed from list: {obj.name}");
    }
    
    private void OnDetected(Collider other)
    {
        if (other.CompareTag("Player") && !playersInRange.Contains(other.gameObject))
        {
            playersInRange.Add(other.gameObject);
            AddPlayerToServerList(other.gameObject);
            //Debug.Log($"[SERVER] Player entered range: {other.name}");
        }
    }

    private void OnLost(Collider other)
    {
        if (other.CompareTag("Player") && playersInRange.Contains(other.gameObject))
        {
            playersInRange.Remove(other.gameObject);
            RemovePlayerFromServerList(other.gameObject);
            //Debug.Log($"[SERVER] Player left range: {other.name}");
        }
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
