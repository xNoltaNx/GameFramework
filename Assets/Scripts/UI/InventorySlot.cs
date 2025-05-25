using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameFramework.Items;

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
        private bool isEquipmentSlot;
        private bool isDragging;
        private GameObject dragIcon;
        
        public ItemDefinition CurrentItem => currentItem;
        
        public void Initialize(InventoryUI ui, int index, bool equipmentSlot)
        {
            inventoryUI = ui;
            slotIndex = index;
            isEquipmentSlot = equipmentSlot;
            
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
                Vector3 tooltipPosition = transform.position + Vector3.up * 100f;
                inventoryUI.ShowTooltip(currentItem, tooltipPosition);
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
            
            // Create drag icon
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(transform.root, false);
            dragIcon.transform.SetAsLastSibling();
            
            Image dragImage = dragIcon.AddComponent<Image>();
            dragImage.sprite = currentItem.icon;
            dragImage.raycastTarget = false;
            
            CanvasGroup canvasGroup = dragIcon.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
            
            RectTransform dragRect = dragIcon.GetComponent<RectTransform>();
            dragRect.sizeDelta = itemIcon.rectTransform.sizeDelta;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                dragIcon.transform.position = eventData.position;
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            
            if (dragIcon != null)
            {
                Destroy(dragIcon);
                dragIcon = null;
            }
            
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
            if (draggedSlot != null && draggedSlot != this)
            {
                HandleSlotDrop(draggedSlot);
            }
        }
        
        private void HandleSlotDrop(InventorySlot other)
        {
            // Handle combining stacks or swapping items
            if (other.currentItem == currentItem && currentItem != null && currentItem.isStackable)
            {
                // Combine stacks
                int totalStack = stackCount + other.stackCount;
                if (totalStack <= currentItem.maxStackSize)
                {
                    SetItem(currentItem, totalStack);
                    other.SetItem(null, 0);
                }
                else
                {
                    SetItem(currentItem, currentItem.maxStackSize);
                    other.SetItem(currentItem, totalStack - currentItem.maxStackSize);
                }
            }
            else
            {
                // Swap items
                var tempItem = currentItem;
                var tempCount = stackCount;
                
                SetItem(other.currentItem, other.stackCount);
                other.SetItem(tempItem, tempCount);
            }
            
            inventoryUI.UpdateUI();
        }
        
        private void HandleEquipmentDrop(EquipmentSlotUI equipmentSlot)
        {
            if (currentItem is EquippableItemDefinition equippable)
            {
                equipmentSlot.EquipItem(equippable);
                SetItem(null, 0);
                inventoryUI.UpdateUI();
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
                // Find empty slot for split items
                int remainingAmount = stackCount - splitAmount;
                
                // Update current slot with remaining items
                SetItem(currentItem, remainingAmount);
                
                // Find empty slot and place split items there
                // This would need to be implemented by the inventory system
                inventoryUI.PlaceSplitStack(currentItem, splitAmount);
                inventoryUI.UpdateUI();
            }
        }
    }
}