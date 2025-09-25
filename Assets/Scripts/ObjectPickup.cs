using System;
using FishNet.Object;
using UnityEngine;

public class ObjectPickup : NetworkBehaviour
{
    private Rigidbody _rb;
    private Collider _col;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }
    
    public virtual void Pickup(NetworkObject holder)
    {
        if (!IsServer) return;
        PickupLogic(holder);
        Pickup_Client(holder);
    }

    [ObserversRpc]
    private void Pickup_Client(NetworkObject holder)
    {
        PickupLogic(holder);
    }

    private void PickupLogic(NetworkObject holder)
    {
        transform.SetParent(holder.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = false;
    }
}
