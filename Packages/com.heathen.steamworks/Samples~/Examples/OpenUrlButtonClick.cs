#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.DEMO
{
    [RequireComponent(typeof(Button))]
    public class OpenUrlButtonClick : MonoBehaviour
    {
        [SerializeField]
        private string url;

        private void Start()
        {   
            if (TryGetComponent<Button>(out var btn))
                btn.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (TryGetComponent<Button>(out var btn))
                btn.onClick.RemoveListener(HandleButtonClick);
        }

        private void HandleButtonClick()
        {
            if (!string.IsNullOrEmpty(url))
                Application.OpenURL(url);
        }
    }
}
#endif