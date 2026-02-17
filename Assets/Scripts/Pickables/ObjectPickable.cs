using System;
using FishNet.Object;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;


public abstract class ObjectPickable : NetworkBehaviour
{
    [InfoBox("if 'Separate Collider' unchecked remember to add collider at root object")]
    public bool useSeparateCollider = false;
    
    [ShowIf("useSeparateCollider")]
    [GUIColor("Yellow")]
    public Collider objectCollider;
    
    public bool changeLayerOnPickup = false;
    
    [ShowIf("changeLayerOnPickup")] 
    [GUIColor("Yellow")]
    public GameObject[] objectsToChangeLayer;
    
    [Space]
    
    [GUIColor("Green")]
    public Transform offset;

    public float dropForce = 5f;
    public bool isBig;
    
    private Rigidbody _rb;
    private Collider _col;
    private Collider _secondCol;
    
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (objectCollider == null)
        {
            _col = GetComponent<Collider>();
        }
        else
        {
            _col = GetComponent<Collider>();
            _secondCol = objectCollider;
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
        if (objectsToChangeLayer != null && changeLayerOnPickup)
        {
            foreach (var obj in objectsToChangeLayer)
            {
                obj.gameObject.layer = LayerMask.NameToLayer("PickableLayer");
            }
        }
        else
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("PickableLayer");
            }
        }
    }

    [ObserversRpc]
    public void Drop_Client()
    {
        if (IsSpawned)
        {
            DropLogic();
        }
        
        if (objectsToChangeLayer != null && changeLayerOnPickup)
        {
            foreach (var obj in objectsToChangeLayer)
            {
                obj.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        else
        {
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
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
        if (_secondCol != null)
        {
            _secondCol.enabled = false;
        }
    }
    
    protected virtual void DropLogic()
    {
        transform.SetParent(null);
        
        _rb.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);

        _rb.isKinematic = false;
        _rb.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = true;
        if (_secondCol != null)
        {
            _secondCol.enabled = true;
        }
    }
}
