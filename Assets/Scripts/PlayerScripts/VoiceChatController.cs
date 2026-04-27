using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MetaVoiceChat;
using UnityEngine;

public class VoiceChatController : PlayerComponent
{
    [SerializeField] private MetaVoiceChat.Input.Mic.VcMicAudioInput micAudioInput;

    public MetaVc metaVc;

    [AllowMutableSyncType] public SyncVar<float> voiceVolume;

    private float _timer;
    private float _sendRate = 0.1f;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            micAudioInput.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        _timer += Time.deltaTime;
        if (_timer >= _sendRate)
        {
            _timer = 0;
            VoiceHandler(metaVc.Volume);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void VoiceHandler(float volume)
    {
        voiceVolume.Value = volume;
    }
    
    protected override void DeathHandle()
    {
        if (IsOwner)
        {
            metaVc.isInputMuted.Value = true;
            metaVc.isOutputMuted.Value = true;
            //metaVc.isDeafened.Value = true; /// testing if this correctly mute the player
        }
    }

    protected override void ReviveHandle()
    {
        if (IsOwner)
        {
            metaVc.isInputMuted.Value = false;
            metaVc.isOutputMuted.Value = false;
            //metaVc.isDeafened.Value = false;
        }
    }
}
