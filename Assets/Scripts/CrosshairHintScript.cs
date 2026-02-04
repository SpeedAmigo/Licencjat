using TMPro;
using UnityEngine;

public class CrosshairHintScript : PlayerComponent
{
    [SerializeField] private GameObject image;
    [SerializeField] private GameObject[] textHints;

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
    
    private void HandleShowHint(int pickable)
    {
        if (pickable == 0)
        {
            textHints[0].gameObject.SetActive(true);
        }
        else if (pickable == 1)
        {
            textHints[1].gameObject.SetActive(true);
        }
        
        image.SetActive(true);
    }

    private void HandleHideHint()
    {
        foreach (var hint in textHints)
        {
            hint.gameObject.SetActive(false);
        }
        image.SetActive(false);
    }
}
