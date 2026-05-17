using System.Collections;
using TMPro;
using UnityEngine;

public class MessageShowerScript : MonoBehaviour
{
    public static MessageShowerScript Instance;

    [SerializeField] private TMP_Text messageText;
    
    private Coroutine _currentCoroutine = null;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ShowMessage(string message, float duration)
    {
        if (_currentCoroutine != null) return;
        
        _currentCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        
        yield return new WaitForSeconds(duration);
        
        messageText.text = "";
        messageText.gameObject.SetActive(false);
        _currentCoroutine = null;
        
        yield return null;
    }
    
}
