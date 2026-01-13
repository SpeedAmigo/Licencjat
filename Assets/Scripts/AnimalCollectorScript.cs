using FishNet.Object;
using UnityEngine;

public class AnimalCollectorScript : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        
        if (other.gameObject.TryGetComponent<BaseEnemyScript>(out var script))
        {
            ObjectValue objectValue = other.GetComponent<ObjectValue>();
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            
            QuotaManagerScript.Instance.AddMoney((uint)objectValue.actualSellValue.Value);
            
            if (networkObject != null)
            {
                networkObject.Despawn();
            }
            
            //DespawnOtherObjectServer(networkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnOtherObjectServer(NetworkObject otherObject)
    {
        otherObject.Despawn();
    }
}
