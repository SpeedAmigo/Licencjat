using System;
using FishNet.Object;
using UnityEngine;

public class UIVisorCracksScript : NetworkBehaviour
{
    [SerializeField] private DamageTemplate[] damageTemplates;

    private void OnEnable()
    {
        OxygenScript.OnDrainRateEvent += UpdateCracks;
    }

    private void OnDisable()
    {
        OxygenScript.OnDrainRateEvent -= UpdateCracks;
    }
    
    private void UpdateCracks(float drainRate)
    {
        if (!IsOwner) return;
        
        foreach (var template in damageTemplates)
        {
            bool active = drainRate >= template.drainRate;
            foreach (var crack in template.cracksToActivate)
            {
                crack.SetActive(active);
            }
        }
    }
}

[Serializable]
public class DamageTemplate
{
    public float drainRate;
    public GameObject[] cracksToActivate;
}
