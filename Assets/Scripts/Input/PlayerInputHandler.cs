using UnityEngine;
using UnityEngine.InputSystem;
using GameFramework.Core.Interfaces;
using System;

namespace GameFramework.Input
{
    public class PlayerInputHandler : MonoBehaviour, IInputHandler
    {
        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        
        // Events for UI interaction
        public event Action OnInventoryToggle;
        public event Action<bool> OnEquipmentCycle; // bool = scrollUp
        public event Action<bool> OnJumpAbilityInput; // bool = jumpPressed
        public event Action<int> OnHotbarSlotSelected; // int = slot index
        
        [SerializeField] private InputActionAsset inputActionAsset;
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;
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
            if (inputActionAsset == null)
            {
                Debug.LogError("InputActionAsset is not assigned in PlayerInputHandler!");
                return;
            }

            playerActionMap = inputActionAsset.FindActionMap("Player");
            uiActionMap = inputActionAsset.FindActionMap("UI");
            
            if (playerActionMap == null || uiActionMap == null)
            {
                Debug.LogError("Required action maps not found in InputActionAsset!");
                return;
            }
            
            playerActionMap.FindAction("Move").performed += OnMovePerformed;
            playerActionMap.FindAction("Move").canceled += OnMoveCanceled;
            
            playerActionMap.FindAction("Look").performed += OnLookPerformed;
            playerActionMap.FindAction("Look").canceled += OnLookCanceled;
            
            playerActionMap.FindAction("Jump").performed += OnJumpPerformed;
            playerActionMap.FindAction("Jump").canceled += OnJumpCanceled;
            
            playerActionMap.FindAction("Sprint").performed += OnSprintPerformed;
            playerActionMap.FindAction("Sprint").canceled += OnSprintCanceled;
            
            playerActionMap.FindAction("Crouch").performed += OnCrouchPerformed;
            playerActionMap.FindAction("Crouch").canceled += OnCrouchCanceled;
            
            playerActionMap.FindAction("Attack").performed += OnAttackPerformed;
            playerActionMap.FindAction("Interact").performed += OnInteractPerformed;
            
            playerActionMap.FindAction("ToggleInventory").performed += OnToggleInventoryPerformed;
            playerActionMap.FindAction("Hotbar1").performed += OnHotbar1Performed;
            playerActionMap.FindAction("Hotbar2").performed += OnHotbar2Performed;
            playerActionMap.FindAction("Hotbar3").performed += OnHotbar3Performed;
            playerActionMap.FindAction("Hotbar4").performed += OnHotbar4Performed;
            playerActionMap.FindAction("Hotbar5").performed += OnHotbar5Performed;
            
            // Use UI scroll wheel input for equipment cycling
            uiActionMap.FindAction("ScrollWheel").performed += OnScrollWheelPerformed;
            uiActionMap.FindAction("ScrollWheel").canceled += OnScrollWheelCanceled;
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
            // No need to dispose InputActionAsset as it's a ScriptableObject
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
            playerActionMap?.Enable();
            uiActionMap?.Enable();
        }

        public void DisableInput()
        {
            playerActionMap?.Disable();
            uiActionMap?.Disable();
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
            
            // Notify jump abilities via event
            OnJumpAbilityInput?.Invoke(true);
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
            OnInventoryToggle?.Invoke();
        }
        
        private void OnHotbar1Performed(InputAction.CallbackContext context)
        {
            OnHotbarSlotSelected?.Invoke(0);
        }
        
        private void OnHotbar2Performed(InputAction.CallbackContext context)
        {
            OnHotbarSlotSelected?.Invoke(1);
        }
        
        private void OnHotbar3Performed(InputAction.CallbackContext context)
        {
            OnHotbarSlotSelected?.Invoke(2);
        }
        
        private void OnHotbar4Performed(InputAction.CallbackContext context)
        {
            OnHotbarSlotSelected?.Invoke(3);
        }
        
        private void OnHotbar5Performed(InputAction.CallbackContext context)
        {
            OnHotbarSlotSelected?.Invoke(4);
        }
        
        private void OnScrollWheelPerformed(InputAction.CallbackContext context)
        {
            scrollInput = context.ReadValue<Vector2>();
            
            // Handle equipment cycling via event
            if (scrollInput.y != 0)
            {
                OnEquipmentCycle?.Invoke(scrollInput.y > 0);
            }
        }
        
        private void OnScrollWheelCanceled(InputAction.CallbackContext context)
        {
            scrollInput = Vector2.zero;
        }
        
    }
}