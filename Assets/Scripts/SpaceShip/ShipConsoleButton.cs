using FishNet.Object;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;

public class ShipConsoleButton : BaseInteractable
{
    [SerializeField] private SpaceShipConsoleScript consoleScript;
    
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private bool playOnLanded;
    
    [Space]
    [SerializeField] private StudioEventEmitter emitter;
    
    public override void Interact(PlayerRoot playerRoot)
    {
        if (!consoleScript.shipPending.Value && consoleScript.shipLanded.Value == playOnLanded)
        {
            Debug.Log($"ship status: {consoleScript.shipLanded.Value} button status: {playOnLanded}");
            playableDirector.Play();
            TimelineStart_Clients();
        }
        else
        {
            Debug.Log($"ship status: {consoleScript.shipLanded.Value} button status: {playOnLanded}");
        }
        
        //SpawnHandle(consoleScript.shipLanded.Value, consoleScript.shipPending.Value);
        
        if (emitter)
        {
            emitter.Play();
        }
    }
    
    [ObserversRpc]
    private void TimelineStart_Clients()
    {
        playableDirector.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnHandle(bool landed, bool pending)
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