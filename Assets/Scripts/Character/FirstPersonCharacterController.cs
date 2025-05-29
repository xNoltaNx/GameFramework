using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Input;
using GameFramework.Camera;
using GameFramework.Locomotion;
using GameFramework.Items;

namespace GameFramework.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("Component References")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private FirstPersonCameraController cameraController;
        [SerializeField] private FirstPersonLocomotionController locomotionController;
        
        [Header("Item System Components")]
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private MonoBehaviour interactionControllerComponent;
        
        [Header("UI References")]
        [SerializeField] private GameFramework.UI.InventoryUI inventoryUI;
        
        private IInputHandler input;
        private ICameraController camera;
        private ILocomotionController locomotion;
        private IInventoryController inventory;
        private IEquipmentController equipment;
        private IInteractionController interaction;

        private void Awake()
        {
            ValidateAndGetComponents();
            InitializeInterfaces();
        }

        private void Start()
        {
            InitializeComponents();
        }

        private void Update()
        {
            HandleInput();
        }

        private void ValidateAndGetComponents()
        {
            if (inputHandler == null)
            {
                inputHandler = GetComponent<PlayerInputHandler>();
                if (inputHandler == null)
                {
                    inputHandler = gameObject.AddComponent<PlayerInputHandler>();
                }
            }

            if (cameraController == null)
            {
                cameraController = GetComponentInChildren<FirstPersonCameraController>();
                if (cameraController == null)
                {
                    Debug.LogError($"FirstPersonCharacterController on {gameObject.name} requires a FirstPersonCameraController component!");
                }
            }

            if (locomotionController == null)
            {
                locomotionController = GetComponent<FirstPersonLocomotionController>();
                if (locomotionController == null)
                {
                    locomotionController = gameObject.AddComponent<FirstPersonLocomotionController>();
                }
            }
            
            // Item System Components
            if (inventoryController == null)
            {
                inventoryController = GetComponent<InventoryController>();
                if (inventoryController == null)
                {
                    inventoryController = gameObject.AddComponent<InventoryController>();
                }
            }
            
            if (equipmentController == null)
            {
                equipmentController = GetComponent<EquipmentController>();
                if (equipmentController == null)
                {
                    equipmentController = gameObject.AddComponent<EquipmentController>();
                }
            }
            
            if (interaction == null)
            {
                interaction = GetComponent<IInteractionController>();
                if (interaction == null && interactionControllerComponent != null)
                {
                    interaction = interactionControllerComponent as IInteractionController;
                }
            }
            
            // UI References
            if (inventoryUI == null)
            {
                inventoryUI = FindObjectOfType<GameFramework.UI.InventoryUI>();
                if (inventoryUI == null)
                {
                    Debug.LogWarning($"FirstPersonCharacterController on {gameObject.name} could not find InventoryUI - camera control during inventory will not work properly.");
                }
            }
        }

        private void InitializeInterfaces()
        {
            input = inputHandler;
            camera = cameraController;
            locomotion = locomotionController;
            inventory = inventoryController;
            equipment = equipmentController;
            // interaction already set in ValidateComponents
        }

        private void InitializeComponents()
        {
            CharacterController characterController = GetComponent<CharacterController>();
            
            if (camera != null)
            {
                camera.Initialize(transform);
            }

            if (locomotion != null && characterController != null)
            {
                Transform cameraTransform = camera?.CameraTransform ?? transform;
                locomotion.Initialize(characterController, cameraTransform);
            }
        }

        private void HandleInput()
        {
            if (input == null || camera == null || locomotion == null) return;

            // Check if inventory is open to disable camera and movement
            bool isInventoryOpen = inventoryUI != null && inventoryUI.IsInventoryOpen;
            
            if (!isInventoryOpen)
            {
                // Only process camera and movement input if inventory is closed
                camera.HandleLookInput(input.LookInput);
                locomotion.HandleMovement(input.MovementInput, input.SprintHeld, input.CrouchHeld);
                locomotion.HandleJump(input.JumpPressed, input.JumpHeld);
            }
            
            // Handle interactions (should work even with inventory open)
            if (interaction != null && input.InteractPressed)
            {
                Debug.Log("Interact pressed - calling HandleInteraction()");
                interaction.HandleInteraction();
            }
        }

        public void SetInputHandler(IInputHandler newInputHandler)
        {
            input = newInputHandler;
        }

        public void SetCameraController(ICameraController newCameraController)
        {
            camera = newCameraController;
            if (camera != null)
            {
                camera.Initialize(transform);
            }
        }

        public void SetLocomotionController(ILocomotionController newLocomotionController)
        {
            locomotion = newLocomotionController;
            if (locomotion != null)
            {
                CharacterController characterController = GetComponent<CharacterController>();
                Transform cameraTransform = camera?.CameraTransform ?? transform;
                locomotion.Initialize(characterController, cameraTransform);
            }
        }

        // Item System Setters
        public void SetInventoryController(IInventoryController newInventoryController)
        {
            inventory = newInventoryController;
        }
        
        public void SetEquipmentController(IEquipmentController newEquipmentController)
        {
            equipment = newEquipmentController;
        }
        
        public void SetInteractionController(IInteractionController newInteractionController)
        {
            interaction = newInteractionController;
        }

        // Getters
        public IInputHandler GetInputHandler() => input;
        public ICameraController GetCameraController() => camera;
        public ILocomotionController GetLocomotionController() => locomotion;
        public IInventoryController GetInventoryController() => inventory;
        public IEquipmentController GetEquipmentController() => equipment;
        public IInteractionController GetInteractionController() => interaction;

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();
            
            if (cameraController == null)
                cameraController = GetComponentInChildren<FirstPersonCameraController>();
            
            if (locomotionController == null)
                locomotionController = GetComponent<FirstPersonLocomotionController>();
                
            // Item System Components
            if (inventoryController == null)
                inventoryController = GetComponent<InventoryController>();
                
            if (equipmentController == null)
                equipmentController = GetComponent<EquipmentController>();
                
            if (interaction == null)
                interaction = GetComponent<IInteractionController>();
        }
    }
}