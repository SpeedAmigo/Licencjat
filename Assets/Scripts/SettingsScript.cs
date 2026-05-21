using System;
using UnityEngine;

public class SettingsScript : MonoBehaviour
{
    public static SettingsScript Instance;

    public static event Action<bool> headBobSetting;
    
    public bool headBobEnabled = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        headBobSetting?.Invoke(headBobEnabled);
    }

    public void HeadBobSettingsChange(bool value)
    {
        headBobEnabled = value;
        headBobSetting?.Invoke(value);
    }
}