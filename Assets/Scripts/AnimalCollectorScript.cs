using FishNet.Object;
using UnityEngine;

public class AnimalCollectorScript : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Frog"))
        {
            ObjectValue objectValue = other.GetComponent<ObjectValue>();
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            
            QuotaManagerScript.Instance.AddMoney((uint)objectValue.sellValue);
            
            DespawnOtherObjectServer(networkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnOtherObjectServer(NetworkObject otherObject)
    {
        otherObject.Despawn();
    }
}
