using FishNet.Object;
using UnityEngine;

public class CactusScript : NetworkBehaviour
{
    [SerializeField] private StatusEffect[] damageEffect;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.parent.TryGetComponent<NetworkObject>(out var nob))
        {
            if (nob.IsOwner)
            {
                TryApplyEffect(nob);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryApplyEffect(NetworkObject nob)
    {
        var handler = nob.GetComponent<StatusEffectHandler>();
        handler.ApplyEffects(damageEffect);
    }
}
