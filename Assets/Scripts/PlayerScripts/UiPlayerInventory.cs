using System.Collections.Generic;
using FishNet.Object;
using Items;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerInventory : NetworkBehaviour
{
    [SerializeField] private List<Image> itemImages = new();
    [SerializeField] private List<GameObject> slotFrames;
    [SerializeField] private List<Slider> slotSliders;

    private Item _currentItem;
    private int _currentIndex;
    
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

    private void UpdateSlotSlider(int currentIndex, Item item)
    {
        if (currentIndex >= slotSliders.Count)
        {
            Debug.Log("Not Enough Sliders");
            return;
        }
        
        if (_currentItem != null)
        {
            _currentItem.durability.OnChange -= OnSliderValueChanged;
        }
        
        _currentIndex = currentIndex;
        _currentItem = item;

        if (item == null)
        {
            slotSliders[currentIndex].gameObject.SetActive(false);
            return;
        }
        
        for (int i = 0; i < slotSliders.Count; i++)
        {
            bool isActive = i == currentIndex;
            
            slotSliders[i].gameObject.SetActive(isActive);
            
            if (isActive)
            {
                slotSliders[i].maxValue = item.maxDurability;
                slotSliders[i].value = item.durability.Value;
                
                _currentItem.durability.OnChange += OnSliderValueChanged;
            }
        }
    }

    private void OnSliderValueChanged(uint prev, uint next, bool asServer)
    {
        if (_currentIndex < 0 || _currentIndex >= slotSliders.Count)
            return;

        slotSliders[_currentIndex].value = next;
    }

    private void OnEnable()
    {
        PlayerInventoryScript.OnUIUpdateAdd += AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove += RemoveUiIcon;
        PlayerInventoryScript.OnUIFrameUpdate += UpdateSlotFrame;
        PlayerInventoryScript.OnUISliderUpdate += UpdateSlotSlider;
    }
    
    private void OnDisable()
    {
        PlayerInventoryScript.OnUIUpdateAdd -= AddUiIcon;
        PlayerInventoryScript.OnUIUpdateRemove -= RemoveUiIcon;
        PlayerInventoryScript.OnUIFrameUpdate -= UpdateSlotFrame;
        PlayerInventoryScript.OnUISliderUpdate -= UpdateSlotSlider;
    }
}
