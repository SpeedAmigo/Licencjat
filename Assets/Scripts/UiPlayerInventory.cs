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

    private void RemoveUiIcon(int index)
    {
        itemImages[index].gameObject.SetActive(false);
        itemImages[index].sprite = null;
    }
    
    private void OnEnable()
    {
        PlayerInventoryScript.OnUIUpdateAdd += AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove += RemoveUiIcon;
    }

    private void OnDisable()
    {
        PlayerInventoryScript.OnUIUpdateAdd -= AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove -= RemoveUiIcon;
    }
}
