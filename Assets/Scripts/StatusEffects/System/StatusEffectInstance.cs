using FishNet.Object;
using UnityEngine;

[System.Serializable]
public class StatusEffectInstance
{
    public StatusEffect effect;
    
    private StatusEffectHandler _target;
    private float _timer;
    
    public bool IsFinished => _timer >= effect.duration;

    public StatusEffectInstance(StatusEffect effect, StatusEffectHandler target)
    {
        this.effect = effect;
        _target = target;
        _timer = 0f;
    }
    
    [Server]
    public void OnApply() => effect.OnApply(this);
    
    [Server]
    public void Tick(float deltaTime)
    {
        _timer += deltaTime;
        Debug.Log(_timer);
        effect.OnTick(this, deltaTime);
    }
    
    [Server]
    public void OnExpire() => effect.OnExpire(this);
    
    public T GetComponent<T>() => _target.GetComponent<T>();
    public bool TryGetComponent<T>(out T component) => _target.TryGetComponent<T>(out component);
}