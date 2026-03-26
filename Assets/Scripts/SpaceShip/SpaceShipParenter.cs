using FishNet;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class SpaceShipParenter : NetworkBehaviour
{
    [SerializeField] private NetworkObject spaceShip;
    
    private void OnTriggerEnter(Collider other)
    {
        CompareForPlayer(other, 1);
        CompareForItems(other, 1);
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

    private void CompareForPlayer(Collider other, int value)
    {
        if (!other.CompareTag("Player")) return;
        
        NetworkObject playerObj = other.transform.root.GetComponent<NetworkObject>();
        
        if (playerObj != null)
        {
            RemoveFrogFromInventory(playerObj);
            SetParentServer(playerObj, value);
        }
    }

    private void CompareForItems(Collider other, int value)
    {
        if (other.TryGetComponent(out ObjectPickable item))
        {
            SetParentServer(item, value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveFrogFromInventory(NetworkObject obj)
    {
        if (!obj.TryGetComponent(out PlayerInventoryScript player))
            return;

        if (player.currentItem.Value == null)
            return;

        if (player.currentItem.Value.TryGetComponent(out FrogScript frog))
        {
            // You can use `frog` here if needed
            player.RemoveBigItem(player.currentItem.Value, Vector3.forward);
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