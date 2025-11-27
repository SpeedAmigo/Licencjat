using FishNet.Object;
using UnityEngine;

public class ItemHolderScript : NetworkBehaviour
{
    [SerializeField] private Transform camera;
    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(camera.forward, camera.up);
    }
}
