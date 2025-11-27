using System;
using FishNet.Object;
using UnityEngine;

public class ObjectPickable : NetworkBehaviour
{
    public Transform offset;
    public GameObject objectToChangeLayer;
    public Sprite itemIcon;
    public bool isBig;
    
    private Rigidbody _rb;
    private Collider _col;
    
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }
    
    public void Pickup(NetworkObject holder)
    {
        if (!IsServerInitialized) return;
        PickupLogic(holder);
        Pickup_Client(holder);
    }

    public void Drop()
    {
        if (!IsServerInitialized) return;
        DropLogic();
        Drop_Client();
    }

    [ObserversRpc]
    private void Pickup_Client(NetworkObject holder)
    {
        PickupLogic(holder);

        if (objectToChangeLayer != null)
        {
            objectToChangeLayer.layer = LayerMask.NameToLayer("PickableLayer");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("PickableLayer"); 
        }
    }

    [ObserversRpc]
    private void Drop_Client()
    {
        DropLogic();
        
        if (objectToChangeLayer != null)
        {
            objectToChangeLayer.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default"); 
        }
    }

    protected virtual void PickupLogic(NetworkObject holder)
    {
        transform.SetParent(holder.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (offset != null)
        {
            transform.localPosition = offset.localPosition;
            transform.localRotation = offset.localRotation;
        }
        
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = false;
    }

    protected virtual void DropLogic()
    {
        transform.SetParent(null);
        
        _rb.AddRelativeForce(Vector3.forward * 2f, ForceMode.Impulse);

        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = true;
    }
}
