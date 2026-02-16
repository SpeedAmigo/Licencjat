using FishNet.Object;
using UnityEngine;

public interface IPickable
{
    public void Pickup(NetworkObject fpHolder, NetworkObject tpHolder);
    public void Drop();
}
