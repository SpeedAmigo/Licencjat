using UnityEngine;

public class FrogRunState : State
{
    private FrogScript _frogScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FrogRunState(StateMachine machine, FrogScript frogScript) : base(machine)
    {
        _frogScript = frogScript;
    }

    public override void Enter()
    {
        Debug.Log("FrogRunState Enter");
        _frogScript.frogState = FrogState.Running;
        
        _frogScript.statusVisualizer.ShowStatusSign(CreatureStatus.Exclamation, 2f);
    }

    public override void Tick()
    {
        if (_frogScript.playersInRange.Count <= _frogScript.maxPlayers)
        {
            stateMachine.ChangeState(new FrogRoamState(stateMachine, _frogScript));
        }
        
        RunMethod();
    }

    public override void Exit()
    {
        Debug.Log("FrogRunState Exit");
    }
    
    private void RunMethod()
    {
        //if (pickedUp.Value) return;
        
        _frogScript.running = true;
        //CancelInvoke(nameof(SetNewPath));

        if (_frogScript.playersInRange.Count != 0)
        {
            SetRunningPath(_frogScript.playersInRange[0].transform, _frogScript.runDistance);
        }
        
        _frogScript.waitingForPath = false;
    }
    
    private void SetRunningPath(Transform player, float runDistance)
    {
        Vector3 direction = (_frogScript.ai.position - player.position).normalized;
        Vector3 runTarget = _frogScript.ai.position + direction * runDistance;
        runTarget.y = _frogScript.ai.position.y;
        
        _frogScript.ai.destination = runTarget;
    }
}
