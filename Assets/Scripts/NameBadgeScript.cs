using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class NameBadgeScript : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameBadge;

    private readonly SyncVar<string> playerName = new SyncVar<string>();

    private void Awake()
    {
        playerName.OnChange += OnNameChanged;
    }

    private void OnDestroy()
    {
        playerName.OnChange -= OnNameChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            if (Heathen.SteamworksIntegration.API.App.Initialized)
            {
                string steamName = Heathen.SteamworksIntegration.UserData.Me.Name;
                SetPlayerNameServerRpc(steamName);
            }
        }

        // Update immediately for late joiners
        nameBadge.text = playerName.Value;
    }

    private void LateUpdate()
    {
        if (IsOwner) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }

    private void OnNameChanged(string oldName, string newName, bool asServer)
    {
        nameBadge.text = newName;
    }
}