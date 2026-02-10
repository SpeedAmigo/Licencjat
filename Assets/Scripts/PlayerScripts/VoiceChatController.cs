using FishNet.Object;
using UnityEngine;

public class VoiceChatController : PlayerComponent
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
    
    protected override void DeathHandle()
    {
        if (IsOwner)
        {
            micAudioInput.enabled = false;
        }
    }

    protected override void ReviveHandle()
    {
        if (IsOwner)
        {
            micAudioInput.enabled = true;
        }
    }
}
