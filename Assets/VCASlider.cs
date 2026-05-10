using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class VCASlider : MonoBehaviour
{
    [SerializeField] private VCAType vcaName;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private float defaultVolume = 0.5f;

    private FMOD.Studio.VCA vca;
    private string vcaPath;

    private void Start()
    {
        vcaPath = $"vca:/{vcaName}";
        vca = RuntimeManager.GetVCA(vcaPath);
        
        float savedVolume = PlayerPrefs.GetFloat(vcaPath, defaultVolume);
        volumeSlider.value = savedVolume;
        vca.setVolume(savedVolume);
        
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        vca.setVolume(volume);
        PlayerPrefs.SetFloat(vcaPath, volume);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(vcaPath, volumeSlider.value);
    }
}

public enum VCAType
{
    Master,
    SFX,
    Environment
}
