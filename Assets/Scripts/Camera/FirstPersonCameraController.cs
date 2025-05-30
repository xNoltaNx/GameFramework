using UnityEngine;
using Unity.Cinemachine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Camera
{
    /// <summary>
    /// Enhanced first-person camera controller using Cinemachine for advanced camera behavior.
    /// Provides seamless integration between player locomotion and dynamic camera effects.
    /// </summary>
    public class FirstPersonCameraController : MonoBehaviour, ICameraController
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalClampAngle = 80f;
        [SerializeField] private float fieldOfView = 75f;
        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private bool debugInput = false;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 1.6f, 0); // Eye level height offset
        
        [Header("Cinemachine Integration")]
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineCameraManager cameraManager;
        [SerializeField] private CameraShakeManager shakeManager;
        [SerializeField] private Transform cameraRig;
        
        [Header("Auto-Setup")]
        [Tooltip("Automatically find and configure Cinemachine components")]
        [SerializeField] private bool autoSetupCinemachine = true;
        
        [Tooltip("Create camera rig if it doesn't exist")]
        [SerializeField] private bool autoCreateCameraRig = true;

        // Camera control state
        private Transform target;
        private float verticalRotation;
        private float horizontalRotation;
        private bool hasBeenInitialized = false;
        
        // Movement tracking for Cinemachine effects
        private Vector2 currentMovementInput = Vector2.zero;
        private float currentMovementSpeed = 0f;
        private bool isMoving = false;
        private bool isSprinting = false;
        private string currentMovementState = "standing";

        // Component references
        private UnityEngine.Camera playerCamera;
        private CinemachineCamera[] virtualCameras;

        #region Properties
        public UnityEngine.Camera Camera => playerCamera;
        public Transform CameraTransform => playerCamera?.transform;
        public CinemachineBrain Brain => cinemachineBrain;
        public CinemachineCameraManager CameraManager => cameraManager;
        public CameraShakeManager ShakeManager => shakeManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
            
            if (autoSetupCinemachine)
            {
                SetupCinemachineComponents();
            }
            
            ValidateReferences();
        }

        private void Start()
        {
            InitializeCamera();
            
            if (cameraManager != null)
            {
                SubscribeToManagerEvents();
            }
        }

        private void Update()
        {
            UpdateCinemachineExtensions();
        }

        private void OnDestroy()
        {
            UnsubscribeFromManagerEvents();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            // Find or create camera rig
            if (cameraRig == null && autoCreateCameraRig)
            {
                CreateCameraRig();
            }
            
            // Find main camera
            if (playerCamera == null)
            {
                playerCamera = UnityEngine.Camera.main;
                if (playerCamera == null)
                {
                    playerCamera = FindObjectOfType<UnityEngine.Camera>();
                }
            }
        }

        private void CreateCameraRig()
        {
            var rigGO = new GameObject("CameraRig");
            rigGO.transform.SetParent(transform);
            
            // Use the actual camera position if available, otherwise use cameraOffset
            if (playerCamera != null)
            {
                rigGO.transform.position = playerCamera.transform.position;
                rigGO.transform.rotation = playerCamera.transform.rotation;
                Debug.Log($"[FirstPersonCameraController] Created camera rig at camera position: {playerCamera.transform.position}");
            }
            else
            {
                rigGO.transform.localPosition = cameraOffset;
                rigGO.transform.localRotation = Quaternion.identity;
                Debug.Log($"[FirstPersonCameraController] Created camera rig at offset {cameraOffset}");
            }
            
            cameraRig = rigGO.transform;
        }

        private void SetupCinemachineComponents()
        {
            // Find Cinemachine Brain
            if (cinemachineBrain == null && playerCamera != null)
            {
                cinemachineBrain = playerCamera.GetComponent<CinemachineBrain>();
                if (cinemachineBrain == null)
                {
                    cinemachineBrain = playerCamera.gameObject.AddComponent<CinemachineBrain>();
                    Debug.Log("[FirstPersonCameraController] Added CinemachineBrain to camera");
                }
            }
            
            // Find or create camera manager
            if (cameraManager == null)
            {
                cameraManager = GetComponent<CinemachineCameraManager>();
                if (cameraManager == null)
                {
                    cameraManager = gameObject.AddComponent<CinemachineCameraManager>();
                    Debug.Log("[FirstPersonCameraController] Added CinemachineCameraManager");
                }
            }
            
            // Find or create shake manager
            if (shakeManager == null)
            {
                shakeManager = GetComponent<CameraShakeManager>();
                if (shakeManager == null)
                {
                    shakeManager = gameObject.AddComponent<CameraShakeManager>();
                    Debug.Log("[FirstPersonCameraController] Added CameraShakeManager");
                }
            }
        }

        private void ValidateReferences()
        {
            if (playerCamera == null)
            {
                Debug.LogError($"[FirstPersonCameraController] No camera found on {gameObject.name}!");
                return;
            }
            
            if (cinemachineBrain == null)
            {
                Debug.LogWarning($"[FirstPersonCameraController] No CinemachineBrain found - camera effects will be limited");
            }
            
            if (cameraManager == null)
            {
                Debug.LogWarning($"[FirstPersonCameraController] No CinemachineCameraManager found - advanced camera effects disabled");
            }
            
            if (cameraRig == null)
            {
                Debug.LogWarning($"[FirstPersonCameraController] No camera rig assigned - using transform as rig");
                cameraRig = transform;
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
            
            // Configure Cinemachine Brain
            if (cinemachineBrain != null)
            {
                cinemachineBrain.DefaultBlend.Time = 0.5f; // Smooth transitions
                cinemachineBrain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            }
        }

        private void SubscribeToManagerEvents()
        {
            if (cameraManager != null)
            {
                cameraManager.OnCameraStateChanged += OnCameraStateChanged;
            }
            
            if (shakeManager != null)
            {
                shakeManager.OnShakeTriggered += OnShakeTriggered;
            }
        }

        private void UnsubscribeFromManagerEvents()
        {
            if (cameraManager != null)
            {
                cameraManager.OnCameraStateChanged -= OnCameraStateChanged;
            }
            
            if (shakeManager != null)
            {
                shakeManager.OnShakeTriggered -= OnShakeTriggered;
            }
        }
        #endregion

        #region ICameraController Implementation
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
            }
            
            // Initialize camera manager with target
            if (cameraManager != null)
            {
                cameraManager.SetFollowTarget(cameraRig);
                cameraManager.SetLookAtTarget(cameraTarget);
            }
        }

        public void HandleLookInput(Vector2 lookInput)
        {
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            if (invertYAxis)
                mouseY = -mouseY;

            horizontalRotation += mouseX;
            verticalRotation -= mouseY;

            verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);

            // Apply rotation to target (horizontal) and camera rig (vertical)
            target.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
            cameraRig.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        }

        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        public void SetFieldOfView(float fov)
        {
            fieldOfView = fov;
            
            // Update base FOV - individual virtual cameras will override this as needed
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }

        public void SetTarget(Transform cameraTarget)
        {
            target = cameraTarget;
            
            // Update camera manager targets
            if (cameraManager != null)
            {
                cameraManager.SetLookAtTarget(cameraTarget);
            }
        }

        public void SetVerticalClamp(float clampAngle)
        {
            verticalClampAngle = clampAngle;
        }

        public void SetInvertYAxis(bool invert)
        {
            invertYAxis = invert;
        }
        #endregion

        #region Movement State Management
        public void NotifyLocomotionStateChanged(string stateName, bool moving, bool sprinting, float movementSpeed)
        {
            // Update internal state
            currentMovementState = stateName;
            isMoving = moving;
            isSprinting = sprinting;
            currentMovementSpeed = movementSpeed;
            
            // Forward to camera manager
            if (cameraManager != null)
            {
                cameraManager.NotifyMovementState(stateName, moving, sprinting, movementSpeed);
            }
            
            // Update Cinemachine extensions
            UpdateMovementDataForExtensions();
            
            if (debugInput)
            {
                Debug.Log($"[FirstPersonCameraController] State: {stateName}, Moving: {moving}, Speed: {movementSpeed:F2}");
            }
        }

        public void NotifyLanding(float landingVelocity)
        {
            // Forward to shake manager
            if (shakeManager != null)
            {
                shakeManager.TriggerLandingShake(landingVelocity);
            }
            
            if (debugInput)
            {
                Debug.Log($"[FirstPersonCameraController] Landing with velocity: {landingVelocity:F2}");
            }
        }

        public void NotifyMovementInput(Vector2 movementInput)
        {
            currentMovementInput = movementInput;
            
            // Forward to camera manager
            if (cameraManager != null)
            {
                cameraManager.UpdateMovementInput(movementInput);
            }
            
            // Update Cinemachine extensions
            UpdateMovementDataForExtensions();
        }
        #endregion

        #region Cinemachine Integration
        private void UpdateCinemachineExtensions()
        {
            UpdateMovementDataForExtensions();
        }

        private void UpdateMovementDataForExtensions()
        {
            // Update static data for custom Cinemachine extensions
            CinemachineMovementRoll.UpdateMovementData(currentMovementInput, currentMovementSpeed);
            
            // Determine movement state type
            var stateType = DetermineMovementStateType();
            CinemachineEnhancedNoise.UpdateMovementData(currentMovementInput, currentMovementSpeed, stateType);
        }

        private MovementStateType DetermineMovementStateType()
        {
            return currentMovementState.ToLower() switch
            {
                "standing" => isMoving ? (isSprinting ? MovementStateType.Sprinting : MovementStateType.Walking) : MovementStateType.Standing,
                "crouching" => MovementStateType.Crouching,
                "sliding" => MovementStateType.Sliding,
                "jumping" => MovementStateType.Airborne,
                "falling" => MovementStateType.Airborne,
                "airborne" => MovementStateType.Airborne,
                "mantle" => MovementStateType.Airborne, // Treat mantling as airborne for camera purposes
                _ => MovementStateType.Standing
            };
        }
        #endregion

        #region Event Handlers
        private void OnCameraStateChanged(MovementStateType newState)
        {
            if (debugInput)
            {
                Debug.Log($"[FirstPersonCameraController] Camera state changed to: {newState}");
            }
        }

        private void OnShakeTriggered(string presetName, float intensity)
        {
            if (debugInput)
            {
                Debug.Log($"[FirstPersonCameraController] Shake triggered: {presetName} (intensity: {intensity:F2})");
            }
        }
        #endregion

        #region Public API Extensions
        /// <summary>
        /// Trigger a custom camera shake effect
        /// </summary>
        public void TriggerCameraShake(string presetName, float intensity = 1f)
        {
            if (shakeManager != null)
            {
                shakeManager.TriggerShake(presetName, intensity);
            }
        }

        /// <summary>
        /// Trigger a custom camera shake with specific parameters
        /// </summary>
        public void TriggerCustomShake(Vector3 velocity, float duration = 0.5f)
        {
            if (shakeManager != null)
            {
                shakeManager.TriggerCustomShake(velocity, duration);
            }
        }

        /// <summary>
        /// Set the camera profile for dynamic behavior configuration
        /// </summary>
        public void SetCameraProfile(MovementStateCameraProfile profile)
        {
            if (cameraManager != null)
            {
                cameraManager.Profile = profile;
            }
        }

        /// <summary>
        /// Get the current active virtual camera
        /// </summary>
        public CinemachineCamera GetActiveVirtualCamera()
        {
            return cameraManager?.ActiveCamera;
        }

        /// <summary>
        /// Enable or disable all camera shake effects
        /// </summary>
        public void SetShakeEnabled(bool enabled)
        {
            if (shakeManager != null)
            {
                shakeManager.ShakeEnabled = enabled;
            }
        }

        /// <summary>
        /// Set global shake intensity (useful for accessibility)
        /// </summary>
        public void SetGlobalShakeIntensity(float intensity)
        {
            if (shakeManager != null)
            {
                shakeManager.GlobalShakeIntensity = intensity;
            }
        }
        #endregion

        #region Editor Utilities
        [ContextMenu("Setup Cinemachine Components")]
        private void EditorSetupCinemachine()
        {
            SetupCinemachineComponents();
            Debug.Log("[FirstPersonCameraController] Cinemachine components setup completed");
        }

        [ContextMenu("Test Camera Shake")]
        private void EditorTestShake()
        {
            if (Application.isPlaying)
            {
                TriggerCameraShake("Landing_Medium");
            }
        }

        private void OnValidate()
        {
            if (playerCamera != null && Application.isPlaying)
            {
                playerCamera.fieldOfView = fieldOfView;
            }
        }
        #endregion

        #region Gizmos and Debug
        private void OnDrawGizmosSelected()
        {
            if (cameraRig != null)
            {
                // Draw camera rig orientation
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(cameraRig.position, cameraRig.forward * 2f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawRay(cameraRig.position, cameraRig.right * 1f);
                
                Gizmos.color = Color.green;
                Gizmos.DrawRay(cameraRig.position, cameraRig.up * 1f);
            }
            
            if (target != null)
            {
                // Draw connection to target
                Gizmos.color = Color.yellow;
                if (cameraRig != null)
                {
                    Gizmos.DrawLine(cameraRig.position, target.position);
                }
                
                // Draw target orientation
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(target.position, target.forward * 1.5f);
            }
        }
        #endregion
    }
}