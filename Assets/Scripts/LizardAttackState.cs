using UnityEngine;

public class LizardAttackState : State
{
    private LizardScript _lizardScript;
    private GameObject _target;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LizardAttackState(StateMachine machine, LizardScript lizardScript) : base(machine)
    {
        _lizardScript = lizardScript;
        _target = lizardScript.VcInRange[Random.Range(0, _lizardScript.VcInRange.Count)].gameObject;
    }

    public override void Enter()
    {
        Debug.Log("Lizard attack state entered");
        _lizardScript.lizardState = LizardState.MoveToAttack;
        _lizardScript.ChangeSpeed(_lizardScript.runSpeed, 0.5f);
        _lizardScript.SetNewPath(_target.transform.position);
    }

    public override void Tick()
    {
        if (_lizardScript.VcInRange.Count != 0 && _lizardScript.GetLoudestVoiceAround().Volume >= _lizardScript.noiseThreshold)
        {
            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }
        
        MoveToAttack();
    }

    public override void Exit()
    {
        Debug.Log("Lizard attack state exiting");
    }
    
    private void MoveToAttack()
    {
        var target = _target.transform.position;

        Vector3 direction = _lizardScript.transform.position - target;
        direction.y = 0f;
        direction.Normalize();

        Vector3 offsetPosition = target + direction * (_lizardScript.attackDistance - 1f);
        offsetPosition.y = _lizardScript.transform.position.y;
        
        _lizardScript.ai.destination = offsetPosition;
        
        if (Vector3.Distance(_lizardScript.transform.position, target) <= _lizardScript.attackDistance)
        {
            Debug.Log("Attack");
            _lizardScript.lizardState = LizardState.Attack;
            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }
    }
}
