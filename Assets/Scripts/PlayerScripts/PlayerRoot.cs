using FishNet.Component.Spawning;
using FishNet.Object;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer
{
    [Header("Player State")]
    public PlayerStateEnum playerState;
    public OxygenScript oxygen;
    
    private PlayerInventoryScript _playerInventory;
    private Transform _initialPosition;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        _initialPosition = gameObject.transform;
        
        playerState = PlayerStateEnum.Alive;
        
        _playerInventory = gameObject.GetComponent<PlayerInventoryScript>();
        //oxygen = gameObject.GetComponent<OxygenScript>();

        oxygen.OnDieEvent += Die;
        oxygen.OnReviveEvent += Revive;
        
        Invoke(nameof(RegisterPlayer), 2f);
    }

    private void RegisterPlayer()
    {
        if (!IsOwner) return;
        GameOverManager.Instance.RegisterPlayer(this);
    }

    public void TakeDamage(float damage)
    {
        oxygen.DrainRate += damage;
        TakeDamageClient(oxygen.DrainRate);
    }

    [ObserversRpc(BufferLast = true)]
    private void TakeDamageClient(float value)
    {
        oxygen.DrainRate = value;
    }

    public void Heal(float heal)
    {
        oxygen.DrainRate -= heal;

        if (oxygen.DrainRate < oxygen.BaseDrainRate)
        {
            oxygen.DrainRate = oxygen.BaseDrainRate;
        }
    }

    public void RequestItemDrop(ObjectPickable item)
    {
        _playerInventory.RequestRemoveItem(item, _playerInventory);
    }

    public void Die()
    {
        if (playerState == PlayerStateEnum.Dead) return;
        
        playerState = PlayerStateEnum.Dead;
        GameOverManager.Instance.ComparePlayersState();
    }

    public void Revive()
    {
        Debug.Log("Revive");
        playerState = PlayerStateEnum.Alive;
        oxygen.CurrentOxygen = oxygen.MaxOxygen;
        
        gameObject.transform.position = _initialPosition.position;
        gameObject.transform.rotation = _initialPosition.rotation;
    }
}
