using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IInventoryController
    {
        int Capacity { get; }
        
        bool CanAddItem(object item, int quantity = 1);
        bool AddItem(object item, int quantity = 1);
        bool RemoveItem(object item, int quantity = 1);
        bool HasItem(object item, int quantity = 1);
        int GetItemCount(object item);
        System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, int>> GetAllItems();
        
        void Clear();
    }
}