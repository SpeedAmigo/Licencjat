using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerInventory : NetworkBehaviour
{
    [SerializeField] private List<Image> itemImages = new();
    [SerializeField] private List<GameObject> slotFrames;
    
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
    
    private void UpdateSlotFrame(int currentIndex)
    {
        if (currentIndex >= slotFrames.Count)
        {
            Debug.Log("Not Enough Slots");
            return;
        }

        for (int i = 0; i < slotFrames.Count; i++)
        {
            if (i == currentIndex)
            {
                slotFrames[i].SetActive(true);
            }
            else
            {
                slotFrames[i].SetActive(false);
            }
        }
    }
    
    private void OnEnable()
    {
        PlayerInventoryScript.OnUIUpdateAdd += AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove += RemoveUiIcon;
        PlayerInventoryScript.OnUIFrameUpdate += UpdateSlotFrame;
    }
    
    private void OnDisable()
    {
        PlayerInventoryScript.OnUIUpdateAdd -= AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove -= RemoveUiIcon;
        PlayerInventoryScript.OnUIFrameUpdate -= UpdateSlotFrame;
    }
}
