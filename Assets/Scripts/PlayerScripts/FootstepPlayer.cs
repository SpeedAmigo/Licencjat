using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [SerializeField] private GameObject surfaceCheckRaycast;
    [SerializeField] private float distance;
    
    private SoundPlayer _soundPlayer;

    private void Awake()
    {
        _soundPlayer = GetComponent<SoundPlayer>();
    }
    
    public void PlayFootstep()
    {
        if (!surfaceCheckRaycast) return;

        if (Physics.Raycast(surfaceCheckRaycast.transform.position, Vector3.down, out var hit, distance))
        {
            switch (hit.collider.tag)
            {
                  case "Ground":
                      _soundPlayer.PlayRandomGlobal("Ground");
                      break;
                  case "Metal":
                      _soundPlayer.PlayRandomGlobal("Metal");
                      break;
            }
        }
    }

    private void Update()
    {
        Debug.DrawRay(surfaceCheckRaycast.transform.position, Vector3.down * distance, Color.red);
    }
}
