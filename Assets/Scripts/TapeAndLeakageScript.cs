using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class TapeAndLeakageScript : PlayerComponent
{
    [SerializeField] private LeakageTemplate[] damageTemplates;

    [SerializeField] private GameObject[] leakages;
    [SerializeField] private GameObject[] tapes;

    private readonly HashSet<int> repairedLeakages = new();

    private LeakageTemplate currentTemplate;

    protected override void OnEnable()
    {
        base.OnEnable();

        OxygenScript.OnDrainRateEvent += UpdateLeakages;
        playerRoot.HealEvent += HandleTape;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OxygenScript.OnDrainRateEvent -= UpdateLeakages;
        playerRoot.HealEvent -= HandleTape;
    }

    private void UpdateLeakages(float drainRate)
    {
        if (!IsOwner) return;
        if (!playerRoot.isAlive.Value) return;

        currentTemplate = null;

        foreach (var template in damageTemplates)
        {
            if (drainRate >= template.drainRate)
            {
                currentTemplate = template;
            }
        }

        List<int> activeLeakages = new();

        if (currentTemplate != null)
        {
            foreach (int index in currentTemplate.leakageIndices)
            {
                activeLeakages.Add(index);
                
                /*// Do not activate repaired leakage
                if (!repairedLeakages.Contains(index))
                {
                    
                }*/
            }
        }

        HandleLeakagesServer(activeLeakages.ToArray());
    }

    [ServerRpc]
    private void HandleLeakagesServer(int[] activeIndices)
    {
        HandleLeakagesObservers(activeIndices);
    }

    [ObserversRpc(BufferLast = true)]
    private void HandleLeakagesObservers(int[] activeIndices)
    {
        HashSet<int> activeSet = new(activeIndices);

        for (int i = 0; i < leakages.Length; i++)
        {
            bool active = activeSet.Contains(i);

            leakages[i].SetActive(active);

            // If leakage becomes active again, remove tape
            if (active)
            {
                tapes[i].SetActive(false);
                repairedLeakages.Remove(i);
            }
        }
    }

    private void HandleTape()
    {
        if (!IsOwner) return;
        if (currentTemplate == null) return;

        foreach (int leakageIndex in currentTemplate.leakageIndices)
        {
            if (repairedLeakages.Contains(leakageIndex))
                continue;

            HandleTapeServer(leakageIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleTapeServer(int leakageIndex)
    {
        HandleTapeObservers(leakageIndex);
    }

    [ObserversRpc]
    private void HandleTapeObservers(int leakageIndex)
    {
        if (leakageIndex < 0 || leakageIndex >= leakages.Length)
            return;

        repairedLeakages.Add(leakageIndex);

        leakages[leakageIndex].SetActive(false);
        tapes[leakageIndex].SetActive(true);
    }

    protected override void ReviveHandle()
    {
        base.ReviveHandle();

        ResetLeakagesServer();
    }

    [ServerRpc]
    private void ResetLeakagesServer()
    {
        ResetLeakagesObservers();
    }

    [ObserversRpc(BufferLast = true)]
    private void ResetLeakagesObservers()
    {
        repairedLeakages.Clear();

        for (int i = 0; i < leakages.Length; i++)
        {
            leakages[i].SetActive(false);
            tapes[i].SetActive(false);
        }
    }
}

[Serializable]
public class LeakageTemplate
{
    public float drainRate;
    public int[] leakageIndices;
}