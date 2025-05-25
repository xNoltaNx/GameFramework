using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Items
{
    [System.Serializable]
    public class EquippedItem
    {
        public EquippableItemDefinition item;
        public GameObject instantiatedPrefab;
        public string slotName;
        
        public EquippedItem(EquippableItemDefinition item, GameObject instantiatedPrefab, string slotName)
        {
            this.item = item;
            this.instantiatedPrefab = instantiatedPrefab;
            this.slotName = slotName;
        }
    }

    public class EquipmentController : MonoBehaviour, IEquipmentController
    {
        [Header("Equipment Settings")]
        [SerializeField] private bool autoFindAttachmentPoints = true;
        
        [Header("Default Attachment Points")]
        [SerializeField] private Transform mainHandAttachment;
        [SerializeField] private Transform offHandAttachment;
        [SerializeField] private Transform twoHandedAttachment;
        [SerializeField] private Transform headAttachment;
        [SerializeField] private Transform chestAttachment;
        [SerializeField] private Transform backAttachment;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private Dictionary<string, EquippedItem> equippedItems = new Dictionary<string, EquippedItem>();
        private Dictionary<string, Transform> attachmentPoints = new Dictionary<string, Transform>();
        
        public IReadOnlyDictionary<string, EquippedItem> EquippedItems => equippedItems;
        
        private void Awake()
        {
            InitializeAttachmentPoints();
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D sound
                }
            }
        }
        
        private void InitializeAttachmentPoints()
        {
            // Add default attachment points
            if (mainHandAttachment != null)
                SetAttachmentPoint("MainHand", mainHandAttachment);
            if (offHandAttachment != null)
                SetAttachmentPoint("OffHand", offHandAttachment);
            if (twoHandedAttachment != null)
                SetAttachmentPoint("TwoHanded", twoHandedAttachment);
            if (headAttachment != null)
                SetAttachmentPoint("Head", headAttachment);
            if (chestAttachment != null)
                SetAttachmentPoint("Chest", chestAttachment);
            if (backAttachment != null)
                SetAttachmentPoint("Back", backAttachment);
            
            if (autoFindAttachmentPoints)
            {
                AutoFindAttachmentPoints();
            }
        }
        
        private void AutoFindAttachmentPoints()
        {
            // Try to find common attachment point names in hierarchy
            Transform[] allTransforms = GetComponentsInChildren<Transform>();
            
            foreach (Transform t in allTransforms)
            {
                string name = t.name.ToLower();
                
                // Common attachment point naming conventions
                if (name.Contains("hand_r") || name.Contains("righthand"))
                    SetAttachmentPoint("MainHand", t);
                else if (name.Contains("hand_l") || name.Contains("lefthand"))
                    SetAttachmentPoint("OffHand", t);
                else if (name.Contains("head") || name.Contains("skull"))
                    SetAttachmentPoint("Head", t);
                else if (name.Contains("spine") && name.Contains("03"))
                    SetAttachmentPoint("Chest", t);
                else if (name.Contains("back") || name.Contains("spine01"))
                    SetAttachmentPoint("Back", t);
            }
        }
        
        public bool CanEquipItem(EquippableItemDefinition item, string slotName = null)
        {
            if (item == null) return false;
            
            // If no slot specified, use the item's primary slot
            if (string.IsNullOrEmpty(slotName))
            {
                slotName = item.equipmentSlot.ToString();
            }
            
            // Check if we have an attachment point for this slot
            if (!attachmentPoints.ContainsKey(slotName))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"No attachment point found for slot: {slotName}");
                }
                return false;
            }
            
            // Check if item can be equipped to this slot
            if (System.Enum.TryParse<EquipmentSlot>(slotName, out EquipmentSlot slot))
            {
                if (!item.CanEquipToSlot(slot))
                {
                    return false;
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Invalid equipment slot name: {slotName}");
                }
                return false;
            }
            
            // Check if slot is already occupied
            if (equippedItems.ContainsKey(slotName))
            {
                if (debugMode)
                {
                    Debug.Log($"Slot {slotName} is already occupied");
                }
                return false;
            }
            
            return true;
        }
        
        public bool EquipItem(EquippableItemDefinition item, string slotName = null)
        {
            if (!CanEquipItem(item, slotName)) return false;
            
            // If no slot specified, use the item's primary slot
            if (string.IsNullOrEmpty(slotName))
            {
                slotName = item.equipmentSlot.ToString();
            }
            
            if (debugMode)
            {
                Debug.Log($"Equipping {item.GetDisplayName()} to {slotName}");
            }
            
            // Unequip any existing item in this slot
            UnequipItem(slotName);
            
            // Instantiate the equipment prefab
            GameObject equippedObject = null;
            if (item.equipmentPrefab != null)
            {
                Transform attachPoint = attachmentPoints[slotName];
                equippedObject = Instantiate(item.equipmentPrefab, attachPoint);
                
                // Apply attachment offset and rotation
                equippedObject.transform.localPosition = item.attachmentOffset;
                equippedObject.transform.localRotation = Quaternion.Euler(item.attachmentRotation);
                equippedObject.transform.localScale = item.attachmentScale;
            }
            
            // Create equipped item entry
            EquippedItem equippedItem = new EquippedItem(item, equippedObject, slotName);
            equippedItems[slotName] = equippedItem;
            
            // Play equip sound
            if (item.equipSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(item.equipSound);
            }
            
            return true;
        }
        
        public bool UnequipItem(string slotName)
        {
            if (!equippedItems.ContainsKey(slotName)) return false;
            
            EquippedItem equippedItem = equippedItems[slotName];
            
            if (debugMode)
            {
                Debug.Log($"Unequipping {equippedItem.item.GetDisplayName()} from {slotName}");
            }
            
            // Destroy the instantiated prefab
            if (equippedItem.instantiatedPrefab != null)
            {
                Destroy(equippedItem.instantiatedPrefab);
            }
            
            // Play unequip sound
            if (equippedItem.item.unequipSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(equippedItem.item.unequipSound);
            }
            
            // Remove from equipped items
            equippedItems.Remove(slotName);
            
            return true;
        }
        
        public EquippedItem GetEquippedItem(string slotName)
        {
            equippedItems.TryGetValue(slotName, out EquippedItem item);
            return item;
        }
        
        public void SetAttachmentPoint(string slotName, Transform attachmentPoint)
        {
            if (attachmentPoint != null)
            {
                attachmentPoints[slotName] = attachmentPoint;
                
                if (debugMode)
                {
                    Debug.Log($"Set attachment point for {slotName}: {attachmentPoint.name}");
                }
            }
        }
        
        public Transform GetAttachmentPoint(string slotName)
        {
            attachmentPoints.TryGetValue(slotName, out Transform point);
            return point;
        }
        
        public void UnequipAll()
        {
            List<string> slotsToUnequip = new List<string>(equippedItems.Keys);
            foreach (string slot in slotsToUnequip)
            {
                UnequipItem(slot);
            }
        }
        
        public bool IsSlotOccupied(string slotName)
        {
            return equippedItems.ContainsKey(slotName);
        }
        
        // Debug method to display equipped items
        [ContextMenu("Debug Equipment")]
        public void DebugEquipment()
        {
            Debug.Log($"=== Equipped Items ({equippedItems.Count}) ===");
            foreach (var kvp in equippedItems)
            {
                Debug.Log($"- {kvp.Key}: {kvp.Value.item.GetDisplayName()}");
            }
            
            Debug.Log($"=== Attachment Points ({attachmentPoints.Count}) ===");
            foreach (var kvp in attachmentPoints)
            {
                Debug.Log($"- {kvp.Key}: {kvp.Value.name}");
            }
        }
    }
}