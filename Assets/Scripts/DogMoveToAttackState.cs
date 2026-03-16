using FishNet.Object;
using UnityEngine;

public class DogMoveToAttackState : State
{
    private DogScript _dogScript;

    private GameObject _target;


    public DogMoveToAttackState(StateMachine machine, DogScript dogScript, GameObject target) : base(machine)
    {
        _dogScript = dogScript;
        _target = target;
    }

    public override void Enter()
    {
        _dogScript.targetPlayer = _target;
        
        _dogScript.agroTimer = _dogScript.agroTime;
        
        Debug.Log("Entering DogFollowTargetState");
    }

    public override void Tick()
    {
        if (_dogScript.startAgroTimer)
        {
            _dogScript.agroTimer -= Time.deltaTime;

            Debug.Log($"AgroTimer; {_dogScript.agroTimer}");
            
            if (_dogScript.agroTimer <= 0)
            {
                _dogScript.startAgroTimer = false;
                _dogScript.targetPlayer = null;
                _dogScript.ai.SetPath(null);
                _dogScript.dogState = DogState.Roam;
                stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
                return;
            }
        }
        
        /*if (_dogScript.targetPlayer == null)
        {

        }*/
        
        if (_dogScript.itemOfInterest && _dogScript.itemOfInterestIsHeld)
        {
            _dogScript.dogState = DogState.FollowTarget;
            stateMachine.ChangeState(new DogFollowTargetState(stateMachine, _dogScript));
            return;
        }
        
        MoveToAttack();
    }

    public override void Exit()
    {
        _dogScript.targetPlayer = null;
        Debug.Log("Exiting DogFollowTargetState");
    }
    
    private void MoveToAttack()
    {
        var target = _dogScript.targetPlayer.transform.position;

        Vector3 direction = _dogScript.transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * (_dogScript.attackDistance - 1f);
        offsetPosition.y = _dogScript.transform.position.y;

        _dogScript.ai.destination = offsetPosition;
        
        if (Vector3.Distance(_dogScript.transform.position, target) <= _dogScript.attackDistance)
        {
            Debug.Log("Attack");
            _dogScript.dogState = DogState.Attack;
            stateMachine.ChangeState(new DogAttackState(stateMachine, _dogScript, _target));
        }
    }
}
