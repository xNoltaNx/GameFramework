using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using GameFramework.Items;
using GameFramework.Core.Interfaces;

namespace GameFramework.UI
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                                 IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI stackCountText;
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        private InventoryUI inventoryUI;
        private ItemDefinition currentItem;
        private int stackCount;
        private int slotIndex;
        private bool isDragging;
        
        public ItemDefinition CurrentItem => currentItem;
        public int SlotIndex => slotIndex;
        
        public void Initialize(InventoryUI ui, int index)
        {
            inventoryUI = ui;
            slotIndex = index;
            
            if (itemIcon == null) itemIcon = GetComponentInChildren<Image>();
            if (stackCountText == null) stackCountText = GetComponentInChildren<TextMeshProUGUI>();
            if (background == null) background = GetComponent<Image>();
            
            SetItem(null, 0);
        }
        
        public void SetItem(ItemDefinition item, int count)
        {
            currentItem = item;
            stackCount = count;
            
            if (item != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.color = Color.white;
                
                if (item.isStackable && count > 1)
                {
                    stackCountText.text = count.ToString();
                    stackCountText.gameObject.SetActive(true);
                }
                else
                {
                    stackCountText.gameObject.SetActive(false);
                }
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.color = Color.clear;
                stackCountText.gameObject.SetActive(false);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && !isDragging)
            {
                background.color = highlightColor;
                inventoryUI.ShowTooltip(currentItem, Mouse.current.position.ReadValue());
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
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentItem == null) return;
            
            isDragging = true;
            background.color = normalColor;
            inventoryUI.HideTooltip();
            
            // Use drag visual manager
            DragVisualManager.Instance.StartDrag(currentItem, eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            DragVisualManager.Instance.UpdateDragPosition(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            
            DragVisualManager.Instance.StopDrag();
            
            // Check if dropped on valid target
            if (eventData.pointerEnter != null)
            {
                var targetSlot = eventData.pointerEnter.GetComponent<InventorySlot>();
                var equipmentSlot = eventData.pointerEnter.GetComponent<EquipmentSlotUI>();
                
                if (targetSlot != null)
                {
                    HandleSlotDrop(targetSlot);
                }
                else if (equipmentSlot != null && currentItem is EquippableItemDefinition)
                {
                    HandleEquipmentDrop(equipmentSlot);
                }
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlot>();
            var draggedEquipmentSlot = eventData.pointerDrag?.GetComponent<EquipmentSlotUI>();
            
            if (draggedSlot != null && draggedSlot != this)
            {
                HandleSlotDrop(draggedSlot);
            }
            else if (draggedEquipmentSlot != null)
            {
                HandleEquipmentSlotDrop(draggedEquipmentSlot);
            }
        }
        
        private void HandleSlotDrop(InventorySlot other)
        {
            var inventoryController = inventoryUI.GetComponent<IInventoryController>() as InventoryController;
            if (inventoryController == null)
                inventoryController = FindObjectOfType<InventoryController>();
                
            if (inventoryController == null)
            {
                Debug.LogWarning("InventoryController not found - cannot handle slot drop");
                return;
            }
            
            // Handle combining stacks or swapping items
            if (other.currentItem == currentItem && currentItem != null && currentItem.isStackable)
            {
                // Try to combine stacks using inventory controller
                if (inventoryController.MoveItemToSlot(other.slotIndex, slotIndex))
                {
                    // Success - update UI
                    inventoryUI.UpdateUI();
                }
            }
            else
            {
                // Swap items using inventory controller
                if (inventoryController.SwapSlots(other.slotIndex, slotIndex))
                {
                    // Success - update UI
                    inventoryUI.UpdateUI();
                }
            }
        }
        
        private void HandleEquipmentDrop(EquipmentSlotUI equipmentSlot)
        {
            // This method is called from OnEndDrag when dropping on equipment
            // The actual equipping will be handled by the EquipmentSlotUI.OnDrop method
            // which will call back to remove the item from this slot
        }
        
        public void RemoveItemForEquipping()
        {
            // Public method to allow EquipmentSlotUI to remove item from this slot
            var inventoryControllerImpl = inventoryUI.GetComponent<IInventoryController>() as InventoryController;
            if (inventoryControllerImpl == null)
                inventoryControllerImpl = FindObjectOfType<InventoryController>();
            
            if (inventoryControllerImpl != null)
            {
                inventoryControllerImpl.SetItemAtSlot(slotIndex, null, 0);
            }
        }
        
        private void HandleEquipmentSlotDrop(EquipmentSlotUI equipmentSlot)
        {
            // Get inventory controller to properly manage items
            var inventoryControllerImpl = inventoryUI.GetComponent<IInventoryController>() as InventoryController;
            if (inventoryControllerImpl == null)
                inventoryControllerImpl = FindObjectOfType<InventoryController>();
            
            if (inventoryControllerImpl == null)
            {
                Debug.LogWarning("InventoryController not found - cannot handle equipment drop");
                return;
            }
            
            // Handle dropping from an equipment slot to inventory slot
            if (equipmentSlot.CurrentEquippedItem != null)
            {
                if (currentItem == null)
                {
                    // Empty slot - place unequipped item here
                    inventoryControllerImpl.SetItemAtSlot(slotIndex, equipmentSlot.CurrentEquippedItem, 1);
                    equipmentSlot.UnequipItem();
                    inventoryUI.UpdateUI();
                }
                else if (currentItem is EquippableItemDefinition currentEquippable)
                {
                    // Both slots have items - validate swap first
                    if (currentEquippable.equipmentSlot == equipmentSlot.SlotType ||
                        (equipmentSlot.SlotType == EquipmentSlot.MainHand && currentEquippable.equipmentSlot == EquipmentSlot.TwoHanded) ||
                        (equipmentSlot.SlotType == EquipmentSlot.OffHand && currentEquippable.equipmentSlot == EquipmentSlot.TwoHanded))
                    {
                        // Valid swap - swap the items
                        var tempItem = equipmentSlot.CurrentEquippedItem;
                        equipmentSlot.EquipItem(currentEquippable); // This will unequip the old item
                        inventoryControllerImpl.SetItemAtSlot(slotIndex, tempItem, 1);
                        inventoryUI.UpdateUI();
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot equip {currentEquippable.itemName} to {equipmentSlot.SlotType} slot");
                    }
                }
                else
                {
                    Debug.LogWarning("Cannot swap equipped item with non-equippable item");
                }
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && 
                currentItem != null && currentItem.isStackable && stackCount > 1)
            {
                // Show stack split dialog
                var splitDialog = inventoryUI.GetComponent<StackSplitDialog>();
                if (splitDialog != null)
                {
                    splitDialog.OnStackSplit = OnStackSplit;
                    splitDialog.ShowDialog(this, currentItem, stackCount);
                }
            }
        }
        
        private void OnStackSplit(int splitAmount)
        {
            if (splitAmount > 0 && splitAmount < stackCount)
            {
                var inventoryController = inventoryUI.GetComponent<IInventoryController>() as InventoryController;
                if (inventoryController == null)
                    inventoryController = FindObjectOfType<InventoryController>();
                    
                if (inventoryController == null)
                {
                    Debug.LogWarning("InventoryController not found - cannot handle stack split");
                    return;
                }
                
                // Find empty slot for split items
                int emptySlot = inventoryController.GetFirstEmptySlot();
                if (emptySlot >= 0)
                {
                    int remainingAmount = stackCount - splitAmount;
                    
                    // Update current slot with remaining items
                    inventoryController.SetItemAtSlot(slotIndex, currentItem, remainingAmount);
                    
                    // Place split items in empty slot
                    inventoryController.SetItemAtSlot(emptySlot, currentItem, splitAmount);
                    
                    inventoryUI.UpdateUI();
                }
                else
                {
                    Debug.LogWarning("No empty slot available for stack split");
                }
            }
        }
    }
}