using FishNet.Component.Animating;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerVisualController : PlayerComponent
{
    [GUIColor("Red")]
    [SerializeField] private GameObject[] visuals;
    [GUIColor("Red")]
    [SerializeField] private NetworkAnimator networkAnimator;
    [GUIColor("Red")]
    [SerializeField] private Animator animator;
    [GUIColor("Red")]
    
    private PlayerController _playerController;
    
    protected override void Awake()
    {
        base.Awake();
        _playerController = GetComponent<PlayerController>();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!IsOwner)
        {
            ChangeVisualRender(true);
        }
        else
        {
            ChangeVisualRender(false);
        }
    }
    
    public void ChangeVisualRender(bool visible)
    {
        foreach (var visual in  visuals)
        {
            Renderer renderer = visual.GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogWarning($"Visual {visual.name} has no renderer");
                continue;
            }
            
            renderer.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            
            //visual.layer = LayerMask.NameToLayer(layerName);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        networkAnimator.Animator.SetFloat("Velocity", _playerController.animatorVelocity);
    }
    
    protected override void DeathHandle()
    {
        base.DeathHandle();
        
        AnimatorHandleServer(true);
    }
    
    protected override void ReviveHandle()
    {
        base.ReviveHandle();
        
        AnimatorHandleServer(false);
    }
    
    [ServerRpc(RequireOwnership = true)]
    private void AnimatorHandleServer(bool hasDied)
    {
        AnimatorHandleClient(hasDied);
    }

    [ObserversRpc(BufferLast = true)]
    private void AnimatorHandleClient(bool hasDied)
    {
        animator.enabled = !hasDied;
        networkAnimator.enabled = !hasDied;
    }
}
