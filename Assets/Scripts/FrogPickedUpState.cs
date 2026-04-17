using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FrogPickedUpState : State
{
    private readonly FrogScript _frogScript;
    private readonly FrogPickScript _frogPickScript;

    private bool _warningPlayed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FrogPickedUpState(StateMachine machine, FrogScript frogScript, FrogPickScript frogPickScript) : base(machine)
    {
        _frogScript = frogScript;
        _frogPickScript = frogPickScript;
    }

    public override void Enter()
    {
        Debug.Log("FrogPickedUpState Enter");
        _frogScript.frogState = FrogState.PickedUp;
        _frogScript.statusVisualizer.ShowStatusSign(CreatureStatus.Questionmark, 1.5f);

        _frogScript.ai.enabled = false;
        _frogScript.running = false;
    }

    public override void Tick()
    {
        if (!_frogScript.canSpit) return;
        
        _frogPickScript.spitTime.Value -= Time.deltaTime;

        if (!_warningPlayed && _frogPickScript.spitTime.Value <= _frogPickScript.pickedTime * _frogScript.spitPercentWarning)
        {
            _warningPlayed = true;
            EventInstance spitSoundInstance = RuntimeManager.CreateInstance(_frogScript.panicSound);
            spitSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(_frogScript.transform.position));
            spitSoundInstance.start();
            spitSoundInstance.release();
        }
            
        if (_frogPickScript.spitTime.Value <= 0f)
        {
            _warningPlayed = false;
            _frogPickScript.pickedTime = _frogScript.GetRandomSpitTime();
            _frogPickScript.spitTime.Value = _frogPickScript.pickedTime;
            _frogScript.PlaySpitAnimation();

            EventInstance spitSoundInstance = RuntimeManager.CreateInstance(_frogScript.spitSound);
            spitSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(_frogScript.transform.position));
            spitSoundInstance.start();
            spitSoundInstance.release();

            _frogScript.PlayParticleServer();
                
            if (_frogPickScript.playerRoot && _frogPickScript.playerEffectHandler)
            {
                _frogPickScript.playerEffectHandler.ApplyEffect(_frogScript.damageEffect);
                _frogPickScript.playerRoot.RequestItemDrop(_frogPickScript);
            }
            else
            {
                Debug.Log("Frog tried to spit on you but failed");
            }
            
            stateMachine.ChangeState(new FrogRoamState(stateMachine, _frogScript));
        }
    }

    public override void Exit()
    {
        Debug.Log("FrogPickedUpState Exit");
        
        _frogScript.ai.enabled = true;
        _frogScript.running = true;
    }
}
