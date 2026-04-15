using System;
using System.Collections.Generic;
using MetaVoiceChat;
using UnityEngine;

public class LizardScript : BaseEnemyScript
{
    [Header("Dependencies")]
    [SerializeField] private StateMachine lizardStateMachine;
    
    public float noiseThreshold = 0.01f;
    public List<MetaVc> VcInRange;

    public float runDistance;
    

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        lizardStateMachine.ChangeState(new LizardRoamState(lizardStateMachine, this));
    }

    /*private void Update()
    {
        if (!IsServerInitialized) return;

        if (VcInRange.Count != 0 && VcInRange[0].Volume > noiseThreshold)
        {
            Debug.Log("Running!");
        }
    }*/

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
