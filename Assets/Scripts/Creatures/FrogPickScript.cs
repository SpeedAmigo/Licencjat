using FishNet.CodeGenerating;
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
    
    protected override void PickupLogic(NetworkObject holder)
    {
        base.PickupLogic(holder);

        frogScript.AI.enabled = false;
        frogScript.Running = false;
        ChangePickupValue(true);
        
        _playerRoot = holder.transform.root.gameObject.GetComponent<PlayerRoot>();
    }
    
    protected override void DropLogic()
    {
        base.DropLogic();
        
        frogScript.AI.enabled = true;
        frogScript.Running = true;
        ChangePickupValue(false);
        
        _playerRoot = null;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChangePickupValue(bool value)
    {
        frogScript.pickedUp.Value = value;
    }

    private void Update()
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
                
                if (_playerRoot != null)
                {
                    _playerRoot.TakeDamage(frogScript.damage);
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
