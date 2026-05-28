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
        _lizardScript.ChangeLayerWeight(_lizardScript.attackLayer, 1f, 0.2f);
        _lizardScript.Animator.Animator.SetFloat("Attack", 1f);
    }

    public override void Tick()
    {
        MoveToAttack();
        
        /*if (_lizardScript.VcInRange.Count != 0 && _lizardScript.GetLoudestVoiceAround().voiceVolume.Value >= _lizardScript.noiseThreshold)
        {
            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }*/
    }

    public override void Exit()
    {
        Debug.Log("Lizard attack state exiting");
    }
    
    private void MoveToAttack()
    {
        if (_target == null)
            return;

        Vector3 target = _target.transform.position;

        Vector3 direction = (target - _lizardScript.transform.position).normalized;
        direction.y = 0f;

        Vector3 offsetPosition = target - direction * (_lizardScript.attackDistance - 1f);
        offsetPosition.y = _lizardScript.transform.position.y;

        _lizardScript.ai.destination = offsetPosition;
        
        float distance = Vector3.Distance(
            _lizardScript.transform.position,
            _target.transform.position
        );
        
        if (distance <= _lizardScript.attackDistance)
        {
            Debug.Log("Attack");

            _lizardScript.lizardState = LizardState.Attack;

            if (_target.transform.parent.TryGetComponent<StatusEffectHandler>(out var effectHandler))
            {
                Debug.Log(effectHandler.name);
                effectHandler.ApplyEffects(_lizardScript.damageEffects);
            }
            else
            {
                Debug.Log("statusEffect Empty");
            }

            stateMachine.ChangeState(new LizardRunAwayState(stateMachine, _lizardScript));
        }
    }
}
