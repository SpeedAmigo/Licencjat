using System;
using FishNet.Object;
using UnityEngine;

public class UIVisorCracksScript : PlayerComponent
{
    [SerializeField] private DamageTemplate[] damageTemplates;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        OxygenScript.OnDrainRateEvent += UpdateCracks;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OxygenScript.OnDrainRateEvent -= UpdateCracks;
    }
    
    private void UpdateCracks(float drainRate)
    {
        if (!IsOwner) return;
        if (!playerRoot.isAlive.Value) return;
        
        foreach (var template in damageTemplates)
        {
            bool active = drainRate >= template.drainRate;
            foreach (var crack in template.cracksToActivate)
            {
                crack.SetActive(active);
            }
        }
    }

    protected override void DeathHandle()
    {
        foreach (var template in damageTemplates)
        {
            foreach (var crack in template.cracksToActivate)
            {
                crack.SetActive(false);
            }
        }
    }

    protected override void ReviveHandle()
    {
        
    }
}

[Serializable]
public class DamageTemplate
{
    public float drainRate;
    public GameObject[] cracksToActivate;
}
