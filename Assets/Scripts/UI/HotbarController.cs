using System.Collections.Generic;
using UnityEngine;
using GameFramework.Items;
using GameFramework.Core.Interfaces;
using static GameFramework.Items.EquippableItemDefinition;

namespace GameFramework.UI
{
    public class HotbarController : MonoBehaviour, IHotbarController
    {
        [Header("Hotbar Settings")]
        [SerializeField] private int hotbarSize = 5;
        [SerializeField] private bool debugMode = true;
        
        private EquippableItemDefinition[] hotbarItems;
        private int currentSelectedIndex = 0;
        private IEquipmentController equipmentController;
        private IInventoryController inventoryController;
        private EquippableItemDefinition lastEquippedItem;
        
        public int HotbarSize => hotbarSize;
        public int CurrentSelectedIndex => currentSelectedIndex;
        
        public System.Action<int> OnHotbarChanged;
        public System.Action<int> OnSelectionChanged;
        
        private void Awake()
        {
            hotbarItems = new EquippableItemDefinition[hotbarSize];
            
            // Find controllers
            equipmentController = GetComponent<IEquipmentController>();
            if (equipmentController == null)
                equipmentController = FindObjectOfType<EquipmentController>();
                
            inventoryController = GetComponent<IInventoryController>();
            if (inventoryController == null)
                inventoryController = FindObjectOfType<InventoryController>();
        }
        
        public EquippableItemDefinition GetHotbarItem(int index)
        {
            if (index < 0 || index >= hotbarSize) return null;
            return hotbarItems[index];
        }
        
        public bool SetHotbarItem(int index, EquippableItemDefinition item)
        {
            if (index < 0 || index >= hotbarSize) return false;
            
            // Validate that item can be equipped to main hand
            if (item != null && !CanEquipToMainHand(item))
            {
                if (debugMode)
                    Debug.LogWarning($"Cannot add {item.itemName} to hotbar - not a main hand item");
                return false;
            }
            
            // Check if item already exists in hotbar - if so, move it instead of duplicating
            if (item != null)
            {
                int existingIndex = FindItemInHotbar(item);
                if (existingIndex >= 0 && existingIndex != index)
                {
                    // Remove from existing slot and move to new slot
                    if (debugMode)
                        Debug.Log($"Moving {item.itemName} from hotbar slot {existingIndex} to slot {index}");
                    
                    hotbarItems[existingIndex] = null;
                    OnHotbarChanged?.Invoke(existingIndex); // This should clear the visual
                }
            }
            
            // Check if item is available in inventory (for reference system)
            if (item != null && inventoryController != null && !inventoryController.HasItem(item, 1))
            {
                // For equipped items, we allow them even if not in inventory
                if (equipmentController != null)
                {
                    var equippedItemObj = equipmentController.GetEquippedItem("MainHand");
                    var equippedItem = equippedItemObj as GameFramework.Items.EquippedItem;
                    if (equippedItem == null || equippedItem.item != item)
                    {
                        if (debugMode)
                            Debug.LogWarning($"Cannot add {item.itemName} to hotbar - not available in inventory or equipped");
                        return false;
                    }
                }
                else
                {
                    if (debugMode)
                        Debug.LogWarning($"Cannot add {item.itemName} to hotbar - not available in inventory");
                    return false;
                }
            }
            
            hotbarItems[index] = item;
            OnHotbarChanged?.Invoke(index);
            
            if (debugMode)
                Debug.Log($"Set hotbar slot {index} to {(item?.itemName ?? "empty")}");
            
            return true;
        }
        
        public bool AddItemToHotbar(EquippableItemDefinition item)
        {
            if (item == null || !CanEquipToMainHand(item)) return false;
            
            // Check if item already exists in hotbar
            int existingIndex = FindItemInHotbar(item);
            if (existingIndex >= 0)
            {
                if (debugMode)
                    Debug.Log($"Item {item.itemName} already in hotbar at slot {existingIndex}");
                return true; // Already in hotbar
            }
            
            // Find first empty slot and use SetHotbarItem for consistent logic
            for (int i = 0; i < hotbarSize; i++)
            {
                if (hotbarItems[i] == null)
                {
                    return SetHotbarItem(i, item);
                }
            }
            
            if (debugMode)
                Debug.LogWarning("Hotbar is full - cannot add item");
            return false;
        }
        
