using UnityEngine;
using UnityEngine.InputSystem;
using GameFramework.Core.Interfaces;

namespace GameFramework.Input
{
    public class PlayerInputHandler : MonoBehaviour, IInputHandler
    {
        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        
        private InputSystem_Actions inputActions;
        private Vector2 movementInput;
        private Vector2 lookInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool attackPressed;
        private bool interactPressed;

        public Vector2 MovementInput => movementInput;
        public Vector2 LookInput => lookInput;
        public bool JumpPressed => jumpPressed;
        public bool JumpHeld => jumpHeld;
        public bool SprintHeld => sprintHeld;
        public bool CrouchHeld => crouchHeld;
        public bool AttackPressed => attackPressed;
        public bool InteractPressed => interactPressed;

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
        }

        public void EnableInput()
        {
            inputActions?.Player.Enable();
        }

        public void DisableInput()
        {
            inputActions?.Player.Disable();
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
    }
}