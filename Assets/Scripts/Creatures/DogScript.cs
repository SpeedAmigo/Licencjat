using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items;
using UnityEngine;

public class DogScript : BaseEnemyScript
{
    [Header("Players in range list")]
    [AllowMutableSyncType] public SyncList<GameObject> itemOfInterestInRange = new();
    
    [Header("General settings")]
    public bool canWalk = true;

    [SerializeField] private float stopDistance = 1.5f;
    
    private DogItemOfInterest _itemOfInterest;
    private bool _itemOfInterestIsHeld;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (canWalk)
        {
            ai.destination = PickRandomPoint();
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        ai.maxSpeed = running ? runSpeed : walkSpeed;

        if (itemOfInterestInRange.Count > 0)
        {
            if (!_itemOfInterestIsHeld)
            {
                Roam(true);
            }
            else
            {
                FollowTarget(itemOfInterestInRange[0].transform.position);
            }
        }
        else
        {
            Roam(false);
        }
    }

    private void Roam(bool hasTarget)
    {
        running = false;
            
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath) && !WaitingForPath)
        {
            if (!canWalk) return;
            
            WaitingForPath = true;

            Invoke(hasTarget ? nameof(NewPathWrapper) : nameof(SetNewPath), 3f);
        }
    }

    private void FollowTarget(Vector3 target)
    {
        Vector3 direction = transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * stopDistance;
        offsetPosition.y = transform.position.y;

        ai.destination = offsetPosition;
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
        
        itemOfInterestInRange.Remove(obj);
        _itemOfInterest.ItemPickedUp -= OnItemOfInterestPickedUp;
        _itemOfInterest.ItemDropped -= OnItemOfInterestDropped;
        _itemOfInterest = null;
    }

    #endregion

    private void OnItemOfInterestPickedUp()
    {
        _itemOfInterestIsHeld = true;
    }

    private void OnItemOfInterestDropped()
    {
        _itemOfInterestIsHeld = false;
    }
}
