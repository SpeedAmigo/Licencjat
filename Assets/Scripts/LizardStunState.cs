using UnityEngine;

public class LizardStunState : State
{
    private LizardScript _lizardScript;
    
    private readonly float _duration;
    private float _currentTime = 0f;
    
    public LizardStunState(StateMachine machine, LizardScript lizardScript, float duration) : base(machine)
    {
        _lizardScript = lizardScript;
        _duration = duration;
    }

    public override void Enter()
    {
        Debug.Log("Entering Lizard Stun State");
        
        _lizardScript.lizardVisualizer.ShowStatusSign(CreatureStatus.Star, _duration);
        _lizardScript.ai.isStopped = true;
        _lizardScript.Animator.Animator.speed = 0f;
    }

    public override void Tick()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime >= _duration)
        {
            stateMachine.ChangeState(new LizardRoamState(stateMachine, _lizardScript));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Lizard Stun State");
        _lizardScript.ai.isStopped = false;
        _lizardScript.Animator.Animator.speed = 1f;
    }
}
