using FishNet.Object;
using UnityEngine;

public class VoiceChatController : NetworkBehaviour
{
    [SerializeField] private MetaVoiceChat.Input.Mic.VcMicAudioInput micAudioInput;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            micAudioInput.enabled = false;
        }
    }
}