        public bool RemoveHotbarItem(int index)
        {
            if (index < 0 || index >= hotbarSize) return false;
            
            hotbarItems[index] = null;
            OnHotbarChanged?.Invoke(index);
            
            if (debugMode)
                Debug.Log($"Removed item from hotbar slot {index}");
            
            return true;
        }
        
        public bool SwapHotbarItems(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= hotbarSize || toIndex < 0 || toIndex >= hotbarSize)
                return false;
            
            var fromItem = hotbarItems[fromIndex];
            var toItem = hotbarItems[toIndex];
            
            if (debugMode)
                Debug.Log($"Swapping hotbar items: slot {fromIndex} ({fromItem?.itemName ?? "empty"}) <-> slot {toIndex} ({toItem?.itemName ?? "empty"})");
            
            // Direct swap without using SetHotbarItem to avoid duplicate prevention logic
            hotbarItems[fromIndex] = toItem;
            hotbarItems[toIndex] = fromItem;
            
            // Fire events for both slots to update visuals
            OnHotbarChanged?.Invoke(fromIndex);
            OnHotbarChanged?.Invoke(toIndex);
            
            return true;
        }
        
        public void SelectHotbarSlot(int index)
        {
            if (index < 0 || index >= hotbarSize) return;
            
            currentSelectedIndex = index;
            OnSelectionChanged?.Invoke(index);
            
            // Get the item for this slot
            var item = hotbarItems[index];
            if (item != null && inventoryController != null && equipmentController != null)
            {
                // Check if item is available (in inventory OR currently equipped)
                bool inInventory = inventoryController.HasItem(item, 1);
                bool alreadyEquipped = false;
                
                var currentEquippedObj = equipmentController.GetEquippedItem("MainHand");
                var currentEquipped = currentEquippedObj as GameFramework.Items.EquippedItem;
                if (currentEquipped != null && currentEquipped.item == item)
                {
                    alreadyEquipped = true;
                }
                
                if (inInventory && !alreadyEquipped)
                {
                    // Item is available in inventory and not already equipped - equip it
                    equipmentController.EquipItem(item, "MainHand");
                    lastEquippedItem = item;
                    
                    if (debugMode)
                        Debug.Log($"Equipped {item.itemName} from inventory to MainHand");
                }
                else if (alreadyEquipped)
                {
                    // Item is already equipped - no need to re-equip
                    lastEquippedItem = item;
                    
                    if (debugMode)
                        Debug.Log($"Item {item.itemName} already equipped in MainHand");
                }
                else if (!inInventory)
                {
                    // Item no longer available in inventory - remove from hotbar
                    if (debugMode)
                        Debug.LogWarning($"Item {item.itemName} no longer available in inventory, removing from hotbar");
                    hotbarItems[index] = null;
                    OnHotbarChanged?.Invoke(index);
                }
            }
            else if (equipmentController != null)
            {
                // Empty slot or no item - unequip main hand
                equipmentController.UnequipItem("MainHand");
                lastEquippedItem = null;
            }
            
            if (debugMode)
                Debug.Log($"Selected hotbar slot {index} - {(item?.itemName ?? "empty")}");
        }
        
        public void CycleSelection(bool forward)
        {
            int newIndex;
            if (forward)
            {
                newIndex = (currentSelectedIndex + 1) % hotbarSize;
            }
            else
            {
                newIndex = (currentSelectedIndex - 1 + hotbarSize) % hotbarSize;
            }
            
            SelectHotbarSlot(newIndex);
        }
        
