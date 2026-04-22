using FishNet.Component.Animating;
using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class ShopJoyStickScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    [SerializeField] private EventReference joyStickSound;
    
    private NetworkAnimator _animator;

    private void Start()
    {
        _animator = GetComponent<NetworkAnimator>();
    }
    
    public void Interact(PlayerRoot playerRoot)
    {
        if (ShopManagerScript.Instance && GlobalDropRule.CanDropItems)
        {
            if (IsController)
            {
                ShopManagerScript.Instance.BuyItems();
                RuntimeManager.PlayOneShot(joyStickSound, transform.position);
                PlayAnimServer();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAnimServer()
    {
        _animator.SetTrigger("Play");
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
