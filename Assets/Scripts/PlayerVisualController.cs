using FishNet.Component.Animating;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerVisualController : NetworkBehaviour
{
    [GUIColor("Red")]
    [SerializeField] private GameObject[] visuals;
    [GUIColor("Red")]
    [SerializeField] private NetworkAnimator networkAnimator;
    [GUIColor("Red")]
    
    private PlayerController _playerController;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            foreach (var visual in  visuals)
            {
                visual.layer = LayerMask.NameToLayer("Player");
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        networkAnimator.Animator.SetFloat("Velocity", _playerController.animatorVelocity);
    }
}
