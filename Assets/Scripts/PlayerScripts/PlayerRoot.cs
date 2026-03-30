using System;
using FishNet.CodeGenerating;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using FMODUnity;
using Items;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer, IDamageable
{
    public event Action OnReviveEvent;
    
    [Header("Player State")]
    [AllowMutableSyncType] public SyncVar<bool> isAlive;
    
    [Header("Sounds")]
    [SerializeField] private EventReference getDamageSound;
    [SerializeField] private EventReference getHealSound;

    [SerializeField] private Animator animator;
    
    [HideInInspector] public OxygenScript oxygen;
    private PlayerInventoryScript _playerInventory;
    
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private EventInstance _getDamageInstance;
    private EventInstance _getHealInstance;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (IsOwner)
        {
            SetPlayerAlive(true);
            
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            Debug.Log(_spawnPosition);
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
        
        if (oxygen.drainRate.Value < oxygen.baseDrainRate.Value)
        {
            oxygen.drainRate.Value = oxygen.baseDrainRate.Value;
        }

        if (damage > 0)
        {
            _getDamageInstance = RuntimeManager.CreateInstance(getDamageSound);
            _getDamageInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            _getDamageInstance.start();
            _getDamageInstance.release();
        }
        else if (damage < 0)
        {
            _getHealInstance = RuntimeManager.CreateInstance(getHealSound);
            _getHealInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            _getHealInstance.start();
            _getHealInstance.release();
        }
    }
    
    [TargetRpc]
    public void StartDurationFill(NetworkConnection conn, float duration)
    {
        PlayerUsageManager.Instance.StartFillUsage(duration);
    }

    [TargetRpc]
    public void StopDurationFill(NetworkConnection conn)
    {
        PlayerUsageManager.Instance.StopFillUsage();
    }
    
    public void RequestItemDrop(Item item)
    {
        _playerInventory.RequestRemoveItem(item, _playerInventory);
    }

    [ServerRpc(RequireOwnership = true)]
    public void RequestDropInventory()
    {
        for (int i = _playerInventory.slots.Count - 1; i >= 0; i--)
        {
            _playerInventory.RequestRemoveItem(_playerInventory.slots[i], _playerInventory);
        }
    }

    private void Die()
    {
        if (!IsOwner || !isAlive.Value) return;
        
        SetPlayerAlive(false);
        GameOverManager.Instance.ComparePlayersState();
    }

    [Server]
    public void RestorePlayer(NetworkConnection conn, bool alive, bool leftOnPlanet)
    {
        RestorePlayerTarget(conn, alive, leftOnPlanet);
    }

    [TargetRpc]
    private void RestorePlayerTarget(NetworkConnection conn, bool alive, bool leftOnPlanet)
    {
        if (!alive)
        {
            SetPlayerAlive(true);
            OnReviveEvent?.Invoke();
        }

        if (leftOnPlanet)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController) playerController.enabled = false;
            
            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            
            if (playerController) playerController.enabled = true;
        }
        
        ChangeOxygenOnRevive(oxygen.maxOxygen.Value, oxygen.baseDrainRate.Value);
        
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
