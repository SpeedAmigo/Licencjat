using FishNet.Object;
using UnityEngine;
using UnityEngine.Scripting;

public abstract class PlayerComponent : NetworkBehaviour
{
    public PlayerRoot playerRoot;

    protected virtual void Awake()
    {
        playerRoot = gameObject.transform.root.GetComponent<PlayerRoot>();
    }

    protected virtual void OnEnable()
    {
        if (playerRoot == null) return;
        playerRoot.OnReviveEvent += ReviveHandle;
        
        if (playerRoot.oxygen == null) return;
        playerRoot.oxygen.OnDieEvent += DeathHandle;
        
        if (playerRoot == null) return;
        playerRoot.SpectateEvent += SpectateHandle;
    }

    protected virtual void OnDisable()
    {
        if (playerRoot == null) return;
        playerRoot.OnReviveEvent -= ReviveHandle;
        
        if (playerRoot.oxygen == null) return;
        playerRoot.oxygen.OnDieEvent -= DeathHandle;

        if (playerRoot == null) return;
        playerRoot.SpectateEvent -= SpectateHandle;
    }

    [Preserve]
    protected virtual void DeathHandle() {}

    [Preserve]
    protected virtual void ReviveHandle() {}
    
    [Preserve]
    protected virtual void SpectateHandle(bool value) {}
}
