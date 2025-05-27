/*
 * EQUIPMENT ABILITY SYSTEM - SETUP EXAMPLE
 * ==========================================
 * 
 * This file demonstrates how to set up the equipment ability system.
 * Follow these steps to create items that grant abilities to players.
 * 
 * STEP 1: CREATE AN ABILITY TEMPLATE
 * ===================================
 * 
 * Right-click in Project window → Create → GameFramework → Equipment Abilities → Double Jump
 * This creates a ScriptableObject template that defines the ability settings.
 * 
 * Configure the template:
 * - doubleJumpHeightMultiplier: 0.8 (80% of normal jump height)
 * - resetDoubleJumpOnWallContact: true (allows wall-kick mechanics)
 * - doubleJumpSound: Assign an AudioClip for the double jump sound
 * 
 * STEP 2: CREATE AN EQUIPABLE ITEM
 * =================================
 * 
 * Right-click in Project window → Create → GameFramework → Items → Equippable Item
 * 
 * Configure the item:
 * - itemName: "Double Jump Boots"
 * - description: "Mystical boots that grant the ability to jump twice in mid-air"
 * - equipmentSlot: Feet
 * - abilityTemplates: Drag your DoubleJumpAbilityTemplate here
 * 
 * STEP 3: VERIFY PLAYER SETUP
 * ============================
 * 
 * Your player GameObject needs these components:
 * - FirstPersonLocomotionController
 * - EquipmentController 
 * - PlayerInputHandler
 * - CharacterController
 * - AudioSource (optional, for ability sounds)
 * 
 * STEP 4: SETUP ATTACHMENT POINTS (Optional)
 * ==========================================
 * 
 * If you want visual equipment on the player:
 * 1. Create empty GameObjects as children of your player
 * 2. Position them at feet location
 * 3. Assign to EquipmentController's "Feet Attachment" field
 * 
 * STEP 5: TEST THE SYSTEM
 * =======================
 * 
 * 1. Add the boots item to inventory
 * 2. Equip to feet slot
 * 3. Jump once normally, then press jump again in mid-air
 * 4. You should perform a double jump!
 * 
 * CREATING NEW ABILITIES
 * ======================
 * 
 * To create new abilities (like dash, wall-climb, etc.):
 * 
 * 1. Create a new ability component class:
 *    - Inherit from MonoBehaviour
 *    - Implement IEquipmentAbility interface
 *    - Add your ability logic
 * 
 * 2. Create a template ScriptableObject:
 *    - Inherit from ScriptableObject
 *    - Implement IEquipmentAbility interface (return false for IsActive)
 *    - Add CreateAbilityComponent method
 * 
 * 3. Update EquipmentController.AddAbilityComponents():
 *    - Add a new case for your template type
 * 
 * EXAMPLE RUNTIME USAGE
 * =====================
 * 
 * // Check if player has double jump ability
 * DoubleJumpAbility doubleJump = player.GetComponent<DoubleJumpAbility>();
 * if (doubleJump != null && doubleJump.IsActive)
 * {
 *     Debug.Log($"Player can double jump! Has used: {doubleJump.HasDoubleJumped}");
 * }
 * 
 * // Manually trigger double jump (if needed)
 * if (doubleJump != null && doubleJump.CanDoubleJump)
 * {
 *     doubleJump.TryDoubleJump();
 * }
 */

using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Items.Abilities;

namespace GameFramework.Items.Examples
{
    // Example of how to create a custom ability template
    [CreateAssetMenu(fileName = "DashAbilityTemplate", menuName = "GameFramework/Equipment Abilities/Dash")]
    public class DashAbilityTemplate : ScriptableObject, IEquipmentAbility
    {
        [Header("Dash Settings")]
        public float dashDistance = 10f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 2f;
        public AudioClip dashSound;
        
        public bool IsActive => false; // Templates are never active
        
        public void OnEquipped(GameObject equipper)
        {
            Debug.LogWarning("OnEquipped called on template - this shouldn't happen!");
        }
        
        public void OnUnequipped(GameObject equipper)
        {
            Debug.LogWarning("OnUnequipped called on template - this shouldn't happen!");
        }
        
        public DashAbility CreateAbilityComponent(GameObject target)
        {
            DashAbility ability = target.AddComponent<DashAbility>();
            
            // Copy settings from template to component
            // You would implement this based on your ability's fields
            
            return ability;
        }
    }
    
    // Example of how to create a custom ability component
    public class DashAbility : MonoBehaviour, IEquipmentAbility
    {
        [SerializeField] private float dashDistance = 10f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private AudioClip dashSound;
        
        private float lastDashTime = 0f;
        private bool isDashing = false;
        
        public bool IsActive { get; private set; }
        public bool CanDash => !isDashing && (Time.time - lastDashTime) >= dashCooldown;
        
        public void OnEquipped(GameObject equipper)
        {
            IsActive = true;
            Debug.Log("Dash ability equipped!");
        }
        
        public void OnUnequipped(GameObject equipper)
        {
            IsActive = false;
            Debug.Log("Dash ability unequipped!");
        }
        
        public bool TryDash(Vector3 direction)
        {
            if (!CanDash) return false;
            
            // Implement dash logic here
            StartCoroutine(PerformDash(direction));
            return true;
        }
        
        private System.Collections.IEnumerator PerformDash(Vector3 direction)
        {
            isDashing = true;
            lastDashTime = Time.time;
            
            // Your dash implementation here
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + direction.normalized * dashDistance;
            
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / dashDuration;
                
                // Move player
                transform.position = Vector3.Lerp(startPos, targetPos, progress);
                
                yield return null;
            }
            
            isDashing = false;
        }
    }
}