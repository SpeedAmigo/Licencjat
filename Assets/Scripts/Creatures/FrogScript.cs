using FishNet.CodeGenerating;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIPath))]
public class FrogScript : BaseEnemyScript
{
    #region Variables

    [Header("Dependencies")]
    [SerializeField] private NetworkAnimator animator;
    
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
    [AllowMutableSyncType] public SyncVar<bool> pickedUp;
    //[AllowMutableSyncType] private SyncVar<float> spitTime = new(5f);
    
    private AIPath _ai;
    private bool _waitingForPath;
    private bool _running;

    public bool Running
    {
        get => _running;
        set => _running = value;
    }
    
    public AIPath AI
    {
        get => _ai;
        set => _ai = value;
    }

    /*public PlayerInventoryScript PlayerInventory
    {
        get => _playerInventory;
        set => _playerInventory = value;
    }
    
    private PlayerInventoryScript _playerInventory;*/
    
    #endregion
    
    /*protected override void Awake()
    {
        base.Awake();
        _ai = GetComponent<AIPath>();
    }*/

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

        _ai.maxSpeed = _running ? runSpeed : walkSpeed;
        
        bool canRun = canRunaway && playersInRange.Count > maxPlayers;
        
        /*if (canSpit && pickedUp.Value)
        {
            spitTime.Value -= Time.deltaTime;
            if (spitTime.Value <= 0f)
            {
                Debug.Log("Frog Spitted on you");
                spitTime.Value = 5f;
                animator.Animator.Play("Spit");
                _playerInventory.RequestRemoveItem(this, _playerInventory);
            }
        }*/

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
    
    /*#region PickUpRegion
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
    
    #endregion*/

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

    public void PlaySpitAnimation()
    {
        animator.Animator.Play("Spit");
    }
    
    #endregion
}
