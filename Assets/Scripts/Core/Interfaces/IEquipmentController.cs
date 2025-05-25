using System.Collections.Generic;
using UnityEngine;
using GameFramework.Items;

namespace GameFramework.Core.Interfaces
{
    public interface IEquipmentController
    {
        IReadOnlyDictionary<string, GameFramework.Items.EquippedItem> EquippedItems { get; }
        
        bool CanEquipItem(GameFramework.Items.EquippableItemDefinition item, string slotName = null);
        bool EquipItem(GameFramework.Items.EquippableItemDefinition item, string slotName = null);
        bool UnequipItem(string slotName);
        GameFramework.Items.EquippedItem GetEquippedItem(string slotName);
        
        void SetAttachmentPoint(string slotName, Transform attachmentPoint);
        Transform GetAttachmentPoint(string slotName);
    }
}