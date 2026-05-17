using UnityEngine;

public class PlayerColliderScript : PlayerComponent
{
    private Collider _collider;
    
    protected override void Awake()
    {
        base.Awake();
        
        _collider = GetComponent<Collider>();
    }
    
    protected override void DeathHandle()
    {
        _collider.enabled = false;
    }

    protected override void ReviveHandle()
    {
        _collider.enabled = true;
    }
}
