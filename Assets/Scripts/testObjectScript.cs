using FishNet.Object;
using UnityEngine;

public class TestObjectScript : NetworkBehaviour, IPickable
{
    private Rigidbody _rigidbody;
    private Collider _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }
    
    public void Pickup(NetworkObject holder)
    {
        SetNewPosition(holder);
        
        SetNewPosition_Client(holder);
    }
    
    [ObserversRpc(RunLocally = true, BufferLast = true)]
    private void SetNewPosition_Client(NetworkObject holder)
    {
        SetNewPosition(holder);
    }

    private void SetNewPosition(NetworkObject holder)
    {
        transform.SetParent(holder.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        
        _collider.enabled = false;
    }
}


