using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameFramework.Items;
using GameFramework.Core.Interfaces;
using static GameFramework.Items.EquippableItemDefinition;

namespace GameFramework.UI
{
    public class HotbarSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
                               IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        private HotbarController hotbarController;
        private EquippableItemDefinition currentItem;
        private int slotIndex;
        private bool isSelected;
        private bool isDragging;
        
        public void Initialize(InventoryUI ui, HotbarController controller, int index)
        {
            inventoryUI = ui;
            hotbarController = controller;
            slotIndex = index;
            
            if (itemIcon == null) itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemIcon == null) itemIcon = GetComponentInChildren<Image>();
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
        
        public void SetItem(EquippableItemDefinition item)
        {
            currentItem = item;
            
            if (item != null)
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = item.icon;
                    itemIcon.color = Color.white;
                    itemIcon.gameObject.SetActive(true);
                }
                else
                {
                }
            }
            else
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                    itemIcon.gameObject.SetActive(false);
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
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                hotbarController.SelectHotbarSlot(slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
            {
                // Right-click to remove from hotbar
                RemoveFromHotbar();
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
            var draggedHotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlot>();
            var draggedEquipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();
            
            if (draggedSlot != null)
            {
                HandleInventorySlotDrop(draggedSlot);
            }
            else if (draggedHotbarSlot != null && draggedHotbarSlot != this)
            {
                HandleHotbarSlotDrop(draggedHotbarSlot);
            }
            else if (draggedEquipmentSlot != null)
            {
                HandleEquipmentSlotDrop(draggedEquipmentSlot);
            }
            
            // Reset visual feedback
            if (background != null && !isSelected)
            {
                background.color = normalColor;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentItem == null) return;
            
            isDragging = true;
            inventoryUI.HideTooltip();
            
            // Use drag visual manager
            DragVisualManager.Instance.StartDrag(currentItem, eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                DragVisualManager.Instance.UpdateDragPosition(eventData.position);
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            DragVisualManager.Instance.StopDrag();
        }
        
        private void HandleInventorySlotDrop(InventorySlot inventorySlot)
        {
            if (inventorySlot.CurrentItem is EquippableItemDefinition equippable)
            {
                // Check if item can be equipped to main hand
                if (equippable.equipmentSlot == EquipmentSlot.MainHand || 
                    equippable.equipmentSlot == EquipmentSlot.TwoHanded)
                {
                    // Reference the item in inventory, don't move it
                    bool success = hotbarController.SetHotbarItem(slotIndex, equippable);
                    if (success)
                    {
                        Debug.Log($"[HotbarSlot {slotIndex}] Successfully added {equippable.itemName} to hotbar");
                    }
                    else
                    {
                        Debug.LogWarning($"[HotbarSlot {slotIndex}] Failed to add {equippable.itemName} to hotbar");
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot add {equippable.itemName} to hotbar - not a main hand item");
                }
            }
        }
        
        private void HandleHotbarSlotDrop(HotbarSlot otherSlot)
        {
            // Swap hotbar items
            hotbarController.SwapHotbarItems(otherSlot.slotIndex, slotIndex);
        }
        
        private void HandleEquipmentSlotDrop(EquipmentSlotUI equipmentSlot)
        {
            // Only allow main hand items
            if (equipmentSlot.SlotType == EquipmentSlot.MainHand && equipmentSlot.CurrentEquippedItem != null)
            {
                var equippedItem = equipmentSlot.CurrentEquippedItem;
                
                // Unequip the item first so it goes back to inventory
                var inventoryController = inventoryUI.GetComponent<GameFramework.Core.Interfaces.IInventoryController>();
                if (inventoryController == null)
                    inventoryController = FindObjectOfType<InventoryController>();
                    
                if (inventoryController != null && inventoryController.CanAddItem(equippedItem, 1))
                {
                    if (inventoryController.AddItem(equippedItem, 1))
                    {
                        equipmentSlot.UnequipItem();
                        hotbarController.SetHotbarItem(slotIndex, equippedItem);
                        inventoryUI.UpdateUI();
                    }
                }
            }
        }
        
        private void RemoveFromHotbar()
        {
            if (currentItem == null) return;
            
            // Just remove the reference, item stays in inventory
            hotbarController.RemoveHotbarItem(slotIndex);
        }
        
        public EquippableItemDefinition CurrentItem => currentItem;
        public int SlotIndex => slotIndex;
    }
}