using FishNet.Object;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer
{
    public PlayerStateEnum playerState;

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerState = PlayerStateEnum.Alive;
    }

    public void TakeDamage(float damage)
    {
        
    }

    public void Heal(float heal)
    {
        
    }

    private void Die()
    {
        playerState = PlayerStateEnum.Dead;
    }

    private void OnEnable()
    {
        OxygenScript.OnDieEvent += Die;
    }

    private void OnDisable()
    {
        OxygenScript.OnDieEvent -= Die;
    }
    
}
