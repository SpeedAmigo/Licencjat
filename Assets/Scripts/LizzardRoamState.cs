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
        _lizardScript.ChangeSpeed(_lizardScript.walkSpeed, 0.5f);
        _lizardScript.SetNewPath();
    }

    public override void Tick()
    {
        Roam();
    }

    public override void Exit()
    {
        
    }

    private void Roam()
    {
        if (_lizardScript.ai.reachedDestination && _lizardScript.ai.reachedEndOfPath && !_lizardScript.waitingForPath)
        {
            _lizardScript.waitingForPath = true;
            _lizardScript.Invoke(nameof(_lizardScript.SetNewPath), 3f);
        }
    }
}
