using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionControlScript : MonoBehaviour
{
    [SerializeField] private TMP_Text currentResolutionText;
    
    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;
    
    private float _currentRefreshRate;
    private int _currentResolutionIndex;
    
    private void Start()
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
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        UpdateResolutionText();
    }

    private void UpdateResolutionText()
    {
        Resolution resolution = _filteredResolutions[_currentResolutionIndex];
        currentResolutionText.text = $"{resolution.width} x {resolution.height}";
    }
}
