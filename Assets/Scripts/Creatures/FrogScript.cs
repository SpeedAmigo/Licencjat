using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Pathfinding;
using UnityEngine;

public class FrogScript : BaseEnemyScript
{
    #region Variables
    
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
    
    [Header("PickedUp settings")]
    [AllowMutableSyncType] public SyncVar<bool> pickedUp;
    //[AllowMutableSyncType] private SyncVar<float> spitTime = new(5f);
    
    private bool _running;

    public bool Running
    {
        get => _running;
        set => _running = value;
    }
    
    public AIPath AI
    {
        get => ai;
        set => ai = value;
    }
    
    #endregion
    
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
        if (!IsServerInitialized) return; // only server runs logic

        ai.maxSpeed = _running ? runSpeed : walkSpeed;
        
        bool canRun = canRunaway && playersInRange.Count > maxPlayers;
        
        if (canRun)
        {
            RunMethod();
        }
        else
        {
            WalkMethod();
        }
        
        animator.Animator.SetFloat("Speed", ai.velocity.magnitude);
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

        WaitingForPath = false;
    }

    [Server]
    private void WalkMethod()
    {
        if (pickedUp.Value) return;
        
        _running = false;
            
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath) && !WaitingForPath)
        {
            if (!canWalk) return;
                
            WaitingForPath = true;
            Invoke(nameof(SetNewPath), 3f);
        }
    }
    
    #region HelperMethods
    
    private void SetRunningPath(Transform player, float runDistance)
    {
        Vector3 direction = (ai.position - player.position).normalized;
        Vector3 runTarget = ai.position + direction * runDistance;
        runTarget.y = ai.position.y;
        
        ai.destination = runTarget;
    }
    
    public void PlaySpitAnimation()
    {
        animator.Animator.Play("Spit");
    }
    
    #endregion
}