        public int FindItemInHotbar(EquippableItemDefinition item)
        {
            if (item == null) return -1;
            
            for (int i = 0; i < hotbarSize; i++)
            {
                if (hotbarItems[i] == item)
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        public bool IsHotbarFull()
        {
            for (int i = 0; i < hotbarSize; i++)
            {
                if (hotbarItems[i] == null)
                    return false;
            }
            return true;
        }
        
        public List<EquippableItemDefinition> GetAllHotbarItems()
        {
            var items = new List<EquippableItemDefinition>();
            for (int i = 0; i < hotbarSize; i++)
            {
                if (hotbarItems[i] != null)
                    items.Add(hotbarItems[i]);
            }
            return items;
        }
        
        private bool CanEquipToMainHand(EquippableItemDefinition item)
        {
            if (item == null) return false;
            
            return item.equipmentSlot == EquipmentSlot.MainHand || 
                   item.equipmentSlot == EquipmentSlot.TwoHanded;
        }
        
        // Save/Load hotbar state (could be expanded for persistence)
        public EquippableItemDefinition[] GetHotbarState()
        {
            var state = new EquippableItemDefinition[hotbarSize];
            System.Array.Copy(hotbarItems, state, hotbarSize);
            return state;
        }
        
        public void SetHotbarState(EquippableItemDefinition[] state)
        {
            if (state == null || state.Length != hotbarSize) return;
            
            for (int i = 0; i < hotbarSize; i++)
            {
                SetHotbarItem(i, state[i]);
            }
        }
        
        // Clear hotbar
        public void ClearHotbar()
        {
            for (int i = 0; i < hotbarSize; i++)
            {
                RemoveHotbarItem(i);
            }
            
            if (debugMode)
                Debug.Log("Cleared all hotbar slots");
        }
        
        // Validate hotbar items against current inventory
        public void ValidateHotbarItems()
        {
            if (inventoryController == null || equipmentController == null) return;
            
            bool anyChanges = false;
            for (int i = 0; i < hotbarSize; i++)
            {
                var item = hotbarItems[i];
                if (item != null)
                {
                    // Check if item is in inventory OR currently equipped in main hand
                    bool inInventory = inventoryController.HasItem(item, 1);
                    bool currentlyEquipped = false;
                    
                    var equippedItemObj = equipmentController.GetEquippedItem("MainHand");
                    var equippedItem = equippedItemObj as GameFramework.Items.EquippedItem;
                    if (equippedItem != null && equippedItem.item == item)
                    {
                        currentlyEquipped = true;
                    }
                    
                    // Only remove from hotbar if item is neither in inventory nor equipped
                    if (!inInventory && !currentlyEquipped)
                    {
                        if (debugMode)
                            Debug.Log($"Removing {item.itemName} from hotbar slot {i} - no longer available");
                        hotbarItems[i] = null;
                        OnHotbarChanged?.Invoke(i);
                        anyChanges = true;
                    }
                }
            }
            
            if (anyChanges && debugMode)
                Debug.Log("Hotbar validated and updated");
        }

        // Interface implementations
        public void UpdateHotbarSlot(int slotIndex, object itemStack)
        {
            if (itemStack is ItemStack stack && stack.item is EquippableItemDefinition equipItem)
            {
                SetHotbarItem(slotIndex, equipItem);
            }
            else if (itemStack is EquippableItemDefinition equipItem2)
            {
                SetHotbarItem(slotIndex, equipItem2);
            }
            else
            {
                SetHotbarItem(slotIndex, null);
            }
        }

        public void RefreshHotbar()
        {
            ValidateHotbarItems();
        }

        object IHotbarController.GetHotbarItem(int slotIndex)
        {
            var item = GetHotbarItem(slotIndex);
            return item != null ? new ItemStack(item, 1) : null;
        }

        public int GetHotbarSize()
        {
            return hotbarSize;
        }

        public bool AddItemToHotbar(object item)
        {
            if (item is EquippableItemDefinition equipItem)
            {
                return AddItemToHotbar(equipItem);
            }
            return false;
        }

        public int FindItemInHotbar(object item)
        {
            if (item is EquippableItemDefinition equipItem)
            {
                return FindItemInHotbar(equipItem);
            }
            return -1;
        }
    }
}