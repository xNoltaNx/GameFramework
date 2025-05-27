using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Items;
using GameFramework.Locomotion;
using GameFramework.Input;

namespace GameFramework.Items.Abilities
{
    /// <summary>
    /// Simple test script to verify the equipment ability system is working correctly.
    /// Add this to any GameObject to run basic integration tests.
    /// </summary>
    public class EquipmentAbilityIntegrationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private EquippableItemDefinition testItem;
        
        private EquipmentController equipmentController;
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunIntegrationTests());
            }
        }
        
        private System.Collections.IEnumerator RunIntegrationTests()
        {
            Debug.Log("=== Equipment Ability System Integration Tests ===");
            
            yield return new WaitForSeconds(1f); // Let everything initialize
            
            // Test 1: Find required components
            yield return StartCoroutine(TestComponentDiscovery());
            
            // Test 2: Test ability template system
            yield return StartCoroutine(TestAbilityTemplates());
            
            // Test 3: Test equipment/unequip cycle
            if (testItem != null)
            {
                yield return StartCoroutine(TestEquipmentCycle());
            }
            
            Debug.Log("=== Integration Tests Complete ===");
        }
        
        private System.Collections.IEnumerator TestComponentDiscovery()
        {
            Debug.Log("Test 1: Component Discovery");
            
            equipmentController = FindObjectOfType<EquipmentController>();
            var locomotionController = FindObjectOfType<FirstPersonLocomotionController>();
            var inputHandler = FindObjectOfType<PlayerInputHandler>();
            
            bool passed = true;
            
            if (equipmentController == null)
            {
                Debug.LogError("✗ No EquipmentController found in scene");
                passed = false;
            }
            else
            {
                Debug.Log("✓ EquipmentController found");
            }
            
            if (locomotionController == null)
            {
                Debug.LogError("✗ No FirstPersonLocomotionController found in scene");
                passed = false;
            }
            else
            {
                Debug.Log("✓ FirstPersonLocomotionController found");
            }
            
            if (inputHandler == null)
            {
                Debug.LogError("✗ No PlayerInputHandler found in scene");
                passed = false;
            }
            else
            {
                Debug.Log("✓ PlayerInputHandler found");
            }
            
            Debug.Log(passed ? "✓ Component Discovery: PASSED" : "✗ Component Discovery: FAILED");
            yield return null;
        }
        
        private System.Collections.IEnumerator TestAbilityTemplates()
        {
            Debug.Log("Test 2: Ability Template System");
            
            // Find all ability templates in the project
            var templates = Resources.FindObjectsOfTypeAll<DoubleJumpAbilityTemplate>();
            
            if (templates.Length == 0)
            {
                Debug.LogWarning("✗ No DoubleJumpAbilityTemplates found. Create one for testing.");
            }
            else
            {
                Debug.Log($"✓ Found {templates.Length} DoubleJumpAbilityTemplate(s)");
                
                foreach (var template in templates)
                {
                    Debug.Log($"  - Template: {template.name}");
                    Debug.Log($"    Height Multiplier: {template.doubleJumpHeightMultiplier}");
                    Debug.Log($"    Wall Reset: {template.resetDoubleJumpOnWallContact}");
                    Debug.Log($"    IsActive: {template.IsActive} (should be false for templates)");
                }
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator TestEquipmentCycle()
        {
            Debug.Log("Test 3: Equipment Cycle");
            
            if (equipmentController == null)
            {
                Debug.LogError("✗ No EquipmentController for equipment test");
                yield break;
            }
            
            // Test equipping
            Debug.Log($"Attempting to equip: {testItem.itemName}");
            bool equipped = equipmentController.EquipItem(testItem);
            
            if (!equipped)
            {
                Debug.LogError("✗ Failed to equip test item");
                yield break;
            }
            
            Debug.Log("✓ Item equipped successfully");
            
            // Wait a frame for components to be added
            yield return null;
            
            // Check if ability components were added
            var abilities = equipmentController.GetComponents<IEquipmentAbility>();
            Debug.Log($"Found {abilities.Length} ability component(s) after equipping");
            
            foreach (var ability in abilities)
            {
                if (ability.IsActive)
                {
                    Debug.Log($"✓ Active ability: {ability.GetType().Name}");
                }
            }
            
            // Test unequipping
            yield return new WaitForSeconds(1f);
            
            string slotName = testItem.equipmentSlot.ToString();
            bool unequipped = equipmentController.UnequipItem(slotName);
            
            if (!unequipped)
            {
                Debug.LogError($"✗ Failed to unequip from slot: {slotName}");
                yield break;
            }
            
            Debug.Log("✓ Item unequipped successfully");
            
            // Wait a frame for components to be removed
            yield return null;
            
            // Check if ability components were removed
            var remainingAbilities = equipmentController.GetComponents<IEquipmentAbility>();
            int activeAbilities = 0;
            foreach (var ability in remainingAbilities)
            {
                if (ability.IsActive) activeAbilities++;
            }
            
            if (activeAbilities == 0)
            {
                Debug.Log("✓ All abilities properly removed");
            }
            else
            {
                Debug.LogWarning($"✗ {activeAbilities} abilities still active after unequipping");
            }
            
            Debug.Log("✓ Equipment Cycle: COMPLETED");
        }
        
        [ContextMenu("Run Integration Tests")]
        public void RunTests()
        {
            StartCoroutine(RunIntegrationTests());
        }
        
        [ContextMenu("Quick Component Check")]
        public void QuickComponentCheck()
        {
            var equipment = FindObjectOfType<EquipmentController>();
            if (equipment != null)
            {
                Debug.Log($"Found EquipmentController on: {equipment.gameObject.name}");
                equipment.DebugEquipment();
                
                // Check for abilities
                var abilities = equipment.GetComponents<IEquipmentAbility>();
                Debug.Log($"Current abilities: {abilities.Length}");
                foreach (var ability in abilities)
                {
                    Debug.Log($"  - {ability.GetType().Name}: Active={ability.IsActive}");
                }
            }
            else
            {
                Debug.LogError("No EquipmentController found in scene!");
            }
        }
    }
}