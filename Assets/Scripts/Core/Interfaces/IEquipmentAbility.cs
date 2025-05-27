using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IEquipmentAbility
    {
        void OnEquipped(GameObject equipper);
        void OnUnequipped(GameObject equipper);
        bool IsActive { get; }
    }
}