using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionControlScript : MonoBehaviour
{
    [SerializeField] private TMP_Text currentResolutionText;
    [SerializeField] private TMP_Text currentScreenModeText;
    [SerializeField] private TMP_Text currentFPSCapText;
    
    [SerializeField] private CapRates[] capRates;
    
    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;
    
    private float _currentRefreshRate;
    private int _currentResolutionIndex;
    private int _currentScreenIndex;
    private int _currentCapIndex;
    
    private void Start()
    {
        _currentCapIndex = PlayerPrefs.GetInt("FPS", _currentCapIndex = capRates.Length - 1);
        _currentScreenIndex = PlayerPrefs.GetInt("Screen", _currentScreenIndex = 0);
        
        ApplyScreenMode(_currentScreenIndex);
        ApplyFPSCap(_currentCapIndex);
        ResolutionSetup();
    }

    private void ResolutionSetup()
    {
        _resolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();
        
        _currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (Mathf.Approximately((float)_resolutions[i].refreshRateRatio.value, _currentRefreshRate))
            {
                _filteredResolutions.Add(_resolutions[i]);
            }
        }

        if (PlayerPrefs.HasKey("Resolution"))
        {
            _currentResolutionIndex = PlayerPrefs.GetInt("Resolution");
        }
        else
        {
            for (int i = 0; i < _filteredResolutions.Count; i++)
            {
                Resolution resolution = _filteredResolutions[i];

                if (resolution.width == Screen.currentResolution.width &&
                    resolution.height == Screen.currentResolution.height)
                {
                    _currentResolutionIndex = i;
                    break;
                }
            } 
        }
        
        _currentResolutionIndex = Mathf.Clamp(
            _currentResolutionIndex,
            0,
            _filteredResolutions.Count - 1
        );
        
        UpdateResolutionText();
    }

    public void CycleUp()
    {
        _currentResolutionIndex++;
        
        if (_currentResolutionIndex >= _filteredResolutions.Count)
        {
            _currentResolutionIndex = 0;
        }
        
        SetResolution();
    }

    public void CycleDown()
    {
        _currentResolutionIndex--;

        if (_currentResolutionIndex < 0)
        {
            _currentResolutionIndex = _filteredResolutions.Count - 1;
        }
        
        SetResolution();
    }

    private void SetResolution()
    {
        if (_filteredResolutions.Count == 0) return;
        
        Resolution resolution = _filteredResolutions[_currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        
        PlayerPrefs.SetInt("Resolution", _currentResolutionIndex);
        PlayerPrefs.Save();
        
        UpdateResolutionText();
    }

    private void UpdateResolutionText()
    {
        Resolution resolution = _filteredResolutions[_currentResolutionIndex];
        currentResolutionText.text = $"{resolution.width} x {resolution.height}";
    }
    
    public void CycleUpScreenMode()
    {
        _currentScreenIndex++;
        
        if (_currentScreenIndex > 2)
        {
            _currentScreenIndex = 0;
        }
        
        ApplyScreenMode(_currentScreenIndex);
    }

    public void CycleDownScreenMode()
    {
        _currentScreenIndex--;

        if (_currentScreenIndex < 0)
        {
            _currentScreenIndex = 2;
        }
        
        ApplyScreenMode(_currentScreenIndex);
    }

    private void ApplyScreenMode(int index)
    {
        FullScreenMode pickedMode =  Screen.fullScreenMode;
        string textToDisplay = null;
        
        switch (index)
        {
            case 0:
                pickedMode = FullScreenMode.FullScreenWindow;
                textToDisplay = "Full Screen";
                break;
            case 1:
                pickedMode = FullScreenMode.MaximizedWindow;
                textToDisplay = "Full Screen Window";
                break;
            case 2:
                pickedMode = FullScreenMode.Windowed;
                textToDisplay = "Windowed";
                break;
        }

        PlayerPrefs.SetInt("Screen", index);
        PlayerPrefs.Save();
        Screen.fullScreenMode = pickedMode;
        currentScreenModeText.text = textToDisplay;
    }
    
    public void CycleUpCap()
    {
        _currentCapIndex++;
        
        if (_currentCapIndex >= capRates.Length)
        {
            _currentCapIndex = 0;
        }
        
        ApplyFPSCap(_currentCapIndex);
    }

    public void CycleDownCap()
    {
        _currentCapIndex--;
        
        if (_currentCapIndex < 0)
        {
            _currentCapIndex = capRates.Length - 1;
        }
        
        ApplyFPSCap(_currentCapIndex);
    }
    
    private void ApplyFPSCap(int index)
    {
        Application.targetFrameRate = capRates[index].value;
        PlayerPrefs.SetInt("FPS", index);
        PlayerPrefs.Save();

        if (Application.targetFrameRate == -1)
        {
            currentFPSCapText.text = "Unlimited";
            return;
        }
        
        currentFPSCapText.text = capRates[index].value.ToString();
    }
}

[System.Serializable]
public class CapRates
{
    public int value;
}
