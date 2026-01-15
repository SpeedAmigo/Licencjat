using FishNet.Object;
using UnityEngine;
using UnityEngine.Playables;

public class ShipConsoleButton : NetworkBehaviour, IInteractable
{
    [SerializeField] private SpaceShipConsoleScript consoleScript;
    
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool playOnLanded;
    
    public void Interact()
    {
        Debug.Log("Interact");
        
        if (!consoleScript.shipPending.Value && consoleScript.shipLanded.Value == playOnLanded)
        {
            Debug.Log($"ship status: {consoleScript.shipLanded.Value} button status: {playOnLanded}");
            playableDirector.Play();
            TimelineStart_Clients();
            SpawnHandle(consoleScript.shipLanded.Value);
        }
        else
        {
            Debug.Log($"ship status: {consoleScript.shipLanded.Value} button status: {playOnLanded}");
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void TimelineStart_Clients()
    {
        playableDirector.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnHandle(bool landed)
    {
        if (!IsServerInitialized) return;
        
        if (!landed)
        {
            SpawnerManager.Instance.StartSpawning();
        }
        else
        {
            SpawnerManager.Instance.RemoveSpawnedObjects();
        }
    }
}
