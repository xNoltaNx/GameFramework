using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameFramework.Items;

namespace GameFramework.UI
{
    public class HotbarSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private Image selectionBorder;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;
        
        private InventoryUI inventoryUI;
        private ItemDefinition currentItem;
        private int slotIndex;
        private bool isSelected;
        
        public void Initialize(InventoryUI ui, int index)
        {
            inventoryUI = ui;
            slotIndex = index;
            
            if (itemIcon == null) itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (background == null) background = GetComponent<Image>();
            if (keyText == null) keyText = GetComponentInChildren<TextMeshProUGUI>();
            if (selectionBorder == null) selectionBorder = transform.Find("SelectionBorder")?.GetComponent<Image>();
            
            if (keyText != null)
            {
                keyText.text = (index + 1).ToString();
            }
            
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(false);
            }
            
            SetItem(null);
        }
        
        public void SetItem(ItemDefinition item)
        {
            currentItem = item;
            
            if (item != null)
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = item.icon;
                    itemIcon.color = Color.white;
                }
            }
            else
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                }
            }
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(selected);
            }
            
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && !isSelected)
            {
                if (background != null)
                {
                    background.color = hoverColor;
                }
                
                Vector3 tooltipPosition = transform.position + Vector3.up * 100f;
                inventoryUI.ShowTooltip(currentItem, tooltipPosition);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected && background != null)
            {
                background.color = normalColor;
            }
            
            inventoryUI.HideTooltip();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            inventoryUI.SelectHotbarSlot(slotIndex);
        }
    }
}