using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class StatusEffectHandler : NetworkBehaviour
{
    public List<StatusEffectInstance> activeEffects = new();
    
    private float _timer = 0f;
    private float _interval = 0.2f;
    
    public void ApplyEffect(StatusEffect effect)
    {
        if (!IsServerInitialized) return;
        
        StatusEffectInstance instance = new StatusEffectInstance(effect, this);
        activeEffects.Add(instance);
        instance.OnApply();
    }

    public void ApplyEffects(StatusEffect[] effects)
    {
        if (!IsServerInitialized) return;
        
        if (effects.Length == 0) return;

        foreach (var effect in effects)
        {
            StatusEffectInstance instance = new StatusEffectInstance(effect, this);
            activeEffects.Add(instance);
            instance.OnApply();
        }
    }
    
    private void Update()
    {
        if (!IsServerInitialized) return;
        if (activeEffects.Count == 0) return;
        
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer -= _interval;
            HandleEffects();
        }
    }

    private void HandleEffects()
    {
        if (activeEffects.Count == 0) return;
        
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Tick(_interval);

            if (activeEffects[i].IsFinished)
            {
                activeEffects[i].OnExpire();
                activeEffects.RemoveAt(i);
            }
        }
    }
}
