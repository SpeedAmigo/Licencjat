using System;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public abstract class Item : ObjectPickable
    {
        [GUIColor("Yellow")]
        public Sprite itemIcon;
    
        [GUIColor("Yellow")]
        public string itemDisplayName = "Pickup";

        [AllowMutableSyncType] protected SyncVar<bool> useDurability = new(true); 
        
        [SerializeField] protected uint maxDurability;
        [AllowMutableSyncType] protected SyncVar<uint> durability = new();

        [Header("Drop Sound")]
        [SerializeField] protected EventReference dropSound;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            SetToMaxDurability();
        }
        
        protected virtual void Update()
        {
            if (!IsServerInitialized) return;
            
            if (_rb.IsSleeping())
            {
                SleepNotifyObservers();
            }
        }
        
        [ObserversRpc]
        private void SleepNotifyObservers()
        {
            _rb.isKinematic = true;
        }
        
        //[ServerRpc(RequireOwnership = false)]
        protected void SetToMaxDurability()
        {
            durability.Value = maxDurability;
        }
        
        protected virtual bool CheckDurability()
        {
            if (durability.Value <= 0)
            {
                return false;
            }   
            
            return true;
        }
        
        [ServerRpc(RequireOwnership = false)]
        protected void DecreaseDurability()
        {
            if (!useDurability.Value) return;
            
            durability.Value--;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                RuntimeManager.PlayOneShotWithParameter(dropSound, "DropParameter", 0, transform.position);
            }
            else if (other.gameObject.CompareTag("Metal"))
            {
                RuntimeManager.PlayOneShotWithParameter(dropSound, "DropParameter", 1, transform.position);
            }
        }
    }
}
