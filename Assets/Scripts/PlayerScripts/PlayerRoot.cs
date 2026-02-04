using System;
using FishNet.CodeGenerating;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer
{
    public event Action OnReviveEvent;
    
    [Header("Player State")]
    [AllowMutableSyncType] public SyncVar<bool> isAlive;
    
    [HideInInspector] public OxygenScript oxygen;
    private PlayerInventoryScript _playerInventory;
    
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        if (IsOwner)
        {
            SetPlayerAlive(true);
        }
        
        _playerInventory = gameObject.GetComponent<PlayerInventoryScript>();

        oxygen.OnDieEvent += Die;
        
        Invoke(nameof(RegisterPlayer), 2f);
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetPlayerAlive(bool value)
    {
        isAlive.Value = value;
    }

    private void RegisterPlayer()
    {
        if (!IsOwner) return;
        GameOverManager.Instance.RegisterPlayer(this);
    }

    public void TakeDamage(float damage)
    {
        oxygen.drainRate.Value += damage;
        //TakeDamageClient(oxygen.drainRate.Value);
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

    private void Die()
    {
        if (!IsOwner || !isAlive.Value) return;
        
        SetPlayerAlive(false);
        GameOverManager.Instance.ComparePlayersState();
    }
    
    [Server]
    public void ReviveServer(NetworkConnection conn)
    {
        ReviveClient(conn, _spawnPosition, _spawnRotation);
    }

    [TargetRpc]
    private void ReviveClient(NetworkConnection conn, Vector3 pos, Quaternion rot)
    {
        SetPlayerAlive(true);
        
        ChangeOxygenOnRevive(oxygen.maxOxygen.Value, oxygen.baseDrainRate.Value);
        
        transform.SetPositionAndRotation(pos, rot);
        
        OnReviveEvent?.Invoke();
    }

    [ServerRpc]
    private void ChangeOxygenOnRevive(float maxOxygen, float baseDrainRate)
    {
        oxygen.canDrainOxygen.Value = false;
        oxygen.currentOxygen.Value = maxOxygen;
        oxygen.drainRate.Value = baseDrainRate;
        oxygen.UpdateCurrentStaminaTarget(Owner, maxOxygen);
    }
}
