using System;
using System.Collections.Generic;
using MetaVoiceChat;
using UnityEngine;

public class LizardScript : BaseEnemyScript
{
    [Header("State")]
    public LizardState lizardState;
    
    [Header("Dependencies")]
    [SerializeField] private StateMachine lizardStateMachine;
    
    public float noiseThreshold = 0.01f;
    public List<MetaVc> VcInRange;

    public float runDistance;

    public float attackDistance;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        lizardStateMachine.ChangeState(new LizardRoamState(lizardStateMachine, this));
    }

    public MetaVc GetLoudestVoiceAround()
    {
        MetaVc loudestVoice = null;
        float maxVolume = 0;
        
        foreach (var voice in VcInRange)
        {
            if (voice.Volume > maxVolume)
            {
                maxVolume = voice.Volume;
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
            MetaVc vc = other.GetComponentInChildren<MetaVc>();

            if (!VcInRange.Contains(vc))
            {
                VcInRange.Add(vc);
            }
        }
    }

    protected override void OnLost(Collider other)
    {
        base.OnLost(other);
        
        if (other.CompareTag("Player"))
        {
            MetaVc vc = other.GetComponentInChildren<MetaVc>();

            if (VcInRange.Contains(vc))
            {
                VcInRange.Remove(vc);
            }
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
