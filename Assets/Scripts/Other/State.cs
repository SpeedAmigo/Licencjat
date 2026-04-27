using UnityEngine;

public class State
{
    protected StateMachine stateMachine;

    public State(StateMachine machine)
    {
        stateMachine = machine;
    }
    
    public virtual void Enter() {}
    public virtual void Tick() {}
    public virtual void Exit() {}
}
