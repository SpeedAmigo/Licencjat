using FishNet.Object;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer
{
    public PlayerStateEnum playerState;
    
    private PlayerInventoryScript _playerInventory;
    private OxygenScript _oxygen;

    public PlayerInventoryScript PlayerInventory
    {
        get { return _playerInventory; }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerState = PlayerStateEnum.Alive;
        
        _playerInventory = gameObject.GetComponent<PlayerInventoryScript>();
        _oxygen = gameObject.GetComponent<OxygenScript>();
    }

    public void TakeDamage(float damage)
    {
        _oxygen.DrainRate += damage;
    }

    public void Heal(float heal)
    {
        _oxygen.DrainRate -= heal;
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
