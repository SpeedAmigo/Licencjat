using FishNet.Object;
using FishNet.Transporting;
using Heathen.SteamworksIntegration;
using UnityEngine;

public class VoiceDataHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private VoiceRecorder voiceRecorder;
    [SerializeField] private VoiceStream voiceStream;

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            voiceRecorder.StartRecording();
        }
    }

    public void SendVoiceData(byte[] data)
    {
        SendVoiceData_Server(data);
    }

    [ServerRpc]
    private void SendVoiceData_Server(byte[] data, Channel channel = Channel.Unreliable)
    {
        ReceiveVoiceData(data);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ReceiveVoiceData(byte[] data)
    {
        voiceStream.PlayVoiceData(data);
    }
}
