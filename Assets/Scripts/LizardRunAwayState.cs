using Pathfinding;
using UnityEngine;

public class LizardRunAwayState : State
{
    private LizardScript _lizardScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LizardRunAwayState(StateMachine machine, LizardScript lizardScript) : base(machine)
    {
        _lizardScript = lizardScript;
    }

    public override void Enter()
    {
        Debug.Log("Entered LizardRunAwayState");
        _lizardScript.lizardState = LizardState.RunningAway;
        _lizardScript.ChangeSpeed(_lizardScript.runSpeed, 0.5f);
        _lizardScript.ChangeLayerWeight(_lizardScript.attackLayer, 1f, 0.2f);
        _lizardScript.Animator.Animator.SetFloat("Attack", 0f);
        RunMethod();
    }

    public override void Tick()
    {
        if (_lizardScript.ReachedDestination())
        {
            if (_lizardScript.VcInRange.Count == 0)
            {
                stateMachine.ChangeState(new LizardRoamState(stateMachine, _lizardScript));
            }
            else
            {
                RunMethod();   
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting LizardRunAwayState");
    }
    
    private void RunMethod()
    {
        var target = _lizardScript.GetLoudestVoiceAround();
        if (target != null)
        {
            SetRunningPath(target.transform, _lizardScript.runDistance);
        }

        _lizardScript.waitingForPath = false;
    }
    
    private void SetRunningPath(Transform player, float runDistance)
    {
        Vector3 direction = (_lizardScript.ai.position - player.position).normalized;
        Vector3 rawTarget = _lizardScript.ai.position + direction * runDistance;
        
        NNInfo nearest = AstarPath.active.GetNearest(rawTarget, NearestNodeConstraint.Walkable);

        if (nearest.node != null && nearest.node.Walkable)
        {
            Vector3 validTarget = nearest.position;
            validTarget.y = _lizardScript.ai.position.y;
            
            _lizardScript.ai.destination = validTarget;
        }
    }
}
