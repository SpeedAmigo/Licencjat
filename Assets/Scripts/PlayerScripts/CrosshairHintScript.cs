using TMPro;
using UnityEngine;

public class CrosshairHintScript : PlayerComponent
{
    [SerializeField] private GameObject image;
    [SerializeField] private TMP_Text textHint;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerInteractor.OnObjectDetection += HandleShowHint;
        PlayerInteractor.OnObjectUnDetection += HandleHideHint;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        PlayerInteractor.OnObjectDetection -= HandleShowHint;
        PlayerInteractor.OnObjectUnDetection -= HandleHideHint;
    }
    
    private void HandleShowHint(string text)
    {
        textHint.gameObject.SetActive(true);
        textHint.text = text;
        
        image.SetActive(true);
    }

    private void HandleHideHint()
    {
        textHint.gameObject.SetActive(false);
        
        image.SetActive(false);
    }
}
