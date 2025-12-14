using FishNet.Object;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ShipConsoleButton : NetworkBehaviour, IInteractable
{
    [SerializeField] private PlayableDirector playableDirector;
    
    public void Interact()
    {
        Debug.Log("Interact");
        playableDirector.Play();
        TimelineStart_Clients();
    }

    [ObserversRpc(BufferLast = true)]
    private void TimelineStart_Clients()
    {
        playableDirector.Play();
    }
}
