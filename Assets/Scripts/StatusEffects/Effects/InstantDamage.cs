using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/InstantDamage")]
public class InstantDamage : StatusEffect
{
    [SerializeField] private float damage;
    
    [Server]
    public override void OnApply(StatusEffectInstance instance)
    {
        if (instance.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
    public override void OnTick(StatusEffectInstance instance, float deltaTime)
    { }

    public override void OnExpire(StatusEffectInstance instance)
    { }
}
