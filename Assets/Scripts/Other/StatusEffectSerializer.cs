using System;
using FishNet.Serializing;
using UnityEngine;

public static class StatusEffectSerializer
{
        public static void WriteStatusEffectData(this Writer writer, StatusEffectData data)
    {
        writer.WriteString(data.typeName ?? string.Empty);
        writer.WriteSingle(data.duration);
    }

    public static StatusEffectData ReadStatusEffectData(this Reader reader)
    {
        var typeName = reader.ReadString();
        var duration = reader.ReadSingle();
        return new StatusEffectData(typeName, duration);
    }
    
    public static void WriteStatusEffect(this Writer writer, StatusEffect statusEffect)
    {
        if (writer == null) return;
        
        bool hasValue = statusEffect != null;
        writer.WriteBoolean(hasValue);
        if (!hasValue) return;
        
        string typeName = statusEffect.GetType().AssemblyQualifiedName ?? string.Empty;
        writer.WriteString(typeName);
        writer.WriteSingle(statusEffect.duration);
    }

    public static StatusEffect ReadStatusEffect(this Reader reader)
    {
        if (reader == null) return null;
        
        bool hasValue = reader.ReadBoolean();
        if (!hasValue) return null;
        
        string typeName = reader.ReadStringAllocated();
        float duration = reader.ReadSingle();
        
        if (string.IsNullOrEmpty(typeName)) return null;
        
        Type t = Type.GetType(typeName, throwOnError: false);
        if (t == null || !typeof(StatusEffect).IsAssignableFrom(t)) return null;

        var effect = (StatusEffect)ScriptableObject.CreateInstance(t);
        effect.duration = duration;
        
        return effect;
    }
}
