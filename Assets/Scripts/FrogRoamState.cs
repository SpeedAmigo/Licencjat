using UnityEngine;

public class FrogRoamState : State
{
    private FrogScript _frogScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FrogRoamState(StateMachine machine, FrogScript frogScript) : base(machine)
    {
        _frogScript = frogScript;
    }
    
    public override void Enter()
    {
        Debug.Log("FrogRoamState Enter");
        _frogScript.frogState = FrogState.Roaming;
        
        _frogScript.ai.destination = _frogScript.PickRandomPoint();
    }

    public override void Tick()
    {
        bool canRun = _frogScript.canRunaway && _frogScript.playersInRange.Count > _frogScript.maxPlayers;

        if (canRun)
        {
            stateMachine.ChangeState(new FrogRunState(stateMachine, _frogScript));
        }
        
        WalkMethod();
    }

    public override void Exit()
    {
        Debug.Log("FrogRoamState Exit");
    }
    
    private void WalkMethod()
    {
        //if (pickedUp.Value) return;
        
        _frogScript.running = false;
            
        if (!_frogScript.ai.pathPending && (_frogScript.ai.reachedEndOfPath || !_frogScript.ai.hasPath) && !_frogScript.waitingForPath)
        {
            if (!_frogScript.canWalk) return;
            
            _frogScript.waitingForPath = true;
            _frogScript.Invoke(nameof(_frogScript.SetNewPath), 3f);
        }
    }
}
