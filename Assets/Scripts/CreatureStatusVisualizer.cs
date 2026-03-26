using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreatureStatusVisualizer : MonoBehaviour
{
    [SerializeField] private bool lookAtPlayer = true;
    [Space]
    
    [SerializeField] private Animator questionmarkAnimator;
    [SerializeField] private Animator exclamationAnimator;
    [SerializeField] private Animator angerAnimator;
    
    public bool isEnabled;
    
    public void ShowStatusSign(CreatureStatus status, float duration)
    {
        isEnabled = true;
        Animator animator = null;
        
        switch (status)
        {
            case CreatureStatus.Questionmark :
                animator = questionmarkAnimator;
                break;
            case CreatureStatus.Exclamation :
                animator = exclamationAnimator;
                break;
            case CreatureStatus.Anger :
                animator = angerAnimator;
                break;
        }

        if (animator != null)
        {
            StartCoroutine(ShowStatusCoroutine(animator, duration));
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
        if (!lookAtPlayer) return;

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
