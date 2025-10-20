using FishNet.Object;
using UnityEngine;

public class StateMachine : NetworkBehaviour
{
    protected State currentState;
    public State CurrentState => currentState;

    public void ChangeState(State newState)
    {
        if (currentState == newState) return;
        if (currentState != null) currentState.Exit();
        currentState = newState;
        if (currentState != null) currentState.Enter();
    }

    protected virtual void Update()
    {
        if (currentState != null) currentState.LogicUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (currentState != null) currentState.PhysicsUpdate();
    }
}
