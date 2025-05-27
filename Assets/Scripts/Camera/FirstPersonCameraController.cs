using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Camera
{
    public class FirstPersonCameraController : MonoBehaviour, ICameraController
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalClampAngle = 80f;
        [SerializeField] private float fieldOfView = 75f;
        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private bool debugInput = false;
        [SerializeField] private bool debugCameraEffects = false;
        
        [Header("References")]
        [SerializeField] private UnityEngine.Camera playerCamera;
        [SerializeField] private CameraEffectController effectController;
        
        private Transform target;
        private float verticalRotation;
        private float horizontalRotation;
        private bool hasBeenInitialized = false;
        
        
        // Movement tracking for effects
        private Vector2 lastMovementInput;
        private bool isCurrentlyMoving;
        private bool isCurrentlySprinting;
        private float currentMovementSpeed;

        public UnityEngine.Camera Camera => playerCamera;
        public Transform CameraTransform => playerCamera.transform;

        private void Awake()
        {
            ValidateReferences();
            InitializeCamera();
        }

        private void ValidateReferences()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<UnityEngine.Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError($"FirstPersonCameraController on {gameObject.name} requires a Camera component!");
                }
            }
            
            if (effectController == null)
            {
                effectController = GetComponent<CameraEffectController>();
                if (effectController == null)
                {
                    Debug.LogWarning($"FirstPersonCameraController on {gameObject.name} has no CameraEffectController - camera effects will be disabled.");
                }
            }
        }

        private void InitializeCamera()
        {
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void Initialize(Transform cameraTarget)
        {
            SetTarget(cameraTarget);
            
            if (target != null)
            {
                horizontalRotation = target.eulerAngles.y;
                
                // Only reset vertical rotation on first initialization
                if (!hasBeenInitialized)
                {
                    verticalRotation = 0f;
                    hasBeenInitialized = true;
                }
                // Otherwise preserve current vertical rotation
            }
        }

        public void HandleLookInput(Vector2 lookInput)
        {
            if (target == null || playerCamera == null) return;

            if (debugInput && lookInput != Vector2.zero)
            {
                Debug.Log($"Look Input: {lookInput}, Target: {target.name}, MouseSensitivity: {mouseSensitivity}");
            }

            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            if (invertYAxis)
                mouseY = -mouseY;

            horizontalRotation += mouseX;
            verticalRotation -= mouseY;

            verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

            target.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            if (debugInput && lookInput != Vector2.zero)
            {
                Debug.Log($"Applied Rotation - Horizontal: {horizontalRotation}, Vertical: {verticalRotation}");
            }
        }

        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        public void SetFieldOfView(float fov)
        {
            fieldOfView = fov;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }

        public void SetTarget(Transform cameraTarget)
        {
            target = cameraTarget;
        }

        public void SetVerticalClamp(float clampAngle)
        {
            verticalClampAngle = clampAngle;
        }

        public void SetInvertYAxis(bool invert)
        {
            invertYAxis = invert;
        }

        private void OnValidate()
        {
            if (playerCamera != null && Application.isPlaying)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }

        // ICameraController interface methods for camera effects
        public void NotifyLocomotionStateChanged(string stateName, bool isMoving, bool isSprinting, float movementSpeed)
        {
            if (effectController == null) return;

            if (debugCameraEffects)
            {
                Debug.Log($"[CameraController] State: {stateName}, Moving: {isMoving}, Sprinting: {isSprinting}, Speed: {movementSpeed:F2}");
            }

            isCurrentlyMoving = isMoving;
            isCurrentlySprinting = isSprinting;
            currentMovementSpeed = movementSpeed;

            switch (stateName.ToLower())
            {
                case "standing":
                    if (isMoving)
                    {
                        if (isSprinting)
                            effectController.SetSprintingEffects();
                        else
                            effectController.SetWalkingEffects();
                    }
                    else
                    {
                        effectController.SetStandingEffects();
                    }
                    break;
                case "crouching":
                    effectController.SetCrouchingEffects();
                    break;
                case "sliding":
                    effectController.SetSlidingEffects();
                    break;
                case "jumping":
                case "falling":
                case "airborne":
                    effectController.SetAirborneEffects();
                    break;
            }
        }

        public void NotifyLanding(float landingVelocity)
        {
            if (effectController == null) return;
            
            // Trigger landing shake based on impact velocity
            if (Mathf.Abs(landingVelocity) > 5f)
            {
                effectController.TriggerLandingShake();
            }
        }

        public void NotifyMovementInput(Vector2 movementInput)
        {
            lastMovementInput = movementInput;
            
            // Update camera effects with current movement data
            if (effectController != null)
            {
                effectController.UpdateMovementData(currentMovementSpeed, movementInput, isCurrentlyMoving, isCurrentlySprinting);
            }
        }
        
        private void Update()
        {
            // Continuously update movement data for smooth camera effects
            if (effectController != null)
            {
                effectController.UpdateMovementData(currentMovementSpeed, lastMovementInput, isCurrentlyMoving, isCurrentlySprinting);
            }
        }
    }
}