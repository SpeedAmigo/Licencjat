using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class AnimalCollectorScript : NetworkBehaviour
{
    [SerializeField] private EventReference collectSound;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        
        if (other.gameObject.TryGetComponent<BaseEnemyScript>(out var script))
        {
            HandleObject(other);
        }

        if (other.gameObject.TryGetComponent<SellableItem>(out var item))
        {
            HandleObject(other);
        }
    }

    private void HandleObject(Collider other)
    {
        ObjectValue objectValue = other.GetComponent<ObjectValue>();
        NetworkObject networkObject = other.GetComponent<NetworkObject>();
            
        QuotaManagerScript.Instance.AddMoney((uint)objectValue.actualSellValue.Value);
            
        SoundCreator.Instance.PlayOneShot(collectSound, other.transform.position);
            
        if (networkObject != null)
        {
            networkObject.Despawn();
        }
    }
}
