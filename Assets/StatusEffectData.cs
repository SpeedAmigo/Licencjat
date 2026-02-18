using UnityEngine;

[System.Serializable]
public struct StatusEffectData
{
    public string typeName;
    public float duration;

    public StatusEffectData(string typeName, float duration)
    {
        this.typeName = typeName;
        this.duration = duration;
    }

    public StatusEffect CreateEffect()
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        var t = System.Type.GetType(typeName, throwOnError: false);
        if (t == null || !typeof(StatusEffect).IsAssignableFrom(t))
            return null;

        var effect = (StatusEffect)ScriptableObject.CreateInstance(t);
        effect.duration = duration;
        return effect;
    }

    public static StatusEffectData FromEffect(StatusEffect effect)
    {
        var tn = effect != null ? effect.GetType().AssemblyQualifiedName : string.Empty;
        var dur = effect != null ? effect.duration : 0f;
        return new StatusEffectData(tn, dur);
    }
}
