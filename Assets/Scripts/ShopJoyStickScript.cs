using FishNet.Component.Animating;
using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class ShopJoyStickScript : BaseInteractable
{
    [SerializeField] private EventReference joyStickSound;
    
    private NetworkAnimator _animator;

    private void Start()
    {
        _animator = GetComponent<NetworkAnimator>();
    }
    
    public override void Interact(PlayerRoot playerRoot)
    {
        if (ShopManagerScript.Instance && GlobalDropRule.CanDropItems)
        {
            if (IsController)
            {
                ShopManagerScript.Instance.BuyItems();
                SoundCreator.Instance.PlayOneShot(joyStickSound, transform.position);
                PlayAnimServer();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAnimServer()
    {
        _animator.SetTrigger("Play");
    }
}
