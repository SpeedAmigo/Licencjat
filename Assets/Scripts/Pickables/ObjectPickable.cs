using System;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;


public class ObjectPickable : NetworkBehaviour
{
    [InfoBox("if 'Separate Collider' unchecked remember to add collider at root object")]
    public bool useSeparateCollider = false;
    
    [ShowIf("useSeparateCollider")]
    [GUIColor("Yellow")]
    public Collider objectCollider;
    
    public bool changeLayerOnPickup = false;
    
    [ShowIf("changeLayerOnPickup")] 
    [GUIColor("Yellow")]
    public GameObject objectToChangeLayer;
    
    [Space]
    
    [GUIColor("Green")]
    public Transform offset;
    [GUIColor("Yellow")]
    public Sprite itemIcon;
        
    public bool isBig;
    
    private Rigidbody _rb;
    private Collider _col;
    
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (objectCollider == null)
        {
            _col = GetComponent<Collider>();
        }
        else
        {
            _col = objectCollider;
        }
    }
    
    public void Pickup(NetworkObject fpHolder, NetworkObject tpHolder)
    {
        if (!IsServerInitialized) return;
        PickupLogic(fpHolder);
        Pickup_Client(fpHolder, tpHolder);
    }

    public void Drop()
    {
        if (!IsServerInitialized) return;
        DropLogic();
        Drop_Client();
    }

    [ObserversRpc]
    public void Pickup_Client(NetworkObject fpHolder, NetworkObject tpHolder)
    {
        if (fpHolder.IsOwner)
        {
            PickupLogic(fpHolder);
        }
        else
        {
            PickupLogic(tpHolder);
        }

        if (!fpHolder.IsOwner) return;
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
    public void Drop_Client()
    {
        if (IsSpawned)
        {
            DropLogic();
        }
        
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
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        _col.enabled = true;
    }
}
