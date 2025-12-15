using FishNet.Object;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FootstepPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject surfaceCheckRaycast;
    [SerializeField] private float distance;
    
    [SerializeField] private StudioEventEmitter emitter;
    
    public void PlayFootstep()
    {
        if (!IsOwner) return;
        if (!surfaceCheckRaycast) return;

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

    [ObserversRpc]
    private void PlaySoundClient(int surface)
    {
        emitter.SetParameter("FootstepParameter", surface);
        emitter.Play();
    }
    
    private void Update()
    {
        Debug.DrawRay(surfaceCheckRaycast.transform.position, Vector3.down * distance, Color.red);
    }
}
