using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class GameOverManager : NetworkBehaviour
{
    public static GameOverManager Instance;

    public static event Action<string> OnGameOverScreen;

    [SerializeField] private string quotaDeathMessage;
    [SerializeField] private string playersDownDeathMessage;

    [SerializeField] private List<PlayerRoot> players;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayer(NetworkObject player)
    {
        player.TryGetComponent(out PlayerRoot playerRoot);
        if (playerRoot != null)
        {
            players.Add(playerRoot);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ComparePlayersState()
    {
        foreach (PlayerRoot player in players)
        {
            
            if (player.playerState != PlayerStateEnum.Dead)
            {
                Debug.Log("Player: " + player.name);
                return;
            }
        }
        
        GameOverServer(false);
    }

    [Server]
    public void GameOverServer(bool causedByQuota)
    {
        GameOverClients(causedByQuota);
    }

    [ObserversRpc]
    private void GameOverClients(bool causedByQuota)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        
        Time.timeScale = 0;
        
        OnGameOverScreen?.Invoke(causedByQuota ? quotaDeathMessage : playersDownDeathMessage);
    }
}
