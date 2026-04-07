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
        [AllowMutableSyncType] protected SyncVar<uint> durability = new();
        
        protected virtual void Update()
        {
            if (!IsServerInitialized) return;
            
            if (rbPrediction.Rigidbody.IsSleeping())
            {
                SleepNotifyObservers();
            }
        }
        
        [ObserversRpc]
        private void SleepNotifyObservers()
        {
            rbPrediction.Rigidbody.isKinematic = true;
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
