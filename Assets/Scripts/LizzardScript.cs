using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class LizardScript : BaseEnemyScript, IStunable
{
    [Header("State")]
    public LizardState lizardState;
    
    [Header("Dependencies")]
    [SerializeField] private StateMachine lizardStateMachine;
    public CreatureStatusVisualizer lizardVisualizer;

    public LizardSetting lizardSetting;
    
    public float noiseThreshold = 0.01f;
    public List<VoiceChatController> VcInRange;

    public float runDistance;

    public float attackDistance;

    [HideInInspector] public int attackLayer;
    
    private Coroutine _weightCoroutine;
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        attackLayer = animator.Animator.GetLayerIndex("Attack");
        
        lizardStateMachine.ChangeState(new LizardRoamState(lizardStateMachine, this));
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        
        /*float normalizedSpeed = ai.velocity.magnitude / ai.maxSpeed;
        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
        
        animator.Animator.SetFloat("Speed", normalizedSpeed);*/
    }

    public void ChangeLayerWeight(int layerIndex, float targetWeight, float duration)
    {
        if (_weightCoroutine != null)
        {
            StopCoroutine(_weightCoroutine);
        }
        
        StartCoroutine(BlendLayer(layerIndex, targetWeight, duration));
    }

    private IEnumerator BlendLayer(int layerIndex, float targetWeight, float duration)
    {
        float startWeight = animator.Animator.GetLayerWeight(layerIndex);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            
            float weight = Mathf.Lerp(startWeight, targetWeight, t);
            animator.Animator.SetLayerWeight(layerIndex, weight);
            
            yield return null;
        }
        
        animator.Animator.SetLayerWeight(layerIndex, targetWeight);
    }
    
    
    [Server]
    public VoiceChatController GetLoudestVoiceAround()
    {
        VoiceChatController loudestVoice = null;
        float maxVolume = 0f;
        
        foreach (var voice in VcInRange)
        {
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

    public void SetStunned(bool stunned, float duration)
    {
        if (stunned)
        {
            lizardStateMachine.ChangeState(new LizardStunState(lizardStateMachine, this, duration));
        }
    }
}

public enum LizardState
{
    Roam,
    RunningAway,
    MoveToAttack,
    Attack
}

public enum LizardSetting
{
    Attacker,
    Runner
}
