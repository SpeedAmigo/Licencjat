using System;
using FishNet.Object;
using UnityEngine;

public class ObjectPickable : NetworkBehaviour
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
        if (!IsServerInitialized) return;
        PickupLogic(holder);
        Pickup_Client(holder);
    }

    public virtual void Drop()
    {
        if (!IsServerInitialized) return;
        DropLogic();
        Drop_Client();
    }

    [ObserversRpc]
    private void Pickup_Client(NetworkObject holder)
    {
        PickupLogic(holder);
    }

    [ObserversRpc]
    private void Drop_Client()
    {
        DropLogic();
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

    private void DropLogic()
    {
        transform.SetParent(null);
        
        _rb.AddForce(Vector3.forward * 2f, ForceMode.Impulse);

        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = true;
    }
}
