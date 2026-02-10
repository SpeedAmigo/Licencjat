using UnityEngine;

public class SpectatorUIScript : PlayerComponent
{
    [SerializeField] private GameObject spectatorUI;
    
    protected override void DeathHandle()
    {
        if (spectatorUI)
        {
            spectatorUI.SetActive(true);
        }
    }

    protected override void ReviveHandle()
    {
        if (spectatorUI)
        {
            spectatorUI.SetActive(false);
        }
    }
}
