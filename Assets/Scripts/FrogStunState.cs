using FMODUnity;
using UnityEngine;

public class FrogStunState : State
{
    FrogScript _frogScript;
    
    private readonly float _duration;
    private float _currentTime = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FrogStunState(StateMachine machine, FrogScript frogScript, float duration) : base(machine)
    {
        _frogScript = frogScript;
        _duration = duration;
    }

    public override void Enter()
    {
        Debug.Log("Entering Frog Stun State");
        _frogScript.frogState = FrogState.Stunned;
        
        _frogScript.statusVisualizer.ShowStatusSign(CreatureStatus.Star, _duration);
        SoundCreator.Instance.PlayOneShot(_frogScript.stunSound, _frogScript.transform.position);
        
        _frogScript.Animator.Animator.SetBool("IsHeld", true);
        _frogScript.ai.isStopped = true;
    }

    public override void Tick()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime >= _duration)
        {
            stateMachine.ChangeState(new FrogRoamState(stateMachine, _frogScript));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Frog Stun State");
        _frogScript.Animator.Animator.SetBool("IsHeld", false);
        _frogScript.ai.isStopped = false;
    }
}
