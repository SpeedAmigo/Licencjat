using FishNet.Object;
using UnityEngine;

public class BaseInteractable : NetworkBehaviour, IInteractable, IOutlineChangeable
{
    [SerializeField] private string interactText;
    [SerializeField] private Renderer rend;
    
    private MaterialPropertyBlock _propertyBlock;
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

    protected virtual void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void SetOutlineColor(Color outlineColor)
    {
        if (rend == null) return;
        
        rend.GetPropertyBlock(_propertyBlock);
        
        _propertyBlock.SetColor(OutlineColor, outlineColor);
        
        rend.SetPropertyBlock(_propertyBlock);
    }
    
    public virtual void Interact(PlayerRoot playerRoot) {}

    public string GetInteractText()
    {
        return interactText;
    }
}