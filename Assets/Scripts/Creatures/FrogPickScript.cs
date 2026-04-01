using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using FMODUnity;
using Items;
using UnityEngine;

public class FrogPickScript : Item
{
    [AllowMutableSyncType] private SyncVar<float> spitTime = new();
    
    [SerializeField] private FrogScript frogScript;
    
    private PlayerRoot _playerRoot;
    private StatusEffectHandler _playerEffectHandler;
    private float _pickedTime;
    private bool _warningPlayed;

    public override void OnStartServer()
    {
        if (IsServerInitialized)
        {
            _pickedTime = frogScript.GetRandomSpitTime();
            spitTime.Value = _pickedTime;
        } 
    }
    
    protected override void PickupLogic(NetworkObject holder , NetworkConnection conn)
    {
        base.PickupLogic(holder, conn);
        
        frogScript.statusVisualizer.ShowStatusSign(CreatureStatus.Questionmark, 1.5f);

        frogScript.AI.enabled = false;
        frogScript.Running = false;
        ChangePickupValue(true);
        
        _playerRoot = holder.transform.root.gameObject.GetComponent<PlayerRoot>();
        _playerEffectHandler = holder.transform.root.gameObject.GetComponent<StatusEffectHandler>();
        
    }
    
    protected override void DropLogic(Vector3 forward)
    {
        base.DropLogic(forward);
        
        frogScript.AI.enabled = true;
        frogScript.Running = true;
        ChangePickupValue(false);
        
        _playerRoot = null;
        _playerEffectHandler = null;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChangePickupValue(bool value)
    {
        frogScript.pickedUp.Value = value;
    }

    protected override void Update()
    {
        if (!IsServerInitialized) return;
        
        if (frogScript.canSpit && frogScript.pickedUp.Value)
        {
            spitTime.Value -= Time.deltaTime;

            if (!_warningPlayed && spitTime.Value <= _pickedTime * frogScript.spitPercentWarning)
            {
                _warningPlayed = true;
                EventInstance spitSoundInstance = RuntimeManager.CreateInstance(frogScript.panicSound);
                spitSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                spitSoundInstance.start();
                spitSoundInstance.release();
            }
            
            if (spitTime.Value <= 0f)
            {
                _warningPlayed = false;
                _pickedTime = frogScript.GetRandomSpitTime();
                spitTime.Value = _pickedTime;
                frogScript.PlaySpitAnimation();

                EventInstance spitSoundInstance = RuntimeManager.CreateInstance(frogScript.spitSound);
                spitSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                spitSoundInstance.start();
                spitSoundInstance.release();

                frogScript.PlayParticleServer();
                
                if (_playerRoot && _playerEffectHandler)
                {
                    _playerEffectHandler.ApplyEffect(frogScript.damageEffect);
                    _playerRoot.RequestItemDrop(this);
                }
                else
                {
                    Debug.Log("Frog tried to spit on you but failed");
                }
            }
        }
        else if (!frogScript.pickedUp.Value)
        {
            if (spitTime.Value < _pickedTime)
            {
                spitTime.Value += Time.deltaTime * frogScript.spitTimeRegen;
            }
        }
    }
}
