using System;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Demo.Prediction.Rigidbodies;
using FishNet.Object;
using FishNet.Object.Prediction;
using GameKit.Dependencies.Utilities;
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
    
    private NetworkTransform _nt;
    //protected Rigidbody _rb;
    private Collider _col;
    private Collider _secondCol;

    protected PredictionRigidbody rbPrediction;
    
    private Transform _tpTransform;
    
    protected virtual void Awake()
    {
        //_rb = GetComponent<Rigidbody>();
        _nt = GetComponent<NetworkTransform>();
        
        rbPrediction = ObjectCaches<PredictionRigidbody>.Retrieve();
        rbPrediction.Initialize(GetComponent<Rigidbody>());

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
    
    public virtual void Pickup(NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        if (!IsServerInitialized) return;
        NetworkObject.GiveOwnership(conn);
        PickupLogic(fpHolder, conn);
        Pickup_Client(fpHolder, tpHolder, conn);
    }

    public void Drop(Vector3 forward)
    {
        if (!IsServerInitialized) return;
        
        NetworkObject.RemoveOwnership();
        //DropLogic(forward);
        Drop_Client(forward);
    }

    [ObserversRpc]
    private void Pickup_Client(NetworkObject fpHolder, NetworkObject tpHolder, NetworkConnection conn)
    {
        if (fpHolder.IsOwner)
        {
            PickupLogic(fpHolder, conn);
        }
        else
        {
            PickupLogic(tpHolder, conn);
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
    private void Drop_Client(Vector3 forward)
    {
        if (IsSpawned)
        {
            DropLogic(forward);
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
    
    protected virtual void PickupLogic(NetworkObject holder, NetworkConnection conn)
    {
        // this was added
        _nt.enabled = false;
        
        transform.SetParent(holder.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        if (offset != null)
        {
            transform.localPosition = offset.localPosition;
            transform.localRotation = offset.localRotation;
        }
        
        //_rb.isKinematic = true;
        rbPrediction.Rigidbody.isKinematic = true;
        //_rb.interpolation = RigidbodyInterpolation.None;
        rbPrediction.Rigidbody.interpolation = RigidbodyInterpolation.None;
        
        _col.enabled = false;
        if (_secondCol != null)
        {
            _secondCol.enabled = false;
        }
    }
    
    protected virtual void DropLogic(Vector3 forward)
    {
        // this was added
        _nt.enabled = true;
        
        
        transform.SetParent(null);
        
        _nt.Teleport();
        
        //_rb.AddRelativeForce(Vector3.forward * dropForce, ForceMode.Impulse);
        
        //_rb.isKinematic = false;
        rbPrediction.Rigidbody.isKinematic = false;
        //_rb.interpolation = RigidbodyInterpolation.None;
        rbPrediction.Rigidbody.interpolation = RigidbodyInterpolation.None;
        
        //_rb.linearVelocity = Vector3.zero;
        rbPrediction.Rigidbody.linearVelocity = Vector3.zero;
        //_rb.AddForce(forward * dropForce, ForceMode.Impulse);
        rbPrediction.Rigidbody.AddForce(forward * dropForce, ForceMode.Impulse);
        
        _col.enabled = true;
        if (_secondCol != null)
        {
            _secondCol.enabled = true;
        }
    }
}
