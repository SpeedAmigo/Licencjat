using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using FMODUnity;
using Pathfinding;
using UnityEngine;

public class FrogScript : BaseEnemyScript, IStunable
{
    #region Variables
    
    public CreatureStatusVisualizer statusVisualizer;
    
    [Header("General settings")]
    public bool canWalk = true;
    public bool canSpit = true;
    public bool canRunaway = true;
    
    [Header("SpitParticle")]
    [SerializeField] private ParticleSystem spitParticle;
    
    [Header("Run away setting")]
    [SerializeField] private int maxPlayers;
    [SerializeField] private float runDistance = 10f;
    
    [Header("PickedUp settings")]
    [AllowMutableSyncType] public SyncVar<bool> pickedUp;
    
    [Header("Spit Time Range")]
    public Vector2 spitRange;
    public float spitPercentWarning = .2f;
    public float spitTimeRegen = 2f;

    [Header("Sounds")] 
    public EventReference spitSound;
    public EventReference waringSound;
    public EventReference panicSound;
    public EventReference idleSound;
    
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
        if (!IsServerInitialized) return;

        ai.maxSpeed = running ? runSpeed : walkSpeed;
        
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
        animator.Animator.SetBool("Running", running);
    }
    
    [Server]
    private void RunMethod()
    {
        if (pickedUp.Value) return;
        
        running = true;
        CancelInvoke(nameof(SetNewPath));

        var target = playersInRange[0];
        if (target != null)
        {
            SetRunningPath(target.transform, runDistance);
        }

        waitingForPath = false;
    }

    [Server]
    private void WalkMethod()
    {
        if (pickedUp.Value) return;
        
        running = false;
            
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath) && !waitingForPath)
        {
            if (!canWalk) return;
                
            waitingForPath = true;
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

    [Server]
    public float GetRandomSpitTime()
    {
        return Random.Range(spitRange.x, spitRange.y);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayParticleServer()
    {
        PlayParticleObservers();
    }

    [ObserversRpc]
    private void PlayParticleObservers()
    {
        spitParticle.Play();
    }
    
    #endregion
    
    public void SetStunned(bool stunned, float duration)
    {
        if (stunned)
        {
            statusVisualizer.ShowStatusSign(CreatureStatus.Star, duration);
            Debug.Log("Stunned");
            canWalk = false;
            walkSpeed = 0;
            canRunaway = false;
            canSpit = false;
        }
        else
        {
            Debug.Log("Not Stunned");
            
            canWalk = true;
            walkSpeed = 2f;
            canRunaway = true;
            canSpit = true;
        }
    }

    protected override void OnDetected(Collider other)
    {
        base.OnDetected(other);
        
        if (other.CompareTag("Player") && !playersInRange.Contains(other.gameObject) && statusVisualizer != null)
        {
            statusVisualizer.ShowStatusSign(CreatureStatus.Exclamation, 2f);
        }
    }
}
