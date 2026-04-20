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
    
    [Header("Dependencies")]
    public StateMachine frogStateMachine;
    public CreatureStatusVisualizer statusVisualizer;
    
    [Header("State")]
    public FrogState frogState;
    
    [Header("General settings")]
    public bool canWalk = true;
    public bool canSpit = true;
    public bool canRunaway = true;
    
    [Header("SpitParticle")]
    [SerializeField] private ParticleSystem spitParticle;
    
    [Header("Run away setting")]
    public int maxPlayers;
    public float runDistance = 10f;
    
    [Header("PickedUp settings")]
    [AllowMutableSyncType] public SyncVar<bool> pickedUp;
    
    [Header("Spit Time Range")]
    public Vector2 spitRange;
    public float spitPercentWarning = .2f;
    public float spitTimeRegen = 2f;

    [Header("Sounds")] 
    public EventReference spitSound;
    public EventReference warningSound;
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
            frogStateMachine.ChangeState(new FrogRoamState(frogStateMachine, this));
        }
    }
    
    private void Update()
    {
        if (!IsServerInitialized) return;

        ai.maxSpeed = running ? runSpeed : walkSpeed;
        
        animator.Animator.SetFloat("Speed", ai.velocity.magnitude);
        animator.Animator.SetBool("Running", running);
    }
    
    #region HelperMethods
    
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
            frogStateMachine.ChangeState(new FrogStunState(frogStateMachine, this, duration));
        }
    }
}

public enum FrogState
{
    Roaming,
    Running,
    PickedUp,
    Stunned
}
