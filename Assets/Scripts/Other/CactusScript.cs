using FishNet.Object;
using UnityEngine;

public class CactusScript : NetworkBehaviour
{
    [SerializeField] private StatusEffect[] damageEffect;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("PlayerCollider")) return;
        
        if (collision.transform.parent.TryGetComponent<NetworkObject>(out var nob))
        {
            Debug.Log(collision.transform.name);
            Debug.Log(collision.transform.parent.name);
            
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
