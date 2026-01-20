using FishNet.Object;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer
{
    public PlayerStateEnum playerState;
    
    private PlayerInventoryScript _playerInventory;
    private OxygenScript _oxygen;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        playerState = PlayerStateEnum.Alive;
        
        _playerInventory = gameObject.GetComponent<PlayerInventoryScript>();
        _oxygen = gameObject.GetComponent<OxygenScript>();
        
        Invoke(nameof(RegisterPlayer), 2f);
    }

    private void RegisterPlayer()
    {
        GameOverManager.Instance.RegisterPlayer(this);
    }

    public void TakeDamage(float damage)
    {
        _oxygen.DrainRate += damage;
        TakeDamageClient(_oxygen.DrainRate);
    }

    [ObserversRpc(BufferLast = true)]
    private void TakeDamageClient(float value)
    {
        _oxygen.DrainRate = value;
    }

    public void Heal(float heal)
    {
        _oxygen.DrainRate -= heal;

        if (_oxygen.DrainRate < _oxygen.BaseDrainRate)
        {
            _oxygen.DrainRate = _oxygen.BaseDrainRate;
        }
    }

    public void RequestItemDrop(ObjectPickable item)
    {
        _playerInventory.RequestRemoveItem(item, _playerInventory);
    }

    private void Die()
    {
        playerState = PlayerStateEnum.Dead;
        GameOverManager.Instance.ComparePlayersState();
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
