using System;
using FishNet.CodeGenerating;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using FMODUnity;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerRoot : NetworkBehaviour, IPlayer, IDamageable, IStunable
{
    public event Action OnReviveEvent;
    public event Action<bool, float> StunEvent;
    public event Action HealEvent;
    
    [Header("Player State")]
    [AllowMutableSyncType] public SyncVar<bool> isAlive;
    [AllowMutableSyncType] public SyncVar<PlayerState> state;
    
    [Header("Sounds")]
    [SerializeField] private EventReference getDamageSound;
    [SerializeField] private EventReference getHealSound;

    [SerializeField] private Animator animator;
    
    [HideInInspector] public OxygenScript oxygen;
    private PlayerInventoryScript _playerInventory;
    private PlayerInteractor _playerInteractor;
    
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
        }
        
        _playerInventory = gameObject.GetComponent<PlayerInventoryScript>();
        _playerInteractor = gameObject.GetComponent<PlayerInteractor>();

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
            SoundCreator.Instance.PlayOneShot(getDamageSound, transform.position);
        }
        else if (damage < 0)
        {
            SoundCreator.Instance.PlayOneShot(getHealSound, transform.position);
            Heal(Owner);
        }
    }

    [TargetRpc]
    private void Heal(NetworkConnection conn)
    {
        HealEvent?.Invoke();
    }
    
    public Vector3 DropPosition()
    {
        return _playerInteractor.TryGetDropPosition(out Vector3 dropPosition) ? dropPosition : transform.position;
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
        _playerInventory.RequestRemoveItem(item, _playerInventory, DropPosition());
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void RequestDropInventory()
    {
        for (int i = _playerInventory.slots.Count - 1; i >= 0; i--)
        {
            _playerInventory.RequestRemoveItem(_playerInventory.slots[i], _playerInventory, DropPosition(), true);
        }
    }

    private void Die()
    {
        if (!IsOwner || !isAlive.Value) return;
        
        SetPlayerAlive(false);
        RequestDropInventory();
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
        oxygen.hasOxygen.Value = true;
        oxygen.canDrainOxygen.Value = false;
        oxygen.currentOxygen.Value = maxOxygen;
        oxygen.drainRate.Value = baseDrainRate;
        oxygen.UpdateCurrentStaminaTarget(Owner, maxOxygen);
    }

    [Server]
    public void SetStunned(bool stunned, float duration)
    {
        SetStunnedObservers(stunned, duration);

        state.Value = stunned ? PlayerState.Stunned : PlayerState.Default;
    }

    [ObserversRpc]
    private void SetStunnedObservers(bool stunned, float duration)
    {
        StunEvent?.Invoke(stunned, duration);
    }
}

public enum PlayerState
{
    Default,
    Stunned,
}
