using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class ShopJoyStickScript : NetworkBehaviour, IInteractable
{
    [SerializeField] private string interactText;
    
    private NetworkAnimator _animator;

    private void Start()
    {
        _animator = GetComponent<NetworkAnimator>();
    }
    
    public void Interact(PlayerRoot playerRoot)
    {
        if (ShopManagerScript.Instance)
        {
            if (IsController)
            {
                ShopManagerScript.Instance.BuyItems();
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
