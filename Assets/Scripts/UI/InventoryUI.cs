using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using GameFramework.Items;
using GameFramework.Core.Interfaces;
using static GameFramework.Items.EquippableItemDefinition;

namespace GameFramework.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject hotbarPanel;
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private GameObject tooltipPanel;
        
        [Header("Grid Containers")]
        [SerializeField] private Transform inventoryGridContainer;
        [SerializeField] private Transform hotbarContainer;
        [SerializeField] private Transform equipmentSlotsContainer;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private GameObject hotbarSlotPrefab;
        [SerializeField] private GameObject equipmentSlotPrefab;
        
        [Header("Tooltip")]
        [SerializeField] private TextMeshProUGUI tooltipTitle;
        [SerializeField] private TextMeshProUGUI tooltipDescription;
        
        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float startScale = 0.8f;
        
        private IInventoryController inventoryController;
        private IEquipmentController equipmentController;
        private HotbarController hotbarController;
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();
        private List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();
        private List<EquipmentSlotUI> characterEquipmentSlots = new List<EquipmentSlotUI>();
        
        private bool isInventoryOpen = false;
        private bool isTooltipVisible = false;
        private bool isAnimating = false;
        private Coroutine currentAnimation;
        
        public bool IsInventoryOpen => isInventoryOpen;
        
        private void Start()
        {
            // Find controllers - they might be on the same GameObject or parent
            inventoryController = GetComponentInParent<IInventoryController>();
            if (inventoryController == null)
                inventoryController = FindObjectOfType<InventoryController>();
            
            equipmentController = GetComponentInParent<IEquipmentController>();
            if (equipmentController == null)
                equipmentController = FindObjectOfType<EquipmentController>();
                
            hotbarController = GetComponent<HotbarController>();
            if (hotbarController == null)
                hotbarController = FindObjectOfType<HotbarController>();
            
            InitializeUI();
            
            // Only update UI if controllers are found
            if (inventoryController != null && equipmentController != null && hotbarController != null)
            {
                // Subscribe to hotbar events
                hotbarController.OnHotbarChanged += OnHotbarItemChanged;
                hotbarController.OnSelectionChanged += OnHotbarSelectionChanged;
                
                UpdateUI();
            }
            else
            {
            }
        }
        
        private void Update()
        {
            // Update tooltip position to follow mouse if tooltip is visible
            if (isTooltipVisible && tooltipPanel != null && tooltipPanel.activeInHierarchy)
            {
                tooltipPanel.transform.position = Mouse.current.position.ReadValue();
            }
        }
        
        private void InitializeUI()
        {
            CreateInventoryGrid();
            CreateHotbar();
            CreateEquipmentSlots();
            
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
                // Initialize scale for future animations
                inventoryPanel.transform.localScale = Vector3.one;
            }
            
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
        
        
        private void CreateInventoryGrid()
        {
            if (inventorySlotPrefab == null || inventoryGridContainer == null)
            {
                return;
            }
            
            // Create unified inventory grid for all items (equipment and regular items)
            for (int i = 0; i < 40; i++) // Increased to 40 slots for more space
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGridContainer);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.Initialize(this, i); // Simplified initialization
                    inventorySlots.Add(slot);
                }
            }
        }
        
        private void CreateHotbar()
        {
            if (hotbarSlotPrefab == null || hotbarContainer == null)
            {
                return;
            }
            
            if (hotbarController == null)
            {
                return;
            }
            
            for (int i = 0; i < hotbarController.HotbarSize; i++)
            {
                GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarContainer);
                HotbarSlot slot = slotObj.GetComponent<HotbarSlot>();
                if (slot != null)
                {
                    slot.Initialize(this, hotbarController, i);
                    hotbarSlots.Add(slot);
                }
            }
        }
        
        private void CreateEquipmentSlots()
        {
            if (equipmentSlotPrefab == null || equipmentSlotsContainer == null)
            {
                return;
            }
            
            if (equipmentController == null)
            {
                return;
            }
            
            if (inventoryController == null)
            {
                return;
            }
            
            var equipmentSlotTypes = System.Enum.GetValues(typeof(EquipmentSlot));
            foreach (EquipmentSlot slotType in equipmentSlotTypes)
            {
                
                GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                EquipmentSlotUI slot = slotObj.GetComponent<EquipmentSlotUI>();
                if (slot != null)
                {
                    slot.Initialize(this, slotType, equipmentController, inventoryController);
                    characterEquipmentSlots.Add(slot);
                }
    
            }
            
        }
        
        public void ToggleInventory()
        {
            if (inventoryPanel == null)
            {
                return;
            }
            
            // Prevent toggling while animating
            if (isAnimating)
                return;
            
            // Stop any current animation
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            
            isInventoryOpen = !isInventoryOpen;
            
            if (isInventoryOpen)
            {
                if (inventoryController != null && equipmentController != null && hotbarController != null)
                {
                    UpdateUI();
                }
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                
                // Start scale-in animation using a safe coroutine runner
                currentAnimation = StartCoroutineIfPossible(AnimateScaleIn());
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                HideTooltip();
                
                // Start scale-out animation using a safe coroutine runner
                currentAnimation = StartCoroutineIfPossible(AnimateScaleOut());
            }
        }
        
        public void SelectHotbarSlot(int index)
        {
            if (hotbarController != null)
            {
                hotbarController.SelectHotbarSlot(index);
            }
        }
        
        public void CycleEquippedItems(bool forward)
        {
            if (hotbarController != null)
            {
                hotbarController.CycleSelection(forward);
            }
        }
        
        private void UpdateHotbarSelection()
        {
            if (hotbarSlots == null || hotbarController == null) return;
            
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                hotbarSlots[i].SetSelected(i == hotbarController.CurrentSelectedIndex);
            }
        }
        
        public void UpdateUI()
        {
            UpdateInventorySlots();
            UpdateHotbar();
            UpdateEquipmentSlots();
        }
        
        private void UpdateInventorySlots()
        {
            if (inventoryController == null) return;
            
            var inventoryControllerImpl = inventoryController as InventoryController;
            if (inventoryControllerImpl == null)
            {
                // Fallback to old method if not using slot-based controller
                UpdateInventorySlotsLegacy();
                return;
            }
            
            // Update all inventory slots using slot-based system
            if (inventorySlots != null)
            {
                for (int i = 0; i < inventorySlots.Count; i++)
                {
                    var itemStack = inventoryControllerImpl.GetItemAtSlot(i);
                    if (itemStack != null)
                    {
                        inventorySlots[i].SetItem(itemStack.item, itemStack.quantity);
                    }
                    else
                    {
                        inventorySlots[i].SetItem(null, 0);
                    }
                }
            }
        }
        
        private void UpdateInventorySlotsLegacy()
        {
            var items = inventoryController.GetAllItems();
            if (items == null) return;
            
            // Update all inventory slots with all items (no separation)
            if (inventorySlots != null)
            {
                for (int i = 0; i < inventorySlots.Count; i++)
                {
                    if (i < items.Count)
                    {
                        inventorySlots[i].SetItem(items[i].Key as ItemDefinition, items[i].Value);
                    }
                    else
                    {
                        inventorySlots[i].SetItem(null, 0);
                    }
                }
            }
        }
        
        private void UpdateHotbar()
        {
            if (hotbarSlots == null || hotbarController == null) return;
            
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                var item = hotbarController.GetHotbarItem(i);
                hotbarSlots[i].SetItem(item);
            }
            UpdateHotbarSelection();
        }
        
        private void OnHotbarItemChanged(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
            {
                var item = hotbarController.GetHotbarItem(slotIndex);
                hotbarSlots[slotIndex].SetItem(item);
            }
        }
        
        private void OnHotbarSelectionChanged(int newIndex)
        {
            UpdateHotbarSelection();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (hotbarController != null)
            {
                hotbarController.OnHotbarChanged -= OnHotbarItemChanged;
                hotbarController.OnSelectionChanged -= OnHotbarSelectionChanged;
            }
        }
        
        private void UpdateEquipmentSlots()
        {
            
            if (equipmentController == null)
            {
                return;
            }
            
            // Update each character equipment slot with currently equipped items
            foreach (var equipmentSlot in characterEquipmentSlots)
            {
                if (equipmentSlot != null)
                {
                    // Get the slot type from the equipment slot
                    var slotType = equipmentSlot.SlotType;
                    string slotName = slotType.ToString(); // Convert enum to string
                    
                    
                    // Get the currently equipped item for this slot
                    var equippedItemObj = equipmentController.GetEquippedItem(slotName);
                    var equippedItem = equippedItemObj as GameFramework.Items.EquippedItem;
                    
                    if (equippedItem != null)
                    {
                        equipmentSlot.SetEquippedItem(equippedItem.item);
                    }
                    else
                    {
                        equipmentSlot.SetEquippedItem(null);
                    }
                }
            }
        }
        
        public void ShowTooltip(ItemDefinition item, Vector3 position)
        {
            if (item == null) return;
            
            tooltipTitle.text = item.itemName;
            tooltipDescription.text = item.description;
            tooltipPanel.transform.position = position;
            tooltipPanel.SetActive(true);
            isTooltipVisible = true;
        }
        
        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
            isTooltipVisible = false;
        }
        
        public void OnItemDrag(ItemDefinition item, int fromSlot, bool isEquipment)
        {
            // Handle drag operations
        }
        
        public void OnItemDrop(ItemDefinition item, int toSlot, bool isEquipment)
        {
            // Handle drop operations
        }
        
        public void PlaceSplitStack(ItemDefinition item, int amount)
        {
            var inventoryControllerImpl = inventoryController as InventoryController;
            if (inventoryControllerImpl != null)
            {
                // Use slot-based system
                int emptySlot = inventoryControllerImpl.GetFirstEmptySlot();
                if (emptySlot >= 0)
                {
                    inventoryControllerImpl.SetItemAtSlot(emptySlot, item, amount);
                    UpdateUI();
                }
            }
            else
            {
                // Fallback to UI-based search
                foreach (var slot in inventorySlots)
                {
                    if (slot.CurrentItem == null)
                    {
                        slot.SetItem(item, amount);
                        break;
                    }
                }
            }
        }
        
        private IEnumerator AnimateScaleIn()
        {
            isAnimating = true;
            
            // Set initial scale and activate panel
            inventoryPanel.transform.localScale = Vector3.one * startScale;
            inventoryPanel.SetActive(true);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = scaleCurve.Evaluate(progress);
                
                // Interpolate from startScale to 1.0
                float currentScale = Mathf.Lerp(startScale, 1f, curveValue);
                inventoryPanel.transform.localScale = Vector3.one * currentScale;
                
                yield return null;
            }
            
            // Ensure final scale is exactly 1
            inventoryPanel.transform.localScale = Vector3.one;
            isAnimating = false;
        }
        
        private IEnumerator AnimateScaleOut()
        {
            isAnimating = true;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = scaleCurve.Evaluate(1f - progress); // Reverse the curve
                
                // Interpolate from 1.0 to startScale
                float currentScale = Mathf.Lerp(startScale, 1f, curveValue);
                inventoryPanel.transform.localScale = Vector3.one * currentScale;
                
                yield return null;
            }
            
            // Ensure final scale and deactivate panel
            inventoryPanel.transform.localScale = Vector3.one * startScale;
            inventoryPanel.SetActive(false);
            isAnimating = false;
        }
        
        private Coroutine StartCoroutineIfPossible(IEnumerator routine)
        {
            // First try to start on this component if it's active
            if (gameObject.activeInHierarchy)
            {
                return StartCoroutine(routine);
            }
            
            // If this component is not active, find an alternative MonoBehaviour
            var characterControllers = FindObjectsOfType<MonoBehaviour>();
            MonoBehaviour characterController = null;
            foreach (var controller in characterControllers)
            {
                if (controller is ICharacterController)
                {
                    characterController = controller;
                    break;
                }
            }
            if (characterController != null && characterController.gameObject.activeInHierarchy)
            {
                return characterController.StartCoroutine(routine);
            }
            
            // Last resort: find any active MonoBehaviour in the scene
            var activeMonoBehaviour = FindObjectOfType<MonoBehaviour>();
            if (activeMonoBehaviour != null && activeMonoBehaviour.gameObject.activeInHierarchy)
            {
                return activeMonoBehaviour.StartCoroutine(routine);
            }
            
            // If we can't find any active MonoBehaviour, fall back to immediate show/hide
            if (isInventoryOpen)
            {
                inventoryPanel.SetActive(true);
                inventoryPanel.transform.localScale = Vector3.one;
            }
            else
            {
                inventoryPanel.SetActive(false);
            }
            
            return null;
        }
    }
}