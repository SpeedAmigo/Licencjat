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
            if (hit.collider.TryGetComponent<StatusEffectHandler>(out var handler))
            {
                ApplyEffect(handler);
            }
        }
    }

    private void ApplyEffect(StatusEffectHandler handler)
    {
        if (effects == null) return;
        
        foreach (var effect in effects)
        {
            handler.ApplyEffect(effect);
        }
    }
}
