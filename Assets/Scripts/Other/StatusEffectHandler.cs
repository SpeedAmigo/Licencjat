using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class StatusEffectHandler : NetworkBehaviour
{
    public List<StatusEffectInstance> activeEffects;
    //[AllowMutableSyncType] private SyncList<StatusEffectInstance> activeEffects = new();
    
    [ServerRpc(RequireOwnership = false)]
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
