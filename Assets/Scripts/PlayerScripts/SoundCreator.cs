using FishNet.Object;
using FMODUnity;
using UnityEngine;

public class SoundCreator : NetworkBehaviour
{
    public static SoundCreator Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void PlayOneShot(EventReference sound, Vector3 position)
    {
        PlayOneShotClients(sound, position);
    }

    [ObserversRpc]
    private void PlayOneShotClients(EventReference sound, Vector3 position)
    {
        RuntimeManager.PlayOneShot(sound, position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayOneShotAttached(EventReference sound, GameObject position)
    {
        PlayOneShotAttachedClients(sound, position);
    }
    
    [ObserversRpc]
    private void PlayOneShotAttachedClients(EventReference sound, GameObject position)
    {
        RuntimeManager.PlayOneShotAttached(sound, position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayOneShotWithParameter(EventReference sound, ParameterValues parameterValues, Vector3 position)
    {
        PlayOneShotWithParameterClients(sound, parameterValues, position);
    }

    [ObserversRpc]
    private void PlayOneShotWithParameterClients(EventReference sound, ParameterValues parameterValues, Vector3 position)
    {
        RuntimeManager.PlayOneShotWithParameter(sound, parameterValues, position);
    }
}
