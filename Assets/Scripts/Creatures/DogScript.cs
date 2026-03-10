using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items;
using UnityEngine;

public class DogScript : BaseEnemyScript
{
    [Header("State")]
    public DogState dogState;
    
    [Header("Dependencies")]
    [SerializeField] private StateMachine dogStateMachine;
    
    [Header("Items in range list")]
    [AllowMutableSyncType] private readonly SyncVar<GameObject> _itemOfInterestInRange = new();
    
    [Header("General settings")]
    public bool canWalk = true;
    public GameObject targetPlayer;
    public float agroTime = 5f;
    public float agroTimer;
    public float attackTimer;
    
    [Header("Distance Settings")]
    public float stopDistance = 1.5f;
    public float agroDistance = 10f;
    public float attackDistance;
    public float maxAwareDistance;
    
    [HideInInspector] public bool itemOfInterestIsHeld;
    [HideInInspector] public DogItemOfInterest itemOfInterest;
    
    public NetworkObject holdingPlayer;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        ai.repathRate = 0.2f;
        
        if (canWalk)
        {
            dogStateMachine.ChangeState(new DogRoamState(dogStateMachine, this));
            
            
            //ai.destination = PickRandomPoint();
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        ai.maxSpeed = running ? runSpeed : walkSpeed;
        
        /*switch (dogState)
        {
            case DogState.Idle:
                HandleIdle();
                break;

            case DogState.Roam:
                HandleRoam();
                break;

            case DogState.FollowTarget:
                HandleFollow();
                break;

            case DogState.MoveToAttack:
                HandleMoveToAttack();
                break;

            case DogState.Attack:
                HandleAttack();
                break;
        }*/
    }

    /*private void HandleAttack()
    {
        Attack();

        dogState = DogState.Roam;
    }*/

    /*private void HandleMoveToAttack()
    {
        if (targetPlayer == null)
        {
            dogState = DogState.Roam;
            return;
        }
        
        if (itemOfInterestInRange.Count > 0 && itemOfInterestIsHeld)
        {
            dogState = DogState.FollowTarget;
            return;
        }
        
        MoveToAttack();
    }*/
    
    /*private void HandleFollow()
    {
        if (itemOfInterestInRange.Count == 0)
        {
            dogState = DogState.Roam;
            return;
        }

        if (!itemOfInterestIsHeld && playersInRange.Count > 0 && Vector3.Distance(itemOfInterestInRange[0].transform.position,
                playersInRange[0].transform.position) <= agroDistance)
        {
            dogState = DogState.MoveToAttack;
            return;
        }
        
        if (!itemOfInterestIsHeld && playersInRange.Count > 0 && Vector3.Distance(itemOfInterestInRange[0].transform.position,
                playersInRange[0].transform.position) >= agroDistance)
        {
            dogState = DogState.Roam;
            return;
        }

        if (!itemOfInterestIsHeld && playersInRange.Count <= 0)
        {
            dogState = DogState.Roam;
            return;
        }

        if (ai.reachedEndOfPath)
        {
            dogState = DogState.Roam;
            return;
        }

        FollowTarget(itemOfInterestInRange[0].transform.position);
    }*/

    /*private void HandleRoam()
    {
        running = false;

        if (playersInRange.Count > 0 && itemOfInterestInRange.Count > 0 && !itemOfInterestIsHeld)
        {
            foreach (var player in playersInRange)
            {
                if (Vector3.Distance(_itemOfInterest.transform.position, player.transform.position) <= agroDistance)
                {
                    targetPlayer = player.gameObject;
                    dogState = DogState.MoveToAttack;
                    return;
                }
            }
        }

        if (itemOfInterestInRange.Count > 0 && itemOfInterestIsHeld)
        {
            dogState = DogState.FollowTarget;
            return;
        }
        
        if (itemOfInterestInRange.Count > 0 && !itemOfInterestIsHeld)
        {
            Roam(true);
            return;
        }

        Roam(false);
    }*/
    
    /*private void Roam(bool hasTarget)
    {
        Debug.Log("Roam");
        
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath) && !WaitingForPath)
        {
            if (!canWalk) return;
            
            WaitingForPath = true;

            Invoke(hasTarget ? nameof(NewPathWrapper) : nameof(SetNewPath), 3f);
        }
    }*/

    /*private void FollowTarget(Vector3 target)
    {
        Debug.Log("FollowTarget");
        
        Vector3 direction = transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * stopDistance;
        offsetPosition.y = transform.position.y;
        
        ai.destination = offsetPosition;
    }*/
    
    /*private void MoveToAttack()
    {
        var target = targetPlayer.transform.position;

        Vector3 direction = transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * (attackDistance - 1f);
        offsetPosition.y = transform.position.y;

        ai.destination = offsetPosition;
        
        if (Vector3.Distance(transform.position, target) <= attackDistance)
        {
            Debug.Log("Attack");
            dogState = DogState.Attack;
        }
    }*/
    
    /*private void Attack()
    {
        if (playersInRange.Count <= 0) return;
        
        Debug.Log("Dog has attacked!");
    }*/

    #region HelperMethods

    public void NewPathWrapper()
    {
        SetNewPath(_itemOfInterestInRange.Value.transform.position);
    }

    #endregion
    
    #region itemOfIntrestRegion
    
    protected override void OnDetected(Collider other)
    {
        base.OnDetected(other);
        
        if (other.CompareTag("ItemOfInterest") && _itemOfInterestInRange.Value == null)
        {
            AddItemToServerList(other.gameObject);
        }
    }

    protected override void OnLost(Collider other)
    {
        base.OnLost(other);
        
        targetPlayer = null;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddItemToServerList(GameObject obj)
    {
        if (_itemOfInterestInRange.Value != null) return;

        _itemOfInterestInRange.Value = obj;
        itemOfInterest = obj.GetComponent<DogItemOfInterest>();
        itemOfInterest.HoldingPlayer += OnHoldingPlayer;
        itemOfInterest.ItemPickedUp += OnItemOfInterestPickedUp;
        itemOfInterest.ItemDropped += OnItemOfInterestDropped;
    }

    private void OnHoldingPlayer(NetworkObject obj)
    {
        holdingPlayer = obj;
    }

    public override void OnDisable()
    {
        if (itemOfInterest)
        {
            itemOfInterest.ItemPickedUp -= OnItemOfInterestPickedUp;
            itemOfInterest.ItemDropped -= OnItemOfInterestDropped;
            itemOfInterest.HoldingPlayer -= OnHoldingPlayer;
        }
    }
    
    private void OnItemOfInterestPickedUp()
    {
        itemOfInterestIsHeld = true;
    }

    private void OnItemOfInterestDropped()
    {
        itemOfInterestIsHeld = false;
        holdingPlayer = null;
    }

    #endregion
}

public enum DogState
{
    Roam,
    FollowTarget,
    MoveToAttack,
    Attack
}
