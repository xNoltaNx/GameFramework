using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using GameFramework.Items.Abilities;
using GameFramework.Locomotion;
using GameFramework.Input;

namespace GameFramework.Items.Examples
{
    /// <summary>
    /// Runtime example demonstrating how to set up and use the equipment ability system.
    /// Attach this to your player GameObject to see the system in action.
    /// </summary>
    public class ExampleSetup : MonoBehaviour
    {
        [Header("Example Items")]
        [SerializeField] private EquippableItemDefinition doubleJumpBoots;
        [SerializeField] private EquippableItemDefinition testGloves;
        
        [Header("Debug")]
        [SerializeField] private bool autoEquipOnStart = true;
        [SerializeField] private bool showDebugUI = true;
        
        private EquipmentController equipmentController;
        private InventoryController inventoryController;
        private DoubleJumpAbility doubleJumpAbility;
        
        // Example: How to verify proper setup
        private void Start()
        {
            ValidateSetup();
            
            if (autoEquipOnStart && doubleJumpBoots != null)
            {
                EquipExampleItem();
            }
        }
        
        private void ValidateSetup()
        {
            // Check for required components
            var locomotion = GetComponent<FirstPersonLocomotionController>();
            var inputHandler = GetComponent<PlayerInputHandler>();
            equipmentController = GetComponent<EquipmentController>();
            inventoryController = GetComponent<InventoryController>();
            
            Debug.Log("=== Equipment Ability System Validation ===");
            Debug.Log($"✓ FirstPersonLocomotionController: {(locomotion != null ? "Found" : "MISSING")}");
            Debug.Log($"✓ PlayerInputHandler: {(inputHandler != null ? "Found" : "MISSING")}");
            Debug.Log($"✓ EquipmentController: {(equipmentController != null ? "Found" : "MISSING")}");
            Debug.Log($"✓ InventoryController: {(inventoryController != null ? "Found" : "MISSING")}");
            
            // Check for audio source (optional but recommended)
            var audioSource = GetComponent<AudioSource>();
            Debug.Log($"✓ AudioSource: {(audioSource != null ? "Found" : "Not found (abilities will work without sound)")}");
            
            Debug.Log("=== Validation Complete ===");
        }
        
        [ContextMenu("Equip Double Jump Boots")]
        private void EquipExampleItem()
        {
            if (doubleJumpBoots == null)
            {
                Debug.LogError("No double jump boots assigned! Drag the DoubleJumpBoots asset to this component.");
                return;
            }
            
            if (equipmentController == null)
            {
                Debug.LogError("No EquipmentController found on this GameObject!");
                return;
            }
            
            // Add to inventory first (if inventory system is present)
            if (inventoryController != null)
            {
                inventoryController.AddItem(doubleJumpBoots, 1);
                Debug.Log("Added double jump boots to inventory");
            }
            
            // Equip the boots
            bool equipped = equipmentController.EquipItem(doubleJumpBoots, "Feet");
            if (equipped)
            {
                Debug.Log("✓ Double jump boots equipped successfully!");
                
                // Find the ability component that was just added
                doubleJumpAbility = GetComponent<DoubleJumpAbility>();
                if (doubleJumpAbility != null)
                {
                    Debug.Log("✓ DoubleJumpAbility component found and active!");
                }
            }
            else
            {
                Debug.LogError("✗ Failed to equip double jump boots!");
            }
        }
        
        [ContextMenu("Unequip Boots")]
        private void UnequipBoots()
        {
            if (equipmentController != null)
            {
                bool unequipped = equipmentController.UnequipItem("Feet");
                Debug.Log(unequipped ? "✓ Boots unequipped" : "✗ No boots to unequip");
            }
        }
        
        [ContextMenu("Test Double Jump")]
        private void TestDoubleJump()
        {
            var ability = GetComponent<DoubleJumpAbility>();
            if (ability == null)
            {
                Debug.Log("No DoubleJumpAbility found - make sure boots are equipped!");
                return;
            }
            
            if (!ability.IsActive)
            {
                Debug.Log("DoubleJumpAbility is not active!");
                return;
            }
            
            if (ability.CanDoubleJump)
            {
                bool success = ability.TryDoubleJump();
                Debug.Log(success ? "✓ Double jump performed!" : "✗ Double jump failed!");
            }
            else
            {
                Debug.Log($"Cannot double jump - IsGrounded: {GetComponent<FirstPersonLocomotionController>()?.IsGrounded}, HasDoubleJumped: {ability.HasDoubleJumped}");
            }
        }
        
        [ContextMenu("Debug Equipment")]
        private void DebugEquipment()
        {
            if (equipmentController != null)
            {
                equipmentController.DebugEquipment();
            }
        }
        
        // Example: How to check abilities at runtime
        private void Update()
        {
            if (!showDebugUI) return;
            
            // Example: Show ability status
            var ability = GetComponent<DoubleJumpAbility>();
            if (ability != null && ability.IsActive)
            {
                // You could update UI here showing double jump availability
                bool canDoubleJump = ability.CanDoubleJump;
                // Update UI indicator based on canDoubleJump
            }
        }
        
        // Example: How to listen for equipment changes
        private void OnEnable()
        {
            // You could subscribe to equipment events here if they existed
            // For now, we poll in Update or use the component directly
        }
        
        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Equipment Ability System Debug");
            GUILayout.Space(5);
            
            var ability = GetComponent<DoubleJumpAbility>();
            if (ability != null)
            {
                GUILayout.Label($"Double Jump Active: {ability.IsActive}");
                GUILayout.Label($"Can Double Jump: {ability.CanDoubleJump}");
                GUILayout.Label($"Has Double Jumped: {ability.HasDoubleJumped}");
                
                var locomotion = GetComponent<FirstPersonLocomotionController>();
                if (locomotion != null)
                {
                    GUILayout.Label($"Is Grounded: {locomotion.IsGrounded}");
                }
            }
            else
            {
                GUILayout.Label("No Double Jump Ability (boots not equipped)");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Equip Boots"))
            {
                EquipExampleItem();
            }
            
            if (GUILayout.Button("Unequip Boots"))
            {
                UnequipBoots();
            }
            
            if (ability != null && GUILayout.Button("Test Double Jump"))
            {
                TestDoubleJump();
            }
            
            GUILayout.EndArea();
        }
    }
}