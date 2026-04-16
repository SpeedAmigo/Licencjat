using UnityEngine;

public class DogStunState : State
{
    DogScript _dogScript;

    private readonly float _duration;
    private float _currentTime = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public DogStunState(StateMachine machine, DogScript dogScript, float duration) : base(machine)
    {
        _dogScript = dogScript;
        _duration = duration;
    }

    public override void Enter()
    {
        Debug.Log("Entering dog stun state");

        _dogScript.dogVisualizer.ShowStatusSign(CreatureStatus.Star, _duration);
        //_dogScript.ai.simulateMovement = false;
        _dogScript.ai.isStopped = true;
    }

    public override void Tick()
    {
        _currentTime += Time.deltaTime;

        if (_currentTime >= _duration)
        {
            stateMachine.ChangeState(new DogRoamState(stateMachine, _dogScript));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting dog stun state");
        //_dogScript.ai.simulateMovement = true;
        _dogScript.ai.isStopped = false;
    }
}
