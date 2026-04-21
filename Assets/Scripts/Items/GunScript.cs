using DG.Tweening;
using FishNet.Object;
using UnityEngine;

public class GunScript : Weapon, IPrimaryClick, IRechargeable
{
    [SerializeField] private Transform raycastStartPoint;
    [SerializeField] private float shootDistance;
    
    [SerializeField] private StatusEffect[] effects;
    
    public void OnPrimaryClick()
    {
        if (!CheckDurability())
        {
            Debug.Log("No more power!");
            return;
        }

        Camera.main.DOShakePosition(0.3f, 0.03f, 10, 90f, true);
        
        DecreaseDurability();
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

       if (handler == null)
       {
           Debug.Log("No status effect handler");
           return;
       }
       
       handler.ApplyEffects(effects);
    }

    [ServerRpc(RequireOwnership = false)]
    public void Recharge()
    {
        Debug.Log("Recharge");
        SetToMaxDurability();
    }
}
