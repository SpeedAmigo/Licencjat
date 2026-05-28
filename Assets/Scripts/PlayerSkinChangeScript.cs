using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerSkinChangeScript : BaseInteractable
{
    [SerializeField] private Material faceMaterial;
    [SerializeField] private Material skinMaterial;
    [SerializeField] private Material helmetMaterial;
    
    public override void Interact(PlayerRoot player)
    {
        ChangePlayerSkinServer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerSkinServer(PlayerRoot playerRoot)
    {
        ChangePlayerSkinObserver(playerRoot);
    }

    [ObserversRpc]
    private void ChangePlayerSkinObserver(PlayerRoot playerRoot)
    {
        Renderer bodyRenderer = playerRoot.GetPlayerBody().GetComponent<Renderer>();
        Renderer faceRenderer = playerRoot.GetPlayerFace().GetComponent<Renderer>();
        Renderer helmetRenderer = playerRoot.GetPlayerHelmet().GetComponent<Renderer>();
        
        bodyRenderer.material = skinMaterial;
        faceRenderer.material = faceMaterial;
        
        Material[] materials = helmetRenderer.materials;
        materials[1] = helmetMaterial;
        
        helmetRenderer.materials = materials;
    }
}
