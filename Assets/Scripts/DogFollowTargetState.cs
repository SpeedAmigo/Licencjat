using UnityEngine;

public class DogFollowTargetState : State
{
    private readonly DogScript _dogScript;
    
    public DogFollowTargetState(StateMachine machine, DogScript dogScript) : base(machine)
    {
        _dogScript = dogScript;
    }

    public override void Enter()
    {
        //_dogScript.ai.maxSpeed = _dogScript.walkSpeed;
        _dogScript.ChangeSpeed(_dogScript.walkSpeed, 0.5f);
        
        Debug.Log("Entering DogFollowTargetState");
    }

    public override void Tick()
    {
        if (!_dogScript.itemOfInterest)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }

        if (!_dogScript.itemOfInterestIsHeld && _dogScript.playersInRange.Count > 0 && Vector3.Distance(_dogScript.itemOfInterest.transform.position,
                _dogScript.playersInRange[0].transform.position) <= _dogScript.agroDistance)
        {
            _dogScript.dogState = DogState.MoveToAttack;
            stateMachine.ChangeState(new DogMoveToAttackState(stateMachine, _dogScript,  _dogScript.playersInRange[0]));
            return;
        }
        
        if (!_dogScript.itemOfInterestIsHeld && _dogScript.playersInRange.Count > 0 && Vector3.Distance(_dogScript.itemOfInterest.transform.position,
                _dogScript.playersInRange[0].transform.position) >= _dogScript.agroDistance)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }

        if (!_dogScript.itemOfInterestIsHeld && _dogScript.playersInRange.Count <= 0)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }

        if (_dogScript.ai.reachedEndOfPath)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }

        FollowTarget(_dogScript.itemOfInterest.transform.position);
    }

    public override void Exit()
    {
        Debug.Log("Exiting DogFollowTargetState");
    }
    
    private void FollowTarget(Vector3 target)
    {
        Debug.Log("FollowTarget");
        
        Vector3 direction = _dogScript.transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * _dogScript.stopDistance;
        offsetPosition.y = _dogScript.transform.position.y;
        
        _dogScript.ai.destination = offsetPosition;
    }
}
