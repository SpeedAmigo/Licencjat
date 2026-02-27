using FishNet.Object;
using Items;
using UnityEngine;

public class DuctTapeScript : Item, IPrimaryClick, IPrimaryCancel, ISecondaryClick, ISecondaryCancel
{
    [SerializeField] private Transform raycastStartPoint;
    [SerializeField] private float shootDistance;

    [SerializeField] private float useTime;
    
    [SerializeField] private StatusEffect[] effects;
    
    private bool _currentlyUsed;
    private bool _primaryClicked;
    [SerializeField] private float _timer;
    
    private void Update()
    {
        if (!_currentlyUsed) return;
        
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            if (_primaryClicked)
            { 
                RaycastShoot(); 
            }
            else
            {
                TryApplyEffect(GetPlayerNetworkObject());
            }
            
            _currentlyUsed = false;
        }
    }
    
    public void OnPrimaryClick()
    {
        ClickHandler(true);
    }

    public void OnPrimaryCancel()
    {
        CancelHandler();
    }
    
    public void OnSecondaryClick()
    {
        ClickHandler(false);
    }

    public void OnSecondaryCancel()
    {
        CancelHandler();
    }

    private void ClickHandler(bool primaryClick)
    {
        if (_currentlyUsed) return;
        
        _primaryClicked  = primaryClick;
        
        _timer = useTime;
        _currentlyUsed = true;
    }

    private void CancelHandler()
    {
        _currentlyUsed = false;
    }
    
    private void RaycastShoot()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
        {
            Debug.Log(hit.collider.transform.parent.gameObject.name);
            
            if (hit.collider.transform.parent.TryGetComponent<NetworkObject>(out var nob))
            {
                TryApplyEffect(nob);
            }
        }
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
