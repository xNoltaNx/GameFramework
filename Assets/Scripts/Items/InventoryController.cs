using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Items
{
    [System.Serializable]
    public class ItemStack
    {
        public ItemDefinition item;
        public int quantity;
        
        public ItemStack(ItemDefinition item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    public class InventoryController : MonoBehaviour, IInventoryController
    {
        [Header("Inventory Settings")]
        [SerializeField] private int capacity = 40; // Increased for unified inventory
        [SerializeField] private bool unlimitedCapacity = false;
        
        [Header("Auto-Equip Settings")]
        [SerializeField] private bool autoEquipOnPickup = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        private ItemStack[] inventorySlots;
        private List<ItemStack> items = new List<ItemStack>();
        private IEquipmentController equipmentController;
        private GameFramework.UI.HotbarController hotbarController;
        
        public IReadOnlyList<ItemStack> Items => items;
        public int Capacity => unlimitedCapacity ? int.MaxValue : capacity;
        
        private void Awake()
        {
            if (items == null)
            {
                items = new List<ItemStack>();
            }
            
            // Initialize slot-based inventory array
            int totalCapacity = unlimitedCapacity ? 100 : capacity;
            inventorySlots = new ItemStack[totalCapacity];
            
            // Sync existing items to slots if any
            SyncItemsListToSlots();
            
            // Find equipment controller on the same GameObject or parent
            equipmentController = GetComponent<IEquipmentController>();
            if (equipmentController == null)
            {
                equipmentController = GetComponentInParent<IEquipmentController>();
            }
            
            // Find hotbar controller
            hotbarController = GetComponent<GameFramework.UI.HotbarController>();
            if (hotbarController == null)
            {
                hotbarController = FindObjectOfType<GameFramework.UI.HotbarController>();
            }
        }
        
        public bool CanAddItem(ItemDefinition item, int quantity = 1)
        {
            if (debugMode)
            {
                Debug.Log($"InventoryController.CanAddItem: {item?.GetDisplayName()} x{quantity}");
            }
            
            if (item == null || quantity <= 0) 
            {
                if (debugMode) Debug.Log("CanAddItem: false - null item or invalid quantity");
                return false;
            }
            
            if (item.CanStack())
            {
                // Check existing stacks
                ItemStack existingStack = FindItemStack(item);
                if (existingStack != null)
                {
                    int availableSpace = item.maxStackSize - existingStack.quantity;
                    if (availableSpace >= quantity)
                    {
                        return true; // Can fit in existing stack
                    }
                    quantity -= availableSpace; // Remaining quantity after filling existing stack
                }
                
                // Check if we can create new stacks for remaining quantity
                int stacksNeeded = Mathf.CeilToInt((float)quantity / item.maxStackSize);
                int availableSlots = Capacity - items.Count;
                
                return availableSlots >= stacksNeeded;
            }
            else
            {
                // Non-stackable items need individual slots
                int availableSlots = Capacity - items.Count;
                return availableSlots >= quantity;
            }
        }
        
        public bool AddItem(ItemDefinition item, int quantity = 1)
        {
            if (!CanAddItem(item, quantity)) return false;
            
            if (debugMode)
            {
                Debug.Log($"Adding {quantity}x {item.GetDisplayName()} to inventory");
            }
            
            // Add items to inventory first, then check for auto-equip
            // This ensures items are always available in inventory for hotbar reference
            if (item.CanStack())
            {
                AddStackableItemToSlots(item, quantity);
            }
            else
            {
                AddNonStackableItemToSlots(item, quantity);
            }
            
            // Sync slots to list for backwards compatibility
            SyncSlotsToItemsList();
            
            // After adding to inventory, try to auto-equip the first item
            TryAutoEquipItem(item);
            
            return true;
        }
        
        private void AddStackableItemToSlots(ItemDefinition item, int quantity)
        {
            while (quantity > 0)
            {
                // Find existing stack with available space
                int existingSlotIndex = FindStackableSlot(item);
                
                if (existingSlotIndex >= 0)
                {
                    // Add to existing stack
                    var existingStack = inventorySlots[existingSlotIndex];
                    int availableSpace = item.maxStackSize - existingStack.quantity;
                    int amountToAdd = Mathf.Min(quantity, availableSpace);
                    
                    existingStack.quantity += amountToAdd;
                    quantity -= amountToAdd;
                }
                else
                {
                    // Create new stack in empty slot
                    int emptySlot = GetFirstEmptySlot();
                    if (emptySlot >= 0)
                    {
                        int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                        inventorySlots[emptySlot] = new ItemStack(item, amountToAdd);
                        quantity -= amountToAdd;
                    }
                    else
                    {
                        // No space available
                        break;
                    }
                }
            }
        }
        
        private void AddNonStackableItemToSlots(ItemDefinition item, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                int emptySlot = GetFirstEmptySlot();
                if (emptySlot >= 0)
                {
                    inventorySlots[emptySlot] = new ItemStack(item, 1);
                }
                else
                {
                    // No space available
                    break;
                }
            }
        }
        
        private void AddStackableItem(ItemDefinition item, int quantity)
        {
            AddStackableItemToSlots(item, quantity);
        }
        
        private void AddNonStackableItem(ItemDefinition item, int quantity)
        {
            AddNonStackableItemToSlots(item, quantity);
        }
        
        public bool RemoveItem(ItemDefinition item, int quantity = 1)
        {
            if (!HasItem(item, quantity)) return false;
            
            if (debugMode)
            {
                Debug.Log($"Removing {quantity}x {item.GetDisplayName()} from inventory");
            }
            
            int remainingToRemove = quantity;
            
            // Remove from slots
            for (int i = inventorySlots.Length - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                var stack = inventorySlots[i];
                if (stack != null && stack.item == item)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, stack.quantity);
                    stack.quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;
                    
                    if (stack.quantity <= 0)
                    {
                        inventorySlots[i] = null;
                    }
                }
            }
            
            // Sync to list for backwards compatibility
            SyncSlotsToItemsList();
            
            return true;
        }
        
        public bool HasItem(ItemDefinition item, int quantity = 1)
        {
            return GetItemCount(item) >= quantity;
        }
        
        public int GetItemCount(ItemDefinition item)
        {
            int count = 0;
            foreach (ItemStack stack in items)
            {
                if (stack.item == item)
                {
                    count += stack.quantity;
                }
            }
            return count;
        }
        
        public List<System.Collections.Generic.KeyValuePair<ItemDefinition, int>> GetAllItems()
        {
            var result = new List<System.Collections.Generic.KeyValuePair<ItemDefinition, int>>();
            foreach (ItemStack stack in items)
            {
                result.Add(new System.Collections.Generic.KeyValuePair<ItemDefinition, int>(stack.item, stack.quantity));
            }
            return result;
        }
        
        private ItemStack FindItemStack(ItemDefinition item)
        {
            foreach (ItemStack stack in items)
            {
                if (stack.item == item && stack.quantity < item.maxStackSize)
                {
                    return stack;
                }
            }
            return null;
        }
        
        private int FindStackableSlot(ItemDefinition item)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                var stack = inventorySlots[i];
                if (stack != null && stack.item == item && stack.quantity < item.maxStackSize)
                {
                    return i;
                }
            }
            return -1;
        }
        
        public void Clear()
        {
            if (debugMode)
            {
                Debug.Log("Clearing inventory");
            }
            items.Clear();
        }
        
        public void SetCapacity(int newCapacity)
        {
            capacity = newCapacity;
        }
        
        public void SetUnlimitedCapacity(bool unlimited)
        {
            unlimitedCapacity = unlimited;
        }
        
        public void SetAutoEquipOnPickup(bool autoEquip)
        {
            autoEquipOnPickup = autoEquip;
        }
        
        // Debug method to display inventory contents
        [ContextMenu("Debug Inventory Contents")]
        public void DebugInventoryContents()
        {
            Debug.Log($"=== Inventory Contents ({items.Count}/{Capacity}) ===");
            foreach (ItemStack stack in items)
            {
                Debug.Log($"- {stack.item.GetDisplayName()}: {stack.quantity}");
            }
        }
        
        /// <summary>
        /// Attempts to auto-equip an item if auto-equip is enabled and the slot is empty
        /// </summary>
        /// <param name="item">The item to potentially auto-equip</param>
        /// <returns>True if the item was auto-equipped, false otherwise</returns>
        private bool TryAutoEquipItem(ItemDefinition item)
        {
            // Check if auto-equip is enabled
            if (!autoEquipOnPickup || equipmentController == null)
                return false;
            
            // Check if item is equippable
            if (!(item is EquippableItemDefinition equippableItem))
                return false;
            
            // Get the slot name for this equipment type
            string slotName = equippableItem.equipmentSlot.ToString();
            
            // Check if the slot is currently empty
            var currentlyEquippedItem = equipmentController.GetEquippedItem(slotName);
            if (currentlyEquippedItem != null)
                return false; // Slot is occupied, don't auto-equip
            
            // Try to equip the item
            bool equipped = equipmentController.EquipItem(equippableItem, slotName);
            
            if (equipped)
            {
                // Remove one instance of the equipped item from inventory since it's now equipped
                if (RemoveItem(equippableItem, 1))
                {
                    if (debugMode)
                    {
                        Debug.Log($"Auto-equipped {item.GetDisplayName()} to {slotName} slot and removed from inventory");
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"Failed to remove {item.GetDisplayName()} from inventory after auto-equipping");
                    }
                }
                
                // If this is a main hand item and we have a hotbar controller, 
                // try to add it to the first available hotbar slot and select it
                if (hotbarController != null && 
                    (equippableItem.equipmentSlot == EquipmentSlot.MainHand || 
                     equippableItem.equipmentSlot == EquipmentSlot.TwoHanded))
                {
                    bool addedToHotbar = hotbarController.AddItemToHotbar(equippableItem);
                    if (addedToHotbar)
                    {
                        // Find the slot where the item was added and select it
                        int slotIndex = hotbarController.FindItemInHotbar(equippableItem);
                        if (slotIndex >= 0)
                        {
                            hotbarController.SelectHotbarSlot(slotIndex);
                        }
                        
                        if (debugMode)
                        {
                            Debug.Log($"Auto-added {item.GetDisplayName()} to hotbar slot {slotIndex}");
                        }
                    }
                }
            }
            
            return equipped;
        }
        
        // Slot-based inventory management methods
        public ItemStack GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
                return null;
            return inventorySlots[slotIndex];
        }
        
        public bool SetItemAtSlot(int slotIndex, ItemDefinition item, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
                return false;
                
            if (item == null || quantity <= 0)
            {
                inventorySlots[slotIndex] = null;
                SyncSlotsToItemsList();
                return true;
            }
            
            inventorySlots[slotIndex] = new ItemStack(item, quantity);
            SyncSlotsToItemsList();
            return true;
        }
        
        public bool SwapSlots(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= inventorySlots.Length ||
                toSlot < 0 || toSlot >= inventorySlots.Length)
                return false;
                
            var temp = inventorySlots[fromSlot];
            inventorySlots[fromSlot] = inventorySlots[toSlot];
            inventorySlots[toSlot] = temp;
            
            SyncSlotsToItemsList();
            return true;
        }
        
        public bool MoveItemToSlot(int fromSlot, int toSlot, int quantity = -1)
        {
            if (fromSlot < 0 || fromSlot >= inventorySlots.Length ||
                toSlot < 0 || toSlot >= inventorySlots.Length)
                return false;
                
            var fromStack = inventorySlots[fromSlot];
            if (fromStack == null || fromStack.quantity <= 0)
                return false;
                
            var toStack = inventorySlots[toSlot];
            
            // If quantity is -1, move entire stack
            if (quantity == -1)
                quantity = fromStack.quantity;
                
            quantity = Mathf.Min(quantity, fromStack.quantity);
            
            if (toStack == null)
            {
                // Move to empty slot
                inventorySlots[toSlot] = new ItemStack(fromStack.item, quantity);
                fromStack.quantity -= quantity;
                
                if (fromStack.quantity <= 0)
                    inventorySlots[fromSlot] = null;
            }
            else if (toStack.item == fromStack.item && fromStack.item.CanStack())
            {
                // Combine stacks
                int availableSpace = fromStack.item.maxStackSize - toStack.quantity;
                int amountToMove = Mathf.Min(quantity, availableSpace);
                
                if (amountToMove <= 0)
                    return false; // Can't move any
                    
                toStack.quantity += amountToMove;
                fromStack.quantity -= amountToMove;
                
                if (fromStack.quantity <= 0)
                    inventorySlots[fromSlot] = null;
            }
            else
            {
                // Can't combine - would need to swap instead
                return false;
            }
            
            SyncSlotsToItemsList();
            return true;
        }
        
        public int GetTotalSlots()
        {
            return inventorySlots.Length;
        }
        
        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] == null)
                    return i;
            }
            return -1;
        }
        
        private void SyncSlotsToItemsList()
        {
            items.Clear();
            foreach (var slot in inventorySlots)
            {
                if (slot != null && slot.quantity > 0)
                {
                    items.Add(slot);
                }
            }
        }
        
        private void SyncItemsListToSlots()
        {
            // Clear slots
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i] = null;
            }
            
            // Place items back into slots
            for (int i = 0; i < items.Count && i < inventorySlots.Length; i++)
            {
                if (items[i] != null && items[i].quantity > 0)
                {
                    inventorySlots[i] = items[i];
                }
            }
        }
    }
}