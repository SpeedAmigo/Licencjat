using FishNet.Object;
using UnityEngine;

public class StateMachine : NetworkBehaviour
{
    private State _currentState;
    
    public void ChangeState(State newState)
    {
        if (!IsServerInitialized) return;
        
        if (_currentState != null)
        {
            _currentState.Exit();
        }
        
        _currentState = newState;
        _currentState.Enter();
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        
        if (_currentState != null)
        {
            _currentState.Tick();
        }
    }
}
