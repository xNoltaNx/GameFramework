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
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private List<ItemStack> items = new List<ItemStack>();
        
        public IReadOnlyList<ItemStack> Items => items;
        public int Capacity => unlimitedCapacity ? int.MaxValue : capacity;
        
        private void Awake()
        {
            if (items == null)
            {
                items = new List<ItemStack>();
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
            
            if (item.CanStack())
            {
                AddStackableItem(item, quantity);
            }
            else
            {
                AddNonStackableItem(item, quantity);
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
    }
}