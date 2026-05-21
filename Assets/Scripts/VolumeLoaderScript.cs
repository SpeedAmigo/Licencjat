using FMODUnity;
using UnityEngine;

public class VolumeLoaderScript : MonoBehaviour
{
    private FMOD.Studio.VCA vca;
    private string vcaPath;
    
    private void Awake()
    {
        LoadVolumeValues();
    }

    private void LoadVolumeValues()
    {
        foreach (var tag in System.Enum.GetNames(typeof(VCAType)))
        {
            vcaPath = $"vca:/{tag}";
            vca = RuntimeManager.GetVCA(vcaPath);
            
            float savedVolume = PlayerPrefs.GetFloat(vcaPath, 1f);
            vca.setVolume(savedVolume);
        }
    }
}
