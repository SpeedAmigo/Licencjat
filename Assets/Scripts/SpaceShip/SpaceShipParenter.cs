using FishNet;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class SpaceShipParenter : NetworkBehaviour
{
    [SerializeField] private NetworkObject spaceShip;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        NetworkObject playerObj = other.transform.root.GetComponent<NetworkObject>();
        
        if (playerObj != null)
        {
            SetParentServer(playerObj, 1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        NetworkObject playerObj = other.transform.GetComponent<NetworkObject>();
        if (playerObj != null)
        { 
            SetParentServer(playerObj, 0);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetParentServer(NetworkObject player, int parentIndex)
    {
        SetParentObservers(player, parentIndex);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetParentObservers(NetworkObject player, int parentIndex)
    {
        if (parentIndex == 0)
        {
            player.UnsetParent();
            //player.transform.SetParent(null);
        }
        else if (parentIndex == 1)
        {
            player.SetParent(spaceShip);
            //player.transform.SetParent(transform.parent, true);
        }
    }
}