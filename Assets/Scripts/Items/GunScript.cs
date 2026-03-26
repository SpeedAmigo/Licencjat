using FishNet.Object;
using UnityEngine;

public class GunScript : Weapon, IPrimaryClick
{
    [SerializeField] private Transform raycastStartPoint;
    [SerializeField] private float shootDistance;
    
    [SerializeField] private StatusEffect[] effects;
    
    public void OnPrimaryClick()
    {
        Shoot();
    }

    private void Shoot()
    {
        Ray ray = new Ray(raycastStartPoint.position, raycastStartPoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
        {
            if (hit.collider.TryGetComponent<NetworkObject>(out var nob))
            {
                TryApplyEffect(nob);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryApplyEffect(NetworkObject target)
    {
       var handler = target.GetComponent<StatusEffectHandler>();
       handler.ApplyEffects(effects);
    }
}
