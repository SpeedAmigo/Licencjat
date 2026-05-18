using System;
using FishNet.Object;
using FMODUnity;
using Items;
using UnityEngine;

public class DuctTapeScript : Item, IPrimaryClick, IPrimaryCancel, ISecondaryClick, ISecondaryCancel
{
    [Header("Dependencies")]
    [SerializeField] private Transform raycastStartPoint;
    
    [Header("Settings")]
    [SerializeField] private float shootDistance;
    [SerializeField] private float useTime;
    
    [Header("Effects")]
    [SerializeField] private StatusEffect[] effects;
    
    [Header("Sounds")]
    [SerializeField] private EventReference useSound;

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Mesh usedTape;
    
    private bool _currentlyUsed;
    private bool _primaryClicked;
    private float _timer;
    
    private PlayerRoot _currentPlayer;

    private void Start()
    {
        durability.OnChange += OnDurabilityChanged;
    }

    private void OnDestroy()
    {
        durability.OnChange -= OnDurabilityChanged;
    }

    private void OnDurabilityChanged(uint prev, uint next, bool asServer)
    {
        if (next == 0)
        {
            meshFilter.sharedMesh = usedTape;
        }
    }

    protected void Update()
    {
        if (!_currentlyUsed) return;
        
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            if (_primaryClicked)
            { 
                var nob = RaycastShoot();
                TryApplyEffect(nob);
            }
            else
            {
                TryApplyEffect(GetPlayerNetworkObject());
            }
            
            DecreaseDurability();
            _currentlyUsed = false;
        }
    }
    
    public void OnPrimaryClick()
    {
        if (!CheckDurability())
        {
            Debug.Log("No more durability!");
            return;
        }
        
        SoundCreator.Instance.PlayOneShotAttached(useSound, gameObject);
        
        ClickHandler(true);
        
        var nob = RaycastShoot();
        if (!nob) return;
        
        TryGetOtherPlayer(nob);
        HandleDurationFill(true);
    }
    
    public void OnPrimaryCancel()
    {
        CancelHandler();
        
        HandleDurationFill(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void HandleDurationFill(bool startFill)
    {
        if (_currentPlayer == null)
        {
            Debug.Log("Handle duration fill doesn't have player");
            return;
        }
        
        if (startFill)
        {
            _currentPlayer.StartDurationFill(_currentPlayer.Owner, useTime);
        }
        else
        {
            _currentPlayer.StopDurationFill(_currentPlayer.Owner);
            _currentPlayer = null;
        }
    }
    
    public void OnSecondaryClick()
    {
        if (!CheckDurability())
        {
            Debug.Log("No more durability!");
            return;
        }
        
        SoundCreator.Instance.PlayOneShotAttached(useSound, gameObject);
        
        ClickHandler(false);
    }

    public void OnSecondaryCancel()
    {
        CancelHandler();
    }

    private void ClickHandler(bool primaryClick)
    {
        if (_currentlyUsed) return;
        
        _primaryClicked = primaryClick;
        
        _timer = useTime;
        _currentlyUsed = true;
        PlayerUsageManager.Instance.StartFillUsage(useTime);
    }

    private void CancelHandler()
    {
        _currentlyUsed = false;
        PlayerUsageManager.Instance.StopFillUsage();
    }
    
    private NetworkObject RaycastShoot()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
        {
            Debug.Log(hit.collider.transform.parent.gameObject.name);
            
            if (hit.collider.transform.parent.TryGetComponent<NetworkObject>(out var nob))
            {
               return nob;
            }
        }
        
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryGetOtherPlayer(NetworkObject target)
    {
        var player = target.GetComponent<PlayerRoot>();

        if (!player)
        {
            Debug.Log($"{target.name} does not have playerRoot component");
            return;
        }
        
        _currentPlayer = player;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryApplyEffect(NetworkObject target)
    {
        var handler = target.GetComponent<StatusEffectHandler>();

        if (!handler)
        {
            Debug.Log(target.name + " has no StatusEffectHandler");
            return;
        }
        
        handler.ApplyEffects(effects);
    }

    private NetworkObject GetPlayerNetworkObject()
    {
        NetworkObject nob = transform.parent.parent.parent.GetComponent<NetworkObject>();
        Debug.Log(nob.name);
        return nob;
    }
}
