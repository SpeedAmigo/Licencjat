using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionControlScript : MonoBehaviour
{
    [SerializeField] private TMP_Text currentResolutionText;
    [SerializeField] private TMP_Text currentScreenModeText;
    
    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;
    
    private float _currentRefreshRate;
    private int _currentResolutionIndex;
    private int _currentScreenIndex;
    
    private void Start()
    {
        ApplyScreenMode(_currentScreenIndex = 0);
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
        
        for (int i = 0; i < _filteredResolutions.Count; i++)
        {
            if (_filteredResolutions[i].width == Screen.width && _filteredResolutions[i].height == Screen.height)
            {
                _currentResolutionIndex = i;
                break;
            }
        }
        
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
        Resolution resolution = _filteredResolutions[_currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
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
                pickedMode = FullScreenMode.ExclusiveFullScreen;
                textToDisplay = "Full Screen";
                break;
            case 1:
                pickedMode = FullScreenMode.FullScreenWindow;
                textToDisplay = "Full Screen Window";
                break;
            case 2:
                pickedMode = FullScreenMode.Windowed;
                textToDisplay = "Windowed";
                break;
        }

        Screen.fullScreenMode = pickedMode;
        currentScreenModeText.text = textToDisplay;
    }
}
