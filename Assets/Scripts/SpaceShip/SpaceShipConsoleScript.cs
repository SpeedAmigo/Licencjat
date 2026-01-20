using System;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class SpaceShipConsoleScript : NetworkBehaviour
{
    public static event Action<bool> ActivateInvitationButton;
    
    [AllowMutableSyncType] public SyncVar<bool> shipLanded;
    [AllowMutableSyncType] public SyncVar<bool> shipPending;
        
    #region ShipPendingRegion
    public void SetShipPending(bool value)
    {
        if (shipPending.Value == value)
        {
            Debug.LogWarning("Ship pending is already set");
            return;
        }
        
        Debug.Log($"shipPending set to: {value}");
        SetShipPendingServer(value);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetShipPendingServer(bool value)
    {
        shipPending.Value = value;
        CheckShipStatus();
    }
    #endregion

    #region SetShipLandedRegion
    public void SetShipLanded(bool value)
    {
        if (shipLanded.Value == value)
        {
            Debug.LogWarning("Ship State is the same");
            return;
        }
        
        SetShipLandedServer(value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetShipLandedServer(bool value)
    {
        shipLanded.Value = value;
        CheckShipStatus();
    }
    #endregion
    
    [Server]
    private void CheckShipStatus()
    {
        if (shipPending.Value || shipLanded.Value)
        {
            SetButtonObservers(false);
        }
        else 
        {
            SetButtonObservers(true);
        }
    }

    [ObserversRpc]
    private void SetButtonObservers(bool value)
    {
        ActivateInvitationButton?.Invoke(value);
    }
}
