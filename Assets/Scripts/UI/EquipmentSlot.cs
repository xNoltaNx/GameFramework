using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameFramework.Items;
using GameFramework.Core.Interfaces;
using static GameFramework.Items.EquippableItemDefinition;

namespace GameFramework.UI
{
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                                   IDropHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI slotNameText;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color validDropColor = Color.green;
        [SerializeField] private Color invalidDropColor = Color.red;
        
        private InventoryUI inventoryUI;
        private IEquipmentController equipmentController;
        private EquipmentSlot slotType;
        private EquippableItemDefinition currentEquippedItem;
        
        public void Initialize(InventoryUI ui, EquipmentSlot slot)
        {
            inventoryUI = ui;
            slotType = slot;
            equipmentController = GetComponentInParent<IEquipmentController>();
            
            if (itemIcon == null) itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (background == null) background = GetComponent<Image>();
            if (slotNameText == null) slotNameText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (slotNameText != null)
            {
                slotNameText.text = slot.ToString();
            }
            
            SetEquippedItem(null);
        }
        
        public void SetEquippedItem(EquippableItemDefinition item)
        {
            currentEquippedItem = item;
            
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
        
        public void EquipItem(EquippableItemDefinition item)
        {
            if (item == null) return;
            
            // Check if item can be equipped in this slot
            if (item.equipmentSlot == slotType || 
                (slotType == EquipmentSlot.MainHand && item.equipmentSlot == EquipmentSlot.TwoHanded) ||
                (slotType == EquipmentSlot.OffHand && item.equipmentSlot == EquipmentSlot.TwoHanded))
            {
                // Unequip current item if any
                if (currentEquippedItem != null)
                {
                    equipmentController.UnequipItem(slotType.ToString());
                }
                
                // Equip new item
                equipmentController.EquipItem(item, slotType.ToString());
                SetEquippedItem(item);
            }
        }
        
        public void UnequipItem()
        {
            if (currentEquippedItem != null)
            {
                equipmentController.UnequipItem(slotType.ToString());
                SetEquippedItem(null);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentEquippedItem != null)
            {
                background.color = highlightColor;
                Vector3 tooltipPosition = transform.position + Vector3.up * 100f;
                inventoryUI.ShowTooltip(currentEquippedItem, tooltipPosition);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            background.color = normalColor;
            inventoryUI.HideTooltip();
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
            if (draggedSlot != null)
            {
                var item = draggedSlot.CurrentItem;
                if (item is EquippableItemDefinition equippable)
                {
                    EquipItem(equippable);
                }
            }
            
            background.color = normalColor;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && currentEquippedItem != null)
            {
                // Right-click to unequip
                UnequipItem();
                inventoryUI.UpdateUI();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var draggedItem = other.GetComponent<InventorySlot>();
            if (draggedItem != null && draggedItem.CurrentItem is EquippableItemDefinition equippable)
            {
                bool canEquip = equippable.equipmentSlot == slotType ||
                               (slotType == EquipmentSlot.MainHand && equippable.equipmentSlot == EquipmentSlot.TwoHanded) ||
                               (slotType == EquipmentSlot.OffHand && equippable.equipmentSlot == EquipmentSlot.TwoHanded);
                
                background.color = canEquip ? validDropColor : invalidDropColor;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            background.color = normalColor;
        }
    }
}