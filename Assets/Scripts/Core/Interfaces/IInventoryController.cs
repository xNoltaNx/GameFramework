using System.Collections.Generic;
using UnityEngine;
using GameFramework.Items;

namespace GameFramework.Core.Interfaces
{
    public interface IInventoryController
    {
        IReadOnlyList<GameFramework.Items.ItemStack> Items { get; }
        int Capacity { get; }
        
        bool CanAddItem(GameFramework.Items.ItemDefinition item, int quantity = 1);
        bool AddItem(GameFramework.Items.ItemDefinition item, int quantity = 1);
        bool RemoveItem(GameFramework.Items.ItemDefinition item, int quantity = 1);
        bool HasItem(GameFramework.Items.ItemDefinition item, int quantity = 1);
        int GetItemCount(GameFramework.Items.ItemDefinition item);
        
        void Clear();
    }
}