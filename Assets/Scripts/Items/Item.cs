using System;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public abstract class Item : ObjectPickable, IOutlineChangeable
    {
        [Header("Item icon")]
        [GUIColor("Yellow")]
        public Sprite itemIcon;
    
        [Header("Item display name")]
        [GUIColor("Yellow")]
        public string itemDisplayName = "Pickup";

        [Header("Durability Settings")]
        [AllowMutableSyncType] protected SyncVar<bool> useDurability = new(true);
        [SerializeField] protected uint maxDurability;
        [AllowMutableSyncType] protected SyncVar<uint> durability = new();
        
        [Header("Drop Sound")]
        [SerializeField] protected EventReference dropSound;
        
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            SetToMaxDurability();
        }
        
        protected virtual void LateUpdate()
        {
            if (!IsServerInitialized) return;
            
            if (_rb.IsSleeping() && !_rb.isKinematic)
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
                RuntimeManager.PlayOneShotWithParameter(dropSound, new ParameterValues("DropParameter", 0), transform.position);
            }
            else if (other.gameObject.CompareTag("Metal"))
            {
                RuntimeManager.PlayOneShotWithParameter(dropSound, new ParameterValues("DropParameter", 1), transform.position);
            }
        }

        public void SetOutlineColor(Color outlineColor)
        {
            if (rend == null) return;
        
            rend.GetPropertyBlock(_propertyBlock);
        
            _propertyBlock.SetColor(OutlineColor, outlineColor);
        
            rend.SetPropertyBlock(_propertyBlock);
        }
    }
}
