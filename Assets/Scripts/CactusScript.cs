using FishNet.Object;
using UnityEngine;

public class CactusScript : NetworkBehaviour
{
    [SerializeField] private StatusEffect[] damageEffect;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent<NetworkObject>(out var nob))
        {
            TryApplyEffect(nob);
            Debug.Log(nob.name);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryApplyEffect(NetworkObject nob)
    {
        var handler = nob.GetComponent<StatusEffectHandler>();
        Debug.Log(handler);
        
        
        handler.ApplyEffects(damageEffect);
    }
}
