using UnityEngine;

public class LizardRoamState : State
{
    private LizardScript _lizardScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LizardRoamState(StateMachine machine, LizardScript lizardScript) : base(machine)
    {
        _lizardScript = lizardScript;
    }

    public override void Enter()
    {
        Debug.Log("Entered lizard roam state");
        _lizardScript.lizardState = LizardState.Roam;
        _lizardScript.ChangeSpeed(_lizardScript.walkSpeed, 0.5f);
        _lizardScript.SetNewPath();
        
        _lizardScript.ChangeLayerWeight(_lizardScript.attackLayer, 0f, 0.2f);
    }

    public override void Tick()
    {
        if (_lizardScript.VcInRange.Count != 0 && _lizardScript.GetLoudestVoiceAround().voiceVolume.Value >= _lizardScript.noiseThreshold)
        {
            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }

        if (_lizardScript.VcInRange.Count != 0 && _lizardScript.GetLoudestVoiceAround().voiceVolume.Value < _lizardScript.noiseThreshold)
        {
            stateMachine.ChangeState(new LizardAttackState(stateMachine, _lizardScript));
        }
        
        Roam();
    }

    public override void Exit()
    {
        Debug.Log("Exiting lizard roam state");
    }

    private void Roam()
    {
        if (_lizardScript.ai.velocity.magnitude > 0.1f)
        {
            _lizardScript.Animator.Animator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            _lizardScript.Animator.Animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }
        
        if (_lizardScript.ReachedDestination())
        {
            _lizardScript.waitingForPath = true;
            _lizardScript.Invoke(nameof(_lizardScript.SetNewPath), 3f);
        }
    }
}
