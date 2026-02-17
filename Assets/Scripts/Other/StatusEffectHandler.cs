using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class StatusEffectHandler : NetworkBehaviour
{
    private List<StatusEffectInstance> activeEffects = new();
    
    public void ApplyEffect(StatusEffect effect)
    {
        StatusEffectInstance instance = new StatusEffectInstance(effect, this);
        activeEffects.Add(instance);
        instance.OnApply();
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Tick(Time.deltaTime);

            if (activeEffects[i].IsFinished)
            {
                activeEffects[i].OnExpire();
                activeEffects.RemoveAt(i);
            }
        }
    }
}
