using FishNet.Object;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;

public class ShipConsoleButton : NetworkBehaviour, IInteractable
{
    [SerializeField] private SpaceShipConsoleScript consoleScript;
    
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool playOnLanded;
    
    [Space]
    
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private string interactText = "Interact";
    
    public void Interact(PlayerRoot playerRoot)
    {
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
        
        if (emitter)
        {
            emitter.Play();
        }
    }

    public string GetInteractText()
    {
        return interactText;
    }

    [ObserversRpc]
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
            SpawnerManager.Instance.RemoveSpawnedEggs();
        }
    }
}
