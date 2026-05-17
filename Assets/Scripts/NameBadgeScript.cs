using System;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class NameBadgeScript : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameBadge;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;
        
        if (Heathen.SteamworksIntegration.API.App.Initialized)
        {
            string name = Heathen.SteamworksIntegration.UserData.Me.Name;
            SetBadgeTextServer(name);
        }
    }

    private void LateUpdate()
    {
        if (IsOwner) return;

        Transform cam = Camera.main.transform;
        transform.forward = cam.forward;
    }

    [ServerRpc]
    private void SetBadgeTextServer(string name)
    {
        SetBadgeTextObservers(name);
    }

    [ObserversRpc]
    private void SetBadgeTextObservers(string name)
    {
        nameBadge.text = name;
    }
}
