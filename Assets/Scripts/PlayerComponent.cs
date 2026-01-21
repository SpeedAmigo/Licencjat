using FishNet.Object;
using UnityEngine;

public abstract class PlayerComponent : NetworkBehaviour
{
    public PlayerRoot playerRoot;

    protected virtual void Awake()
    {
        playerRoot = gameObject.transform.root.GetComponent<PlayerRoot>();
        //playerRoot = GetComponentInParent<PlayerRoot>();
    }

    protected virtual void OnEnable()
    {
        if (playerRoot == null || playerRoot.oxygen == null) return;
        
        playerRoot.oxygen.OnDieEvent += DeathHandle;
        playerRoot.oxygen.OnReviveEvent += ReviveHandle;
    }

    protected virtual void OnDisable()
    {
        if (playerRoot == null || playerRoot.oxygen == null) return;
        
        playerRoot.oxygen.OnDieEvent -= DeathHandle;
        playerRoot.oxygen.OnReviveEvent -= ReviveHandle;
    }
    
    protected abstract void DeathHandle();
    protected abstract void ReviveHandle();
}
