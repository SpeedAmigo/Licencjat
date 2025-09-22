using TMPro;
using UnityEngine;

public class ConnectedHostID : MonoBehaviour
{
    private void Start()
    {
        if (TryGetComponent(out TextMeshProUGUI text))
        {
            text.text = ConnectionManager.GetHostHex();
        }
    }
}
