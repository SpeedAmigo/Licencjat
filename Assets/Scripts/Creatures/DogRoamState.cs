using FMODUnity;
using UnityEngine;

public class DogRoamState : State
{
    private readonly DogScript _dogScript;

    private float _maxDistance;

    private float _currentTime;
    private float _timer;
    
    public DogRoamState(StateMachine machine, DogScript dogScript) : base(machine)
    {
        _dogScript = dogScript;
        _timer = Random.Range(_dogScript.idleSoundTimer - 1f, _dogScript.idleSoundTimer + 1f);
    }

    public override void Enter()
    {
        _dogScript.running = false;
        _maxDistance = _dogScript.maxAwareDistance;
        
        _dogScript.ChangeSpeed(_dogScript.walkSpeed, 0.5f);
        
        _dogScript.waitingForPath = true;
        _dogScript.Invoke(nameof(_dogScript.SetNewPath), 0.1f);
        
        Debug.Log("Entered DogRoamState");
    }

    public override void Tick()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime >= _timer)
        {
            PlaySoundAndRandomize();
        }
        
        if (!_dogScript.itemOfInterest)
        {
            Roam(false);
            return;
        }
        
        if (_dogScript.playersInRange.Count > 0 && !_dogScript.itemOfInterestIsHeld && Vector3.Distance(_dogScript.transform.position, _dogScript.itemOfInterest.transform.position) <= _maxDistance)
        {
            foreach (var player in _dogScript.playersInRange)
            {
                if (Vector3.Distance(_dogScript.itemOfInterest.transform.position, player.transform.position) <= _dogScript.agroDistance)
                {
                    _dogScript.targetPlayer = player.gameObject;
                    _dogScript.dogState = DogState.MoveToAttack;
                    stateMachine.ChangeState(new DogMoveToAttackState(stateMachine, _dogScript, player));
                    return;
                }
            }
        }

        if (_dogScript.itemOfInterestIsHeld && Vector3.Distance(_dogScript.transform.position, _dogScript.holdingPlayer.transform.position) < _maxDistance)
        {
            _dogScript.dogState = DogState.FollowTarget;
            stateMachine.ChangeState(new DogFollowTargetState(stateMachine, _dogScript));
            return;
        }
        
        if (!_dogScript.itemOfInterestIsHeld)
        {
            Roam(true);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting DogRoamState");
    }
    
    private void Roam(bool hasTarget)
    {
        /*if (!_dogScript.ai.pathPending && (_dogScript.ai.reachedEndOfPath || !_dogScript.ai.hasPath) && !_dogScript.waitingForPath)
        {
            if (!_dogScript.canWalk) return;
            
            _dogScript.waitingForPath = true;
            
            _dogScript.Invoke(hasTarget ? nameof(_dogScript.NewPathWrapper) : nameof(_dogScript.SetNewPath), 3f);
        }*/
        
        if (_dogScript.ReachedDestination())
        {
            if (!_dogScript.canWalk) return;
            
            _dogScript.waitingForPath = true;
            
            _dogScript.Invoke(hasTarget ? nameof(_dogScript.NewPathWrapper) : nameof(_dogScript.SetNewPath), 3f);
        }
    }

    private void PlaySoundAndRandomize()
    {
        RuntimeManager.PlayOneShotAttached(_dogScript.idleSound, _dogScript.gameObject);
        _currentTime = 0;
        _timer = Random.Range(_dogScript.idleSoundTimer - 1f, _dogScript.idleSoundTimer + 1f);
    }
}
