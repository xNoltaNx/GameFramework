using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using GameFramework.Items;
using GameFramework.Core.Interfaces;
using static GameFramework.Items.EquippableItemDefinition;

namespace GameFramework.UI
{
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                                   IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        private IInventoryController inventoryController;
        private EquipmentSlot slotType;
        private EquippableItemDefinition currentEquippedItem;
        private bool isDragging;
        
        public EquippableItemDefinition CurrentEquippedItem => currentEquippedItem;
        public EquipmentSlot SlotType => slotType;
        
        public void Initialize(InventoryUI ui, EquipmentSlot slot, IEquipmentController equipController, IInventoryController invController)
        {
            inventoryUI = ui;
            slotType = slot;
            equipmentController = equipController;
            inventoryController = invController;
            
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
                // If there's already an item equipped, return it to inventory
                if (currentEquippedItem != null)
                {
                    var inventoryControllerImpl = inventoryController as InventoryController;
                    if (inventoryControllerImpl != null)
                    {
                        int emptySlot = inventoryControllerImpl.GetFirstEmptySlot();
                        if (emptySlot >= 0)
                        {
                            inventoryControllerImpl.SetItemAtSlot(emptySlot, currentEquippedItem, 1);
                            equipmentController.UnequipItem(slotType.ToString());
                        }
                        else
                        {
                            Debug.LogWarning($"Cannot unequip {currentEquippedItem.itemName} - inventory full!");
                            return; // Don't equip new item if we can't unequip current one
                        }
                    }
                    else if (inventoryController != null && inventoryController.CanAddItem(currentEquippedItem, 1))
                    {
                        inventoryController.AddItem(currentEquippedItem, 1);
                        equipmentController.UnequipItem(slotType.ToString());
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot unequip {currentEquippedItem.itemName} - inventory full!");
                        return; // Don't equip new item if we can't unequip current one
                    }
                }
                
                // Equip new item
                equipmentController.EquipItem(item, slotType.ToString());
                SetEquippedItem(item);
            }
        }
        
        public void UnequipItem()
        {
            if (currentEquippedItem != null && equipmentController != null)
            {
                equipmentController.UnequipItem(slotType.ToString());
                SetEquippedItem(null);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentEquippedItem != null && !isDragging)
            {
                background.color = highlightColor;
                inventoryUI.ShowTooltip(currentEquippedItem, Mouse.current.position.ReadValue());
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDragging)
            {
                background.color = normalColor;
                inventoryUI.HideTooltip();
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
            if (draggedSlot != null)
            {
                var item = draggedSlot.CurrentItem;
                if (item is EquippableItemDefinition equippable)
                {
                    // Check if item can be equipped in this slot
                    if (equippable.equipmentSlot == slotType || 
                        (slotType == EquipmentSlot.MainHand && equippable.equipmentSlot == EquipmentSlot.TwoHanded) ||
                        (slotType == EquipmentSlot.OffHand && equippable.equipmentSlot == EquipmentSlot.TwoHanded))
                    {
                        // Remove the item from the inventory slot
                        draggedSlot.RemoveItemForEquipping();
                        
                        // Equip the item
                        EquipItem(equippable);
                        inventoryUI.UpdateUI();
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot equip {equippable.itemName} to {slotType} slot");
                    }
                }
                else if (item != null)
                {
                    Debug.LogWarning($"Cannot equip non-equippable item: {item.itemName}");
                }
            }
            
            background.color = normalColor;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && currentEquippedItem != null)
            {
                // Right-click to unequip - add to first available inventory slot
                var inventoryControllerImpl = inventoryController as InventoryController;
                if (inventoryControllerImpl != null)
                {
                    int emptySlot = inventoryControllerImpl.GetFirstEmptySlot();
                    if (emptySlot >= 0)
                    {
                        inventoryControllerImpl.SetItemAtSlot(emptySlot, currentEquippedItem, 1);
                        UnequipItem();
                        inventoryUI.UpdateUI();
                    }
                    else
                    {
                        Debug.LogWarning("Cannot unequip - inventory full!");
                    }
                }
                else if (inventoryController != null && inventoryController.CanAddItem(currentEquippedItem, 1))
                {
                    if (inventoryController.AddItem(currentEquippedItem, 1))
                    {
                        UnequipItem();
                        inventoryUI.UpdateUI();
                    }
                }
                else
                {
                    Debug.LogWarning("Cannot unequip - inventory full!");
                }
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
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentEquippedItem == null) return;
            
            isDragging = true;
            background.color = normalColor;
            inventoryUI.HideTooltip();
            
            // Use drag visual manager
            DragVisualManager.Instance.StartDrag(currentEquippedItem, eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            DragVisualManager.Instance.UpdateDragPosition(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            
            DragVisualManager.Instance.StopDrag();
            
            // Let the target handle the drop - we only handle equipment-to-equipment swaps here
            if (eventData.pointerEnter != null)
            {
                var equipmentSlot = eventData.pointerEnter.GetComponent<EquipmentSlotUI>();
                if (equipmentSlot != null && equipmentSlot != this)
                {
                    HandleEquipmentSwap(equipmentSlot);
                }
            }
        }
        
        
        private void HandleEquipmentSwap(EquipmentSlotUI otherSlot)
        {
            // Swap equipped items between equipment slots
            var tempItem = currentEquippedItem;
            var otherItem = otherSlot.CurrentEquippedItem;
            
            if (otherItem != null)
            {
                EquipItem(otherItem);
            }
            else
            {
                UnequipItem();
            }
            
            if (tempItem != null)
            {
                otherSlot.EquipItem(tempItem);
            }
            
            inventoryUI.UpdateUI();
        }
    }
}