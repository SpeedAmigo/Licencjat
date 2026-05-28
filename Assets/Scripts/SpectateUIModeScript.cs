using FishNet.Object;
using UnityEngine;

public class SpectateUIModeScript : PlayerComponent
{
    [SerializeField] private GameObject[] uiToDeactivate;

    protected override void SpectateHandle(bool value)
    {
        /*foreach (var ui in uiToDeactivate)
        {
            ui.SetActive(!value);
        }*/
        
        ObjectHandleServer(value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ObjectHandleServer(bool value)
    {
        ObjectHandleObservers(value);
    }

    [ObserversRpc]
    private void ObjectHandleObservers(bool value)
    {
        foreach (var ui in uiToDeactivate)
        {
            ui.SetActive(!value);
        }
    }
}
