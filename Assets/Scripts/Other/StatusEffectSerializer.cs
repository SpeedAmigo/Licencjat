using System;
using FishNet.Serializing;
using UnityEngine;

public static class StatusEffectSerializer
{
    public static void WriteStatusEffect(this Writer writer, StatusEffect statusEffect)
    {
        if (statusEffect == null)
        {
            writer.WriteBoolean(false);
            return;
        }
        
        writer.WriteBoolean(true);
        
        writer.WriteString(statusEffect.GetType().AssemblyQualifiedName);
        writer.WriteSingle(statusEffect.duration);
    }

    public static StatusEffect ReadStatusEffect(this Reader reader)
    {
        bool hasValue = reader.ReadBoolean();
        if (!hasValue) return null;
        
        string typeName = reader.ReadStringAllocated();
        float duration = reader.ReadSingle();
        
        if (string.IsNullOrEmpty(typeName))
            return null;
        
        Type t = Type.GetType(typeName, throwOnError: false);
        if (t == null || !typeof(StatusEffect).IsAssignableFrom(t))
            return null;

        var effect = (StatusEffect)ScriptableObject.CreateInstance(t);
        effect.duration = duration;
        // TODO: read additional fields into 'effect' if needed.
        
        return effect;
    }

    public static void WriteStatusEffectInstance(this Writer writer, StatusEffectInstance statusEffectInstance)
    {
        writer.WriteStatusEffect(statusEffectInstance?.effect);
    }
    
    public static StatusEffectInstance ReadStatusEffectInstance(this Reader reader, StatusEffectHandler target)
    {
        var effect = reader.ReadStatusEffect();
        if (effect == null)
            return null;

        var instance = new StatusEffectInstance(effect, target);
        return instance;
    }
}
