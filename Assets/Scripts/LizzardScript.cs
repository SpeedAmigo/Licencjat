using System;
using System.Collections.Generic;
using MetaVoiceChat;
using UnityEngine;

public class LizzardScript : BaseEnemyScript
{
    [SerializeField] private List<MetaVc> VcInRange;

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (VcInRange.Count != 0 && VcInRange[0].Volume > VcInRange[0].speakingThreshold)
        {
            Debug.Log("Running!");
        }
    }

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
}
