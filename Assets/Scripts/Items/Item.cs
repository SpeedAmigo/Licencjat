using FishNet.Object;
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
        
        protected virtual void Update()
        {
            if (!IsServerInitialized) return;
            
            /*if (_rb.IsSleeping())
            {
                SleepNotifyObservers();
            }*/

            if (rbPrediction.Rigidbody.IsSleeping())
            {
                SleepNotifyObservers();
            }
        }
        
        [ObserversRpc]
        private void SleepNotifyObservers()
        {
            //_rb.isKinematic = true;
            rbPrediction.Rigidbody.isKinematic = true;
        }
    }
}
