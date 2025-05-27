using UnityEngine;
using UnityEngine.InputSystem;
using GameFramework.Core.Interfaces;
using GameFramework.UI;
using GameFramework.Items.Abilities;

namespace GameFramework.Input
{
    public class PlayerInputHandler : MonoBehaviour, IInputHandler
    {
        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        
        [Header("UI References")]
        [SerializeField] private InventoryUI inventoryUI;
        
        private InputSystem_Actions inputActions;
        private Vector2 movementInput;
        private Vector2 lookInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool attackPressed;
        private bool interactPressed;
        private bool inventoryTogglePressed;
        private Vector2 scrollInput;

        public Vector2 MovementInput => movementInput;
        public Vector2 LookInput => lookInput;
        public bool JumpPressed => jumpPressed;
        public bool JumpHeld => jumpHeld;
        public bool SprintHeld => sprintHeld;
        public bool CrouchHeld => crouchHeld;
        public bool AttackPressed => attackPressed;
        public bool InteractPressed => interactPressed;
        public bool InventoryTogglePressed => inventoryTogglePressed;
        public Vector2 ScrollInput => scrollInput;

        private void Awake()
        {
            InitializeInputActions();
        }

        private void InitializeInputActions()
        {
            inputActions = new InputSystem_Actions();
            
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
            
            inputActions.Player.Look.performed += OnLookPerformed;
            inputActions.Player.Look.canceled += OnLookCanceled;
            
            inputActions.Player.Jump.performed += OnJumpPerformed;
            inputActions.Player.Jump.canceled += OnJumpCanceled;
            
            inputActions.Player.Sprint.performed += OnSprintPerformed;
            inputActions.Player.Sprint.canceled += OnSprintCanceled;
            
            inputActions.Player.Crouch.performed += OnCrouchPerformed;
            inputActions.Player.Crouch.canceled += OnCrouchCanceled;
            
            inputActions.Player.Attack.performed += OnAttackPerformed;
            inputActions.Player.Interact.performed += OnInteractPerformed;
            
            inputActions.Player.ToggleInventory.performed += OnToggleInventoryPerformed;
            inputActions.Player.Hotbar1.performed += OnHotbar1Performed;
            inputActions.Player.Hotbar2.performed += OnHotbar2Performed;
            inputActions.Player.Hotbar3.performed += OnHotbar3Performed;
            inputActions.Player.Hotbar4.performed += OnHotbar4Performed;
            inputActions.Player.Hotbar5.performed += OnHotbar5Performed;
            
            // Use UI scroll wheel input for equipment cycling
            inputActions.UI.ScrollWheel.performed += OnScrollWheelPerformed;
            inputActions.UI.ScrollWheel.canceled += OnScrollWheelCanceled;
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }

        private void LateUpdate()
        {
            ClearFrameBasedInputs();
        }

        private void ClearFrameBasedInputs()
        {
            jumpPressed = false;
            attackPressed = false;
            interactPressed = false;
            inventoryTogglePressed = false;
            scrollInput = Vector2.zero;
        }

        public void EnableInput()
        {
            inputActions?.Player.Enable();
            inputActions?.UI.Enable();
        }

        public void DisableInput()
        {
            inputActions?.Player.Disable();
            inputActions?.UI.Disable();
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            lookInput = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpPressed = true;
            jumpHeld = true;
            
            // Notify any double jump abilities
            NotifyDoubleJumpAbilities(true);
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            jumpHeld = false;
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            sprintHeld = true;
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            sprintHeld = false;
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            crouchHeld = true;
        }

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            crouchHeld = false;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            attackPressed = true;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Interact input detected!");
            interactPressed = true;
        }
        
        private void OnToggleInventoryPerformed(InputAction.CallbackContext context)
        {
            inventoryTogglePressed = true;
            if (inventoryUI != null)
            {
                inventoryUI.ToggleInventory();
            }
        }
        
        private void OnHotbar1Performed(InputAction.CallbackContext context)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SelectHotbarSlot(0);
            }
        }
        
        private void OnHotbar2Performed(InputAction.CallbackContext context)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SelectHotbarSlot(1);
            }
        }
        
        private void OnHotbar3Performed(InputAction.CallbackContext context)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SelectHotbarSlot(2);
            }
        }
        
        private void OnHotbar4Performed(InputAction.CallbackContext context)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SelectHotbarSlot(3);
            }
        }
        
        private void OnHotbar5Performed(InputAction.CallbackContext context)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SelectHotbarSlot(4);
            }
        }
        
        private void OnScrollWheelPerformed(InputAction.CallbackContext context)
        {
            scrollInput = context.ReadValue<Vector2>();
            
            // Handle equipment cycling when inventory UI is available
            if (inventoryUI != null && scrollInput.y != 0)
            {
                inventoryUI.CycleEquippedItems(scrollInput.y > 0);
            }
        }
        
        private void OnScrollWheelCanceled(InputAction.CallbackContext context)
        {
            scrollInput = Vector2.zero;
        }
        
        private void NotifyDoubleJumpAbilities(bool jumpPressed)
        {
            // Find all double jump abilities and notify them of jump input
            DoubleJumpAbility[] doubleJumpAbilities = GetComponents<DoubleJumpAbility>();
            foreach (var ability in doubleJumpAbilities)
            {
                if (ability.IsActive)
                {
                    ability.HandleJumpInput(jumpPressed);
                }
            }
        }
    }
}