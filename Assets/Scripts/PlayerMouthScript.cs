using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MetaVoiceChat;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMouthScript : PlayerComponent
{
    [SerializeField] private Material idleMouthMaterial;
    [SerializeField] private Material speakingMouthMaterial;
    
    [SerializeField] private DecalProjector _decalProjector;
    [SerializeField] private MetaVc _playerMetaVc;

    //private bool _wasSpeaking;
    private readonly SyncVar<bool> _isSpeaking = new();

    protected override void Awake()
    {
        base.Awake();
        _isSpeaking.OnChange += OnSpeakingChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _isSpeaking.OnChange -= OnSpeakingChanged;
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        _decalProjector = GetComponent<DecalProjector>();

        if (IsOwner)
        {
            _playerMetaVc = playerRoot.GetComponentInChildren<MetaVc>();
        }
    }

    private void Update()
    {
        if (!IsOwner || _playerMetaVc == null) return;
        
        bool isSpeaking = _playerMetaVc.isSpeaking;

        if (isSpeaking != _isSpeaking.Value)
        {
            UpdateMouthMaterial(isSpeaking);
            SetSpeakingServer(isSpeaking);
        }
    }

    [ServerRpc]
    private void SetSpeakingServer(bool isSpeaking)
    {
        _isSpeaking.Value = isSpeaking;
    }

    private void OnSpeakingChanged(bool prev, bool next, bool asServer)
    {
        if (asServer) return;
        if (IsOwner) return;
        
        UpdateMouthMaterial(next);
    }

    private void UpdateMouthMaterial(bool isSpeaking)
    {
        if (_decalProjector == null) return;
        _decalProjector.material = isSpeaking ? speakingMouthMaterial : idleMouthMaterial;
    }
}
