using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Transform equipmentGridContainer;
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
        
        private IInventoryController inventoryController;
        private IEquipmentController equipmentController;
        private List<InventorySlot> equipmentSlots = new List<InventorySlot>();
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();
        private List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();
        private List<EquipmentSlotUI> characterEquipmentSlots = new List<EquipmentSlotUI>();
        
        private bool isInventoryOpen = false;
        private int currentHotbarIndex = 0;
        
        private void Start()
        {
            // Find controllers - they might be on the same GameObject or parent
            inventoryController = GetComponentInParent<IInventoryController>();
            if (inventoryController == null)
                inventoryController = FindObjectOfType<InventoryController>();
            
            equipmentController = GetComponentInParent<IEquipmentController>();
            if (equipmentController == null)
                equipmentController = FindObjectOfType<EquipmentController>();
            
            InitializeUI();
            
            // Only update UI if controllers are found
            if (inventoryController != null && equipmentController != null)
            {
                UpdateUI();
            }
            else
            {
                Debug.LogWarning("InventoryUI: Could not find InventoryController or EquipmentController!");
            }
        }
        
        private void InitializeUI()
        {
            CreateEquipmentGrid();
            CreateInventoryGrid();
            CreateHotbar();
            CreateEquipmentSlots();
            
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
        
        private void CreateEquipmentGrid()
        {
            if (inventorySlotPrefab == null || equipmentGridContainer == null)
            {
                Debug.LogWarning("InventoryUI: inventorySlotPrefab or equipmentGridContainer not assigned!");
                return;
            }
            
            for (int i = 0; i < 8; i++)
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, equipmentGridContainer);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.Initialize(this, i, true);
                    equipmentSlots.Add(slot);
                }
            }
        }
        
        private void CreateInventoryGrid()
        {
            if (inventorySlotPrefab == null || inventoryGridContainer == null)
            {
                Debug.LogWarning("InventoryUI: inventorySlotPrefab or inventoryGridContainer not assigned!");
                return;
            }
            
            for (int i = 0; i < 32; i++)
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGridContainer);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.Initialize(this, i + 8, false);
                    inventorySlots.Add(slot);
                }
            }
        }
        
        private void CreateHotbar()
        {
            if (hotbarSlotPrefab == null || hotbarContainer == null)
            {
                Debug.LogWarning("InventoryUI: hotbarSlotPrefab or hotbarContainer not assigned!");
                return;
            }
            
            for (int i = 0; i < 5; i++)
            {
                GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarContainer);
                HotbarSlot slot = slotObj.GetComponent<HotbarSlot>();
                if (slot != null)
                {
                    slot.Initialize(this, i);
                    hotbarSlots.Add(slot);
                }
            }
        }
        
        private void CreateEquipmentSlots()
        {
            if (equipmentSlotPrefab == null || equipmentSlotsContainer == null)
            {
                Debug.LogWarning("InventoryUI: equipmentSlotPrefab or equipmentSlotsContainer not assigned!");
                return;
            }
            
            var equipmentSlotTypes = System.Enum.GetValues(typeof(EquipmentSlot));
            foreach (EquipmentSlot slotType in equipmentSlotTypes)
            {
                GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                EquipmentSlotUI slot = slotObj.GetComponent<EquipmentSlotUI>();
                if (slot != null)
                {
                    slot.Initialize(this, slotType);
                    characterEquipmentSlots.Add(slot);
                }
            }
        }
        
        public void ToggleInventory()
        {
            if (inventoryPanel == null)
            {
                Debug.LogWarning("InventoryUI: inventoryPanel is not assigned!");
                return;
            }
            
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
            
            if (isInventoryOpen)
            {
                if (inventoryController != null && equipmentController != null)
                {
                    UpdateUI();
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                HideTooltip();
            }
        }
        
        public void SelectHotbarSlot(int index)
        {
            if (hotbarSlots == null || index < 0 || index >= hotbarSlots.Count) return;
            
            currentHotbarIndex = index;
            UpdateHotbarSelection();
            
            var item = GetHotbarItem(index);
            if (item != null && item is EquippableItemDefinition equippable && equipmentController != null)
            {
                equipmentController.EquipItem(equippable);
            }
        }
        
        public void CycleEquippedItems(bool forward)
        {
            if (forward)
            {
                currentHotbarIndex = (currentHotbarIndex + 1) % 5;
            }
            else
            {
                currentHotbarIndex = (currentHotbarIndex - 1 + 5) % 5;
            }
            
            SelectHotbarSlot(currentHotbarIndex);
        }
        
        private void UpdateHotbarSelection()
        {
            if (hotbarSlots == null) return;
            
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                hotbarSlots[i].SetSelected(i == currentHotbarIndex);
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
            
            var items = inventoryController.GetAllItems();
            if (items == null) return;
            
            // Update equipment slots (equippable items)
            if (equipmentSlots != null)
            {
                var equippableItems = items.FindAll(item => item.Key is EquippableItemDefinition);
                for (int i = 0; i < equipmentSlots.Count; i++)
                {
                    if (i < equippableItems.Count)
                    {
                        equipmentSlots[i].SetItem(equippableItems[i].Key, equippableItems[i].Value);
                    }
                    else
                    {
                        equipmentSlots[i].SetItem(null, 0);
                    }
                }
            }
            
            // Update regular inventory slots (non-equippable items)
            if (inventorySlots != null)
            {
                var regularItems = items.FindAll(item => !(item.Key is EquippableItemDefinition));
                for (int i = 0; i < inventorySlots.Count; i++)
                {
                    if (i < regularItems.Count)
                    {
                        inventorySlots[i].SetItem(regularItems[i].Key, regularItems[i].Value);
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
            if (hotbarSlots == null) return;
            
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                var item = GetHotbarItem(i);
                hotbarSlots[i].SetItem(item);
            }
            UpdateHotbarSelection();
        }
        
        private ItemDefinition GetHotbarItem(int index)
        {
            if (inventoryController == null) return null;
            
            var items = inventoryController.GetAllItems();
            if (items == null) return null;
            
            var equippableItems = items.FindAll(item => item.Key is EquippableItemDefinition);
            return index < equippableItems.Count ? equippableItems[index].Key : null;
        }
        
        private void UpdateEquipmentSlots()
        {
            // Update based on currently equipped items
            // This will be implemented when we add the equipment slot logic
        }
        
        public void ShowTooltip(ItemDefinition item, Vector3 position)
        {
            if (item == null) return;
            
            tooltipTitle.text = item.itemName;
            tooltipDescription.text = item.description;
            tooltipPanel.transform.position = position;
            tooltipPanel.SetActive(true);
        }
        
        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
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
            // Find first empty slot in inventory
            foreach (var slot in inventorySlots)
            {
                if (slot.CurrentItem == null)
                {
                    slot.SetItem(item, amount);
                    break;
                }
            }
            
            // If no empty slot in regular inventory, try equipment slots
            if (item is EquippableItemDefinition)
            {
                foreach (var slot in equipmentSlots)
                {
                    if (slot.CurrentItem == null)
                    {
                        slot.SetItem(item, amount);
                        break;
                    }
                }
            }
        }
    }
}