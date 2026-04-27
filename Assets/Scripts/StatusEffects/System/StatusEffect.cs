using FishNet.Object;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public float duration;
    
    public abstract void OnApply(StatusEffectInstance instance);
    public abstract void OnTick(StatusEffectInstance instance, float deltaTime);
    public abstract void OnExpire(StatusEffectInstance instance);
}
