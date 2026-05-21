using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class CreatureStatusVisualizer : NetworkBehaviour
{
    [SerializeField] private Animator questionmarkAnimator;
    [SerializeField] private Animator exclamationAnimator;
    [SerializeField] private Animator angerAnimator;
    [SerializeField] private Animator starAnimator;
    
    public bool isEnabled;

    private Animator _pickedAnimator;
    
    public void ShowStatusSign(CreatureStatus status, float duration)
    {
        if (IsServerInitialized)
        {
            ShowStatusObservers(status, duration);
        }
        else
        {
            ShowStatusServer(status, duration);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowStatusServer(CreatureStatus status, float duration)
    {
        ShowStatusObservers(status, duration);
    }

    [ObserversRpc]
    private void ShowStatusObservers(CreatureStatus status, float duration)
    {
        PlayStatusLocally(status, duration);
    }
    
    private void PlayStatusLocally(CreatureStatus status, float duration)
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
            case CreatureStatus.Star :
                _pickedAnimator = starAnimator;
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
            _pickedAnimator.transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }
}

public enum CreatureStatus
{
    Questionmark = 0,
    Exclamation = 1,
    Anger = 2,
    Star = 3
}
