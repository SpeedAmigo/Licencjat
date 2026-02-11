using FishNet.Object;
using MetaVoiceChat;
using UnityEngine;

public class VoiceChatController : PlayerComponent
{
    [SerializeField] private MetaVoiceChat.Input.Mic.VcMicAudioInput micAudioInput;

    [SerializeField] private MetaVc metaVc;

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
            metaVc.isInputMuted.Value = true;
            metaVc.isOutputMuted.Value = true;
            metaVc.isDeafened.Value = true;
        }
    }

    protected override void ReviveHandle()
    {
        if (IsOwner)
        {
            metaVc.isInputMuted.Value = false;
            metaVc.isOutputMuted.Value = false;
            metaVc.isDeafened.Value = false;
        }
    }
}
