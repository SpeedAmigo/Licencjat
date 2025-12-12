using FishNet;
using FishNet.Object;
using UnityEngine;

public class SpaceShipParenter : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject playerObj = other.transform.root.GetComponent<NetworkObject>();
            if (playerObj != null)
                SetParentServer(playerObj, 1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject playerObj = other.transform.root.GetComponent<NetworkObject>();
            if (playerObj != null)
                SetParentServer(playerObj, 0);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetParentServer(NetworkObject player, int parentId)
    {
        SetParentObservers(player, parentId);
    }

    [ObserversRpc]
    private void SetParentObservers(NetworkObject player, int parentId)
    {
        Debug.Log("Parented for observers");
        if (parentId == 0)
        {
            player.transform.SetParent(null);
        }
        else if (parentId == 1)
        {
            player.transform.SetParent(transform.parent);
        }
    }
}