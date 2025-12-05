using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class PlayerVisualController : NetworkBehaviour
{
    [SerializeField] private GameObject[] visuals;

    [SerializeField] private NetworkAnimator networkAnimator;
    
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject spineBend;
    
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

    private void LateUpdate()
    {
        if (!IsOwner) return;
        
        var negated = Quaternion.Inverse(cameraHolder.transform.localRotation);
        spineBend.transform.localRotation = negated;
    }
}
