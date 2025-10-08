using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerInventory : NetworkBehaviour
{
    [SerializeField] private List<Image> itemImages = new();
    
    private void AddUiIcon(int index, Sprite sprite)
    {
        itemImages[index].gameObject.SetActive(true);
        itemImages[index].sprite = sprite;
    }
    
    private void OnEnable()
    {
        PlayerInventoryScript.OnUIUpdate += AddUiIcon;
    }

    private void OnDisable()
    {
        PlayerInventoryScript.OnUIUpdate -= AddUiIcon; 
    }
}
