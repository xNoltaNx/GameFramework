using UnityEngine;
using System.Collections.Generic;

namespace GameFramework.Items
{
    [CreateAssetMenu(fileName = "New Equippable Item", menuName = "GameFramework/Items/Equippable Item")]
    public class EquippableItemDefinition : ItemDefinition
    {
        [Header("Equipment Properties")]
        public EquipmentSlot equipmentSlot = EquipmentSlot.MainHand;
        public GameObject equipmentPrefab;
        
        [Header("Attachment")]
        public Vector3 attachmentOffset = Vector3.zero;
        public Vector3 attachmentRotation = Vector3.zero;
        public Vector3 attachmentScale = Vector3.one;
        
        [Header("Alternative Slots")]
        public List<EquipmentSlot> alternativeSlots = new List<EquipmentSlot>();
        
        [Header("Equipment Audio")]
        public AudioClip equipSound;
        public AudioClip unequipSound;
        
        public override string GetDisplayName()
        {
            return $"{itemName} ({equipmentSlot})";
        }
        
        public override bool CanStack()
        {
            return false; // Equipment items typically don't stack
        }
        
        public bool CanEquipToSlot(EquipmentSlot slot)
        {
            return equipmentSlot == slot || alternativeSlots.Contains(slot);
        }
    }
    
    public enum EquipmentSlot
    {
        MainHand,
        OffHand,
        TwoHanded,
        Head,
        Chest,
        Legs,
        Feet,
        Back,
        Ring,
        Necklace
    }
}