using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class CreatureWalkSoundScript : NetworkBehaviour
{
    [SerializeField] private GameObject surfaceCheckRaycast;
    [SerializeField] private float distance;
    
    [SerializeField] private StudioEventEmitter emitter;

    public void WalkSound()
    {
        if (!IsServerInitialized) return;
        
        if (Physics.Raycast(surfaceCheckRaycast.transform.position, Vector3.down, out var hit, distance))
        {
            switch (hit.collider.tag)
            {
                case "Ground":
                    PlaySoundServer(0);
                    break;
                case "Metal":
                    PlaySoundServer(1);
                    break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaySoundServer(int surface)
    {
        PlaySoundClient(surface);
    }

    [ObserversRpc(ExcludeOwner = false)]
    private void PlaySoundClient(int surface)
    {
        emitter.Play();
        emitter.SetParameter("FootstepParameter", surface);
    }
    
    private void Update()
    {
        Debug.DrawRay(surfaceCheckRaycast.transform.position, Vector3.down * distance, Color.red);
    }
}
