using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class testObjectScript : NetworkBehaviour, IPickable
{
    private Collider _collider;
    private Rigidbody _rigidbody;
    
    private void Start()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    public void Pickup(NetworkConnection picker, NetworkObject holder)
    {
        if (!IsServer) return;
        SetPickedUp(holder);

        Pickup_Client(holder);
    }
    
    [ObserversRpc(RunLocally = true)]
    private void Pickup_Client(NetworkObject holder)
    {
        SetPickedUp(holder);
    }

    private void SetPickedUp(NetworkObject holder)
    {
        _rigidbody.isKinematic = true;
        _collider.enabled = false;
        
        transform.SetParent(holder.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
