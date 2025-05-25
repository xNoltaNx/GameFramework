using UnityEngine;

namespace GameFramework.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "GameFramework/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName = "New Item";
        [TextArea(3, 5)]
        public string description = "";
        public Sprite icon;
        
        [Header("Properties")]
        public bool isStackable = true;
        public int maxStackSize = 99;
        public float weight = 1f;
        public int value = 0;
        
        [Header("World Representation")]
        public GameObject worldPrefab;
        
        [Header("Audio")]
        public AudioClip pickupSound;
        public AudioClip dropSound;
        
        public virtual string GetDisplayName()
        {
            return itemName;
        }
        
        public virtual string GetDescription()
        {
            return description;
        }
        
        public virtual bool CanStack()
        {
            return isStackable;
        }
    }
}