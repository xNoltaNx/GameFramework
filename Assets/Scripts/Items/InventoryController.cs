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
        [SerializeField] private int capacity = 20;
        [SerializeField] private bool unlimitedCapacity = false;
        
        [Header("Auto-Equip Settings")]
        [SerializeField] private bool autoEquipOnPickup = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private List<ItemStack> items = new List<ItemStack>();
        private IEquipmentController equipmentController;
        
        public IReadOnlyList<ItemStack> Items => items;
        public int Capacity => unlimitedCapacity ? int.MaxValue : capacity;
        
        private void Awake()
        {
            if (items == null)
            {
                items = new List<ItemStack>();
            }
            
            // Find equipment controller on the same GameObject or parent
            equipmentController = GetComponent<IEquipmentController>();
            if (equipmentController == null)
            {
                equipmentController = GetComponentInParent<IEquipmentController>();
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
            
            // Check for auto-equip before adding to inventory
            bool itemWasAutoEquipped = TryAutoEquipItem(item);
            
            // If item was auto-equipped and it's not stackable, don't add to inventory
            if (itemWasAutoEquipped && !item.CanStack())
            {
                quantity--; // Reduce quantity by 1 for the equipped item
                if (quantity <= 0)
                    return true; // All items were equipped, nothing to add to inventory
            }
            
            // Add remaining items to inventory
            if (quantity > 0)
            {
                if (item.CanStack())
                {
                    AddStackableItem(item, quantity);
                }
                else
                {
                    AddNonStackableItem(item, quantity);
                }
            }
            
            return true;
        }
        
        private void AddStackableItem(ItemDefinition item, int quantity)
        {
            while (quantity > 0)
            {
                ItemStack existingStack = FindItemStack(item);
                
                if (existingStack != null)
                {
                    // Add to existing stack
                    int availableSpace = item.maxStackSize - existingStack.quantity;
                    int amountToAdd = Mathf.Min(quantity, availableSpace);
                    
                    existingStack.quantity += amountToAdd;
                    quantity -= amountToAdd;
                }
                else
                {
                    // Create new stack
                    int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                    items.Add(new ItemStack(item, amountToAdd));
                    quantity -= amountToAdd;
                }
            }
        }
        
        private void AddNonStackableItem(ItemDefinition item, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                items.Add(new ItemStack(item, 1));
            }
        }
        
        public bool RemoveItem(ItemDefinition item, int quantity = 1)
        {
            if (!HasItem(item, quantity)) return false;
            
            if (debugMode)
            {
                Debug.Log($"Removing {quantity}x {item.GetDisplayName()} from inventory");
            }
            
            int remainingToRemove = quantity;
            
            for (int i = items.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                ItemStack stack = items[i];
                if (stack.item == item)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, stack.quantity);
                    stack.quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;
                    
                    if (stack.quantity <= 0)
                    {
                        items.RemoveAt(i);
                    }
                }
            }
            
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
            
            if (equipped && debugMode)
            {
                Debug.Log($"Auto-equipped {item.GetDisplayName()} to {slotName} slot");
            }
            
            return equipped;
        }
    }
}