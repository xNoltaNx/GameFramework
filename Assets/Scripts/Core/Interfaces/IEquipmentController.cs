using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IEquipmentController
    {
        bool CanEquipItem(object item, string slotName = null);
        bool EquipItem(object item, string slotName = null);
        bool UnequipItem(string slotName);
        object GetEquippedItem(string slotName);
        
        void SetAttachmentPoint(string slotName, Transform attachmentPoint);
        Transform GetAttachmentPoint(string slotName);
    }
}