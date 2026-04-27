using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UsageFillScript : NetworkBehaviour
{
    [SerializeField] private GameObject fillParent;
    [SerializeField] private Image fillImage;

    private float _fillTime;
    private float _elapsedTime;
    private bool _fillStarted;
    
    public override void OnStartClient()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        PlayerUsageManager.StartUsage += StartFill;
        PlayerUsageManager.StopUsage += StopFill;
    }

    private void OnDisable()
    {
        PlayerUsageManager.StartUsage -= StartFill;
        PlayerUsageManager.StopUsage -= StopFill;
    }

    private void StartFill(float fillTime)
    {
        fillParent.SetActive(true);
        fillImage.fillAmount = 0;
        _fillTime = fillTime;
        _elapsedTime = 0;
        
        _fillStarted = true;
    }

    private void StopFill()
    {
        _fillStarted = false;
        fillParent.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_fillStarted)
        {
            _elapsedTime += Time.deltaTime;
            fillImage.fillAmount = _elapsedTime / _fillTime;

            if (_elapsedTime >= _fillTime)
            {
                fillImage.fillAmount = 1;
                StopFill();
            }
        }
    }
}
