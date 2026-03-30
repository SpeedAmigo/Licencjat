using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreatureStatusVisualizer : MonoBehaviour
{
    [SerializeField] private Animator questionmarkAnimator;
    [SerializeField] private Animator exclamationAnimator;
    [SerializeField] private Animator angerAnimator;
    
    public bool isEnabled;

    private Animator _pickedAnimator;
    
    public void ShowStatusSign(CreatureStatus status, float duration)
    {
        if (_pickedAnimator != null)
        {
            _pickedAnimator.gameObject.SetActive(false);
            _pickedAnimator = null;
            
            StopAllCoroutines();
        }
        
        isEnabled = true;
        
        switch (status)
        {
            case CreatureStatus.Questionmark :
                _pickedAnimator = questionmarkAnimator;
                break;
            case CreatureStatus.Exclamation :
                _pickedAnimator = exclamationAnimator;
                break;
            case CreatureStatus.Anger :
                _pickedAnimator = angerAnimator;
                break;
        }

        if (_pickedAnimator != null)
        {
            StartCoroutine(ShowStatusCoroutine(_pickedAnimator, duration));
        }
    }

    private IEnumerator ShowStatusCoroutine(Animator animator, float duration)
    {
        animator.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(duration);

        animator.SetTrigger("Exit");
        isEnabled = false;
        
        yield return new WaitForSeconds(0.5f);
        
        animator.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (isEnabled)
        {
            gameObject.transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }
}

public enum CreatureStatus
{
    Questionmark = 0,
    Exclamation = 1,
    Anger = 2
}
