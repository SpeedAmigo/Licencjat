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
        int index = Random.Range(0, _lizardScript.playersInRange.Count);
        var target = _lizardScript.playersInRange[index];
        //var target = _lizardScript.GetLoudestVoiceAround();
        if (target != null)
        {
            SetRunningPath(target.transform, _lizardScript.runDistance);
        }
        else
        {
            Debug.Log("No target to run from");
        }

        _lizardScript.waitingForPath = false;
    }
    
    /*private void SetRunningPath(Transform player, float runDistance)
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
    }*/

    private NNConstraint GetConstraint()
    {
        NNConstraint constraint = NNConstraint.Default;

        constraint.constrainWalkability = true;
        constraint.walkable = true;

        constraint.constrainTags = true;

        // Allow only tag 0
        constraint.tags = 1 << 0;

        return constraint;
    }
    
    private void SetRunningPath(Transform player, float runDistance)
    {
        NNConstraint constraint = GetConstraint();
        
        GraphNode startNode = AstarPath.active.GetNearest(_lizardScript.ai.position, constraint).node;

        if (startNode == null)
            return;

        Vector3 direction = (_lizardScript.ai.position - player.position).normalized;

        for (int i = 0; i < 15; i++)
        {
            // Add some randomness so it doesn't always run perfectly straight
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = 0;

            Vector3 rawTarget = _lizardScript.ai.position + direction * runDistance + randomOffset;

            var nearest = AstarPath.active.GetNearest(rawTarget, constraint);

            GraphNode targetNode = nearest.node;

            if (targetNode == null)
                continue;

            if (targetNode == startNode)
                continue;

            if (!PathUtilities.IsPathPossible(startNode, targetNode))
                continue;

            Vector3 finalPos = (Vector3)targetNode.position;
            
            _lizardScript.ai.destination = finalPos;
            return;
        }

        Debug.LogWarning("Failed to find valid run away position");
    }
}
