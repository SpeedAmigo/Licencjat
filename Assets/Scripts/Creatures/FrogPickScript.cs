using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using FMODUnity;
using Items;
using UnityEngine;

public class FrogPickScript : Item
{
    [AllowMutableSyncType] public SyncVar<float> spitTime = new();
    
    [SerializeField] private FrogScript frogScript;
    
    public PlayerRoot playerRoot;
    public StatusEffectHandler playerEffectHandler;
    public float pickedTime;
    private bool _warningPlayed;

    public override void OnStartServer()
    {
        if (IsServerInitialized)
        {
            pickedTime = frogScript.GetRandomSpitTime();
            spitTime.Value = pickedTime;
        } 
    }

    public override void Pickup(NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        base.Pickup(fpHolder, tpHolder, conn);
        
        ChangePickupValue(true);
        
        playerRoot = fpHolder.transform.root.gameObject.GetComponent<PlayerRoot>();
        playerEffectHandler = fpHolder.transform.root.gameObject.GetComponent<StatusEffectHandler>();

        if (IsServerInitialized)
        {
            frogScript.frogStateMachine.ChangeState(new FrogPickedUpState(frogScript.frogStateMachine, frogScript, this));
        }
    }

    [ObserversRpc]
    public void HandleNavAgent(bool enable)
    {
        frogScript.ai.enabled = enable;
        frogScript.running = enable;
    }
    
    protected override void DropLogic(Vector3 forward)
    {
        base.DropLogic(forward);
        
        ChangePickupValue(false);
        
        playerRoot = null;
        playerEffectHandler = null;

        if (IsServerInitialized)
        {
            frogScript.frogStateMachine.ChangeState(new FrogRoamState(frogScript.frogStateMachine, frogScript));
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChangePickupValue(bool value)
    {
        frogScript.pickedUp.Value = value;
    }

    protected override void Update()
    {
        if (!IsServerInitialized) return;
        
        if (!frogScript.pickedUp.Value)
        {
            if (spitTime.Value < pickedTime)
            {
                spitTime.Value += Time.deltaTime * frogScript.spitTimeRegen;
            }
        }
    }
}
