using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    }
}
