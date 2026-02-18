using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class StatusEffectHandler : NetworkBehaviour
{
    [AllowMutableSyncType] private SyncList<StatusEffectData> activeEffects = new();
    
    private List<StatusEffectInstance> _serverInstances = new();
    
    [ServerRpc(RequireOwnership = false)]
    public void ApplyEffect(StatusEffect effect)
    {
        var data = StatusEffectData.FromEffect(effect);
        activeEffects.Add(data);
        
        var instance = new StatusEffectInstance(effect, this);
        instance.OnApply();
        
        _serverInstances.Add(instance);
    }
    
    private void Update()
    {
        if (!IsServerInitialized) return;
        
        for (int i = _serverInstances.Count - 1; i >= 0; i--)
        {
            var inst = _serverInstances[i];
            inst.Tick(Time.deltaTime);

            if (inst.IsFinished)
            {
                inst.OnExpire();
                _serverInstances.RemoveAt(i);
                RemoveMatchingData(inst.effect);
            }
        }
    }
    
    private void RemoveMatchingData(StatusEffect effect)
    {
        var tn = effect.GetType().AssemblyQualifiedName;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].typeName == tn)
            {
                activeEffects.RemoveAt(i);
                break;
            }
        }
    }
}
