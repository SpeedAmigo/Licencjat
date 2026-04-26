using System;
using System.Collections.Generic;
using FishNet.Object;
using MetaVoiceChat;
using UnityEngine;

public class LizardScript : BaseEnemyScript
{
    [Header("State")]
    public LizardState lizardState;
    
    [Header("Dependencies")]
    [SerializeField] private StateMachine lizardStateMachine;
    
    public float noiseThreshold = 0.01f;
    public List<VoiceChatController> VcInRange;

    public float runDistance;

    public float attackDistance;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        lizardStateMachine.ChangeState(new LizardRoamState(lizardStateMachine, this));
    }
    
    [Server]
    public VoiceChatController GetLoudestVoiceAround()
    {
        VoiceChatController loudestVoice = null;
        float maxVolume = 0f;
        
        foreach (var voice in VcInRange)
        {
            Debug.Log(voice.voiceVolume.Value);
            
            if (voice.voiceVolume.Value >= maxVolume)
            {
                maxVolume = voice.voiceVolume.Value;
                loudestVoice = voice;
            }
        }
        
        return loudestVoice;
    }
    #region Detection Region
    
    protected override void OnDetected(Collider other)
    {
        base.OnDetected(other);
        
        if (other.CompareTag("Player"))
        {
            AddVcToList(other.gameObject);
        }
    }

    protected override void OnLost(Collider other)
    {
        base.OnLost(other);
        
        if (other.CompareTag("Player"))
        {
            RemoveVcFromList(other.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddVcToList(GameObject go)
    {
        if (!IsServerInitialized) return;
        
        VoiceChatController vc = go.GetComponentInChildren<VoiceChatController>();

        if (!VcInRange.Contains(vc))
        {
            VcInRange.Add(vc);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveVcFromList(GameObject go)
    {
        if (!IsServerInitialized) return;
        
        VoiceChatController vc = go.GetComponentInChildren<VoiceChatController>();

        if (VcInRange.Contains(vc))
        {
            VcInRange.Remove(vc);
        }
    }
    
    #endregion
}

public enum LizardState
{
    Roam,
    RunningAway,
    MoveToAttack,
    Attack
}
