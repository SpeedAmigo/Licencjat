using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items;
using UnityEngine;

public class DogScript : BaseEnemyScript
{
    [SerializeField] private DogState dogState;
    
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> itemOfInterestInRange = new();
    
    [Header("General settings")]
    public bool canWalk = true;

    [SerializeField] private float stopDistance = 1.5f;
    
    private DogItemOfInterest _itemOfInterest;
    private bool _itemOfInterestIsHeld;

    [SerializeField] private GameObject targetPlayer;
    [SerializeField] private float agroDistance = 10f;
    [SerializeField] private float agroTime = 5f;
    [SerializeField] private float agroTimer;
    [SerializeField] private float attackDistance;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        ai.repathRate = 0.2f;
        
        if (canWalk)
        {
            ai.destination = PickRandomPoint();
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        ai.maxSpeed = running ? runSpeed : walkSpeed;
        
        switch (dogState)
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
        }

        /*if (itemOfInterestInRange.Count > 0)
        {
            if (!_itemOfInterestIsHeld && playersInRange.Count > 0 && Vector3.Distance(itemOfInterestInRange[0].transform.position, playersInRange[0].transform.position) <= agroDistance)
            {
                MoveToAttack();
            }
            
            if (!_itemOfInterestIsHeld && dogState == DogState.Roam)
            {
                Roam(true);
            }
            else if (dogState == DogState.FollowTarget)
            {
                FollowTarget(itemOfInterestInRange[0].transform.position);
            }
        }
        else
        {
            Roam(false);
        }*/
    }

    private void HandleAttack()
    {
        Attack();

        dogState = DogState.Roam;
    }

    private void HandleMoveToAttack()
    {
        if (targetPlayer == null)
        {
            dogState = DogState.Roam;
            return;
        }

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
    }

    private void HandleFollow()
    {
        if (itemOfInterestInRange.Count == 0)
        {
            dogState = DogState.Roam;
            return;
        }

        if (!_itemOfInterestIsHeld && playersInRange.Count > 0 && Vector3.Distance(itemOfInterestInRange[0].transform.position,
                playersInRange[0].transform.position) <= agroDistance)
        {
            dogState = DogState.MoveToAttack;
            return;
        }

        FollowTarget(itemOfInterestInRange[0].transform.position);
    }

    private void HandleRoam()
    {
        running = false;

        if (playersInRange.Count > 0 && itemOfInterestInRange.Count > 0 && !_itemOfInterestIsHeld)
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

        if (itemOfInterestInRange.Count > 0 && _itemOfInterestIsHeld)
        {
            dogState = DogState.FollowTarget;
            return;
        }
        
        if (itemOfInterestInRange.Count > 0 && !_itemOfInterestIsHeld)
        {
            Roam(true);
            return;
        }

        Roam(false);
    }

    private void HandleIdle()
    {
        throw new System.NotImplementedException();
    }

    private void Roam(bool hasTarget)
    {
        Debug.Log("Roam");
        
        //running = false;
            
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath) && !WaitingForPath)
        {
            if (!canWalk) return;
            
            WaitingForPath = true;

            Invoke(hasTarget ? nameof(NewPathWrapper) : nameof(SetNewPath), 3f);
        }
    }

    private void FollowTarget(Vector3 target)
    {
        Debug.Log("FollowTarget");
        
        Vector3 direction = transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * stopDistance;
        offsetPosition.y = transform.position.y;
        
        ai.destination = offsetPosition;
    }

    private void MoveToAttack()
    {
        Debug.Log("Move to attack");
        
        if (targetPlayer == null || playersInRange.Count == 0) return;
        
        dogState = DogState.MoveToAttack;
        CancelInvoke(_itemOfInterest ? nameof(NewPathWrapper) : nameof(SetNewPath));

        var target = targetPlayer.transform.position;
        
        Vector3 direction = transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * attackDistance;
        offsetPosition.y = transform.position.y;
        
        ai.destination = offsetPosition;

        if (Vector3.Distance(transform.position, target) <= attackDistance)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (playersInRange.Count <= 0) return;
        
        Debug.Log("Dog has attacked!");
    }

    #region HelperMethods

    private void NewPathWrapper()
    {
        SetNewPath(itemOfInterestInRange[0].transform.position);
    }

    #endregion
    
    #region itemOfIntrestRegion
    
    protected override void OnDetected(Collider other)
    {
        base.OnDetected(other);
        
        if (other.CompareTag("ItemOfInterest") && !itemOfInterestInRange.Contains(other.gameObject))
        {
            AddItemToServerList(other.gameObject);
        }
    }

    protected override void OnLost(Collider other)
    {
        base.OnLost(other);
        
        targetPlayer = null;
        
        if (other.CompareTag("ItemOfInterest") && itemOfInterestInRange.Contains(other.gameObject))
        {
            RemoveItemFromServerList(other.gameObject);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddItemToServerList(GameObject obj)
    {
        if (itemOfInterestInRange.Contains(obj)) return;
        
        itemOfInterestInRange.Add(obj);
        _itemOfInterest = obj.GetComponent<DogItemOfInterest>();
        _itemOfInterest.ItemPickedUp += OnItemOfInterestPickedUp;
        _itemOfInterest.ItemDropped += OnItemOfInterestDropped;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RemoveItemFromServerList(GameObject obj)
    {
        if (!itemOfInterestInRange.Contains(obj)) return;
        
        //itemOfInterestInRange.Remove(obj);
        _itemOfInterest.ItemPickedUp -= OnItemOfInterestPickedUp;
        _itemOfInterest.ItemDropped -= OnItemOfInterestDropped;
        //_itemOfInterest = null;
    }
    
    private void OnItemOfInterestPickedUp()
    {
        _itemOfInterestIsHeld = true;
    }

    private void OnItemOfInterestDropped()
    {
        _itemOfInterestIsHeld = false;
    }

    #endregion
}

public enum DogState
{
    Idle,
    Roam,
    FollowTarget,
    MoveToAttack,
    Attack
}
