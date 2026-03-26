using UnityEngine;

public class DogAttackState : State
{
    private DogScript _dogScript;

    private float _timer;

    private float _minDistance;
    private float _maxDistance;
    
    private GameObject _target;
    
    public DogAttackState(StateMachine machine, DogScript dogScript, GameObject target) : base(machine)
    {
        _dogScript = dogScript;
        _target = target;
    }

    public override void Enter()
    {
        _timer = _dogScript.attackTimer;

        _minDistance = _dogScript.attackDistance;
        _maxDistance = _dogScript.maxAwareDistance;
        
        Attack();
        
        Debug.Log("Attack state Entered");
    }

    public override void Tick()
    {
        // ensuring that dog has item and any player near him
        if (!_dogScript.itemOfInterest || _dogScript.playersInRange.Count == 0)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }
        
        var distance = Vector3.Distance(_dogScript.transform.position, _target.transform.position);
        
        // if player hold item and dog is within max range it starts to follow the player
        if (_dogScript.itemOfInterestIsHeld && Vector3.Distance(_dogScript.transform.position, _dogScript.holdingPlayer.transform.position) < _maxDistance)
        {
            _dogScript.dogState = DogState.FollowTarget;
            stateMachine.ChangeState(new DogFollowTargetState(stateMachine, _dogScript));
            return;
        }
        
        // if target player is too far away to attack him dog start to roam
        if (!_dogScript.itemOfInterestIsHeld && distance > _maxDistance)
        {
            _dogScript.dogState = DogState.Roam;
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
            return;
        }

        // if target is in max range but too far away to attack dog start to move towards the player
        if (!_dogScript.itemOfInterestIsHeld && distance > _minDistance)
        {
            _dogScript.dogState = DogState.MoveToAttack;
            stateMachine.ChangeState(new DogMoveToAttackState(stateMachine, _dogScript,  _dogScript.playersInRange[0]));
            return;
        }
        
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            Attack();
            _timer = _dogScript.attackTimer;
        }
    }

    public override void Exit()
    {
        Debug.Log("Attack state Exit");
    }
    
    private void Attack()
    {
        if (_dogScript.playersInRange.Count <= 0) return;
        
        _dogScript.Animator.Animator.SetTrigger("Attack");
        
        Debug.Log("Dog has attacked!");
    }
}
