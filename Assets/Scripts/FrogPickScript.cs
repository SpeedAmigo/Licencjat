using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class FrogPickScript : ObjectPickable
{
    //[AllowMutableSyncType] public SyncVar<bool> pickedUp;
    [AllowMutableSyncType] private SyncVar<float> spitTime = new(5f);
    
    [SerializeField] private FrogScript frogScript;
    
    private PlayerInventoryScript _playerInventory;
    
    protected override void PickupLogic(NetworkObject holder)
    {
        base.PickupLogic(holder);

        frogScript.AI.enabled = false;
        frogScript.Running = false;
        ChangePickupValue(true);
        
        //frogScript.PlayerInventory = holder.transform.root.gameObject.GetComponent<PlayerInventoryScript>();
        _playerInventory = holder.transform.root.gameObject.GetComponent<PlayerInventoryScript>();
    }
    
    protected override void DropLogic()
    {
        base.DropLogic();
        
        frogScript.AI.enabled = true;
        frogScript.Running = true;
        ChangePickupValue(false);
        
        //frogScript.PlayerInventory = null;
        _playerInventory = null;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ChangePickupValue(bool value)
    {
        frogScript.pickedUp.Value = value;
        //pickedUp.Value = value;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        
        if (frogScript.canSpit && frogScript.pickedUp.Value)
        {
            spitTime.Value -= Time.deltaTime;
            if (spitTime.Value <= 0f)
            {
                Debug.Log("Frog Spitted on you");
                spitTime.Value = 5f;
                //animator.Animator.Play("Spit");
                frogScript.PlaySpitAnimation();
                _playerInventory.RequestRemoveItem(this, _playerInventory);
            }
        }
    }
}
