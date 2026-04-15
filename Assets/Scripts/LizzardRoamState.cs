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
        _lizardScript.ChangeSpeed(_lizardScript.walkSpeed, 0.5f);
        _lizardScript.SetNewPath();
    }

    public override void Tick()
    {
        if (_lizardScript.VcInRange.Count != 0 && _lizardScript.VcInRange[0].Volume > _lizardScript.noiseThreshold)
        {
            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }
        
        Roam();
    }

    public override void Exit()
    {
        Debug.Log("Exiting lizard roam state");
    }

    private void Roam()
    {
        if (_lizardScript.ReachedDestination())
        {
            _lizardScript.waitingForPath = true;
            _lizardScript.Invoke(nameof(_lizardScript.SetNewPath), 3f);
        }
    }
}
