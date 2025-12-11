using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    private SoundPlayer _soundPlayer;

    private void Awake()
    {
        _soundPlayer = GetComponent<SoundPlayer>();
    }
    
    public void PlayFootstep()
    {
        //Debug.Log("Footstep left");
        _soundPlayer.PlayRandomGlobal("Footstep");
    }
}
