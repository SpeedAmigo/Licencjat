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
        
        _frogPickScript.HandleNavAgent(false);
    }

    public override void Tick()
    {
        if (!_frogScript.canSpit) return;
        
        _frogPickScript.spitTime.Value -= Time.deltaTime;

        if (!_warningPlayed && _frogPickScript.spitTime.Value <= _frogPickScript.pickedTime * _frogScript.spitPercentWarning)
        {
            _warningPlayed = true;
            SoundCreator.Instance.PlayOneShotAttached(_frogScript.panicSound, _frogScript.gameObject);
        }
            
        if (_frogPickScript.spitTime.Value <= 0f)
        {
            _warningPlayed = false;
            _frogPickScript.pickedTime = _frogScript.GetRandomSpitTime();
            _frogPickScript.spitTime.Value = _frogPickScript.pickedTime;
            _frogScript.PlaySpitAnimation();
            
            SoundCreator.Instance.PlayOneShot(_frogScript.spitSound, _frogScript.transform.position);

            _frogScript.PlayParticleServer();
                
            if (_frogPickScript.playerRoot && _frogPickScript.playerEffectHandler)
            {
                _frogPickScript.playerEffectHandler.ApplyEffects(_frogScript.damageEffects);
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
        
        _frogPickScript.HandleNavAgent(true);
    }
}
