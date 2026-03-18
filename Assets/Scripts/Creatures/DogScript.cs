using System.Collections;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Items;
using Pathfinding;
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

    [HideInInspector] public bool startAgroTimer;
    
    private Coroutine _speedCoroutine;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        //ai.repathRate = 0.2f;
        
        if (canWalk)
        {
            dogStateMachine.ChangeState(new DogRoamState(dogStateMachine, this));
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        
        animator.Animator.SetFloat("Speed", ai.velocity.magnitude);
    }
    
    #region HelperMethods

    public void NewPathWrapper()
    {
        SetNewPath(_itemOfInterestInRange.Value.transform.position);
    }

    public void ChangeSpeed(float newSpeed, float duration)
    {
        if (_speedCoroutine != null)
        {
            StopCoroutine(_speedCoroutine);
        }
        
        StartCoroutine(ChangeSpeedCoroutine(newSpeed, duration));
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

        startAgroTimer = true;
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
