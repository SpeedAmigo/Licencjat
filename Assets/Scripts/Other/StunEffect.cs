using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/StunEffect")]
public class StunEffect : StatusEffect
{
    [Server]
    public override void OnApply(StatusEffectInstance instance)
    {
        if (instance.TryGetComponent<IStunable>(out var stunnable))
        {
            stunnable.SetStunned(true);
        }
    }

    [Server]
    public override void OnTick(StatusEffectInstance instance, float deltaTime){}

    [Server]
    public override void OnExpire(StatusEffectInstance instance)
    {
        if (instance.TryGetComponent<IStunable>(out var stunnable))
        {
            stunnable.SetStunned(false);
        }
    }
}
