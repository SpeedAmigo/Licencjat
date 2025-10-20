using FishNet.Object;
using UnityEngine;

public abstract class State
{
    protected StateMachine StateMachine;
    protected NetworkBehaviour Owner;

    public State(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
        Owner = stateMachine;
    }

    public virtual void Enter() { }

    public virtual void LogicUpdate() { }

    public virtual void PhysicsUpdate() { }

    public virtual void Exit() { }
}
