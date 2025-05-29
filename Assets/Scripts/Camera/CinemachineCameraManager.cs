using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System;

namespace GameFramework.Camera
{
    /// <summary>
    /// Advanced Cinemachine-based camera manager for dynamic movement-responsive camera behavior.
    /// Provides smooth transitions, performance optimization, and designer-friendly controls.
    /// </summary>
    public class CinemachineCameraManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MovementStateCameraProfile cameraProfile;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform lookAtTarget;
        
        [Header("Virtual Cameras")]
        [SerializeField] private CinemachineCamera standingCamera;
        [SerializeField] private CinemachineCamera walkingCamera;
        [SerializeField] private CinemachineCamera sprintingCamera;
        [SerializeField] private CinemachineCamera crouchingCamera;
        [SerializeField] private CinemachineCamera slidingCamera;
        [SerializeField] private CinemachineCamera airborneCamera;
        
        [Header("Camera Shake")]
        [SerializeField] private CinemachineImpulseSource landingShakeSource;
        [SerializeField] private CinemachineImpulseSource customShakeSource;
        
        [Header("Auto-Setup")]
        [Tooltip("Automatically configure virtual cameras on start")]
        [SerializeField] private bool autoConfigureCameras = true;
        
        [Tooltip("Create virtual cameras if they don't exist")]
        [SerializeField] private bool autoCreateCameras = true;
        
        [Tooltip("Parent virtual cameras under this transform (should be Brain camera or separate parent)")]
        [SerializeField] private Transform virtualCameraParent;
        
        [Header("Performance")]
        [Tooltip("Update frequency when far from player (performance mode)")]
        [SerializeField] private float distantUpdateFrequency = 0.1f;
        
        [SerializeField] private float performanceModeDistance = 50f;

        // Runtime state
        private MovementStateType currentState = MovementStateType.Standing;
        private MovementStateType previousState = MovementStateType.Standing;
        private CinemachineCamera activeCamera;
        private Dictionary<MovementStateType, CinemachineCamera> stateToCamera;
        private Dictionary<CinemachineCamera, CinemachineBasicMultiChannelPerlin> cameraNoiseComponents;
        
        // Movement tracking
        private Vector2 currentMovementInput = Vector2.zero;
        private float currentMovementSpeed = 0f;
        private bool isMoving = false;
        private bool isSprinting = false;
        
        // Performance optimization
        private float lastUpdateTime = 0f;
        private bool isInPerformanceMode = false;
        private UnityEngine.Camera mainCamera;
        
        // Roll effect state
        private float currentRoll = 0f;
        private float targetRoll = 0f;

        // Events
        public event Action<MovementStateType> OnCameraStateChanged;
        public event Action<float> OnCameraShake;

        #region Public Properties
        public MovementStateCameraProfile Profile 
        { 
            get => cameraProfile; 
            set => SetProfile(value);
        }
        
        public MovementStateType CurrentState => currentState;
        public CinemachineCamera ActiveCamera => activeCamera;
        public bool IsConfigured => stateToCamera != null && stateToCamera.Count > 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
            
            if (autoCreateCameras)
            {
                CreateMissingCameras();
            }
            
            if (autoConfigureCameras && cameraProfile != null)
            {
                ConfigureCameras();
            }
        }

        private void Start()
        {
            SetupCameraReferences();
            
            if (cameraProfile != null)
            {
                ApplyProfileSettings();
                SetCameraState(MovementStateType.Standing);
            }
            else
            {
                Debug.LogWarning($"[CinemachineCameraManager] No camera profile assigned to {gameObject.name}!");
            }
        }

        private void Update()
        {
            if (!IsConfigured) return;
            
            // Performance optimization
            if (ShouldSkipUpdate()) return;
            
            UpdateCameraEffects();
            UpdateCameraRoll();
        }

        private void LateUpdate()
        {
            // Update last frame for performance tracking
            lastUpdateTime = Time.time;
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            stateToCamera = new Dictionary<MovementStateType, CinemachineCamera>();
            cameraNoiseComponents = new Dictionary<CinemachineCamera, CinemachineBasicMultiChannelPerlin>();
            
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<UnityEngine.Camera>();
            }
        }

        private void CreateMissingCameras()
        {
            try
            {
                if (standingCamera == null) standingCamera = CreateVirtualCamera("vcam_Standing");
                if (walkingCamera == null) walkingCamera = CreateVirtualCamera("vcam_Walking");
                if (sprintingCamera == null) sprintingCamera = CreateVirtualCamera("vcam_Sprinting");
                if (crouchingCamera == null) crouchingCamera = CreateVirtualCamera("vcam_Crouching");
                if (slidingCamera == null) slidingCamera = CreateVirtualCamera("vcam_Sliding");
                if (airborneCamera == null) airborneCamera = CreateVirtualCamera("vcam_Airborne");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CinemachineCameraManager] Failed to create missing cameras: {e.Message}");
                throw;
            }
            
            // Create impulse sources if missing
            if (landingShakeSource == null)
            {
                var landingShakeGO = new GameObject("LandingShakeSource");
                landingShakeGO.transform.SetParent(transform);
                landingShakeSource = landingShakeGO.AddComponent<CinemachineImpulseSource>();
            }
            
            if (customShakeSource == null)
            {
                var customShakeGO = new GameObject("CustomShakeSource");
                customShakeGO.transform.SetParent(transform);
                customShakeSource = customShakeGO.AddComponent<CinemachineImpulseSource>();
            }
        }

        private CinemachineCamera CreateVirtualCamera(string cameraName)
        {
            var cameraGO = new GameObject(cameraName);
            
            // Use proper parenting - prefer virtualCameraParent, fallback to scene root
            if (virtualCameraParent != null)
            {
                cameraGO.transform.SetParent(virtualCameraParent);
            }
            else
            {
                // Don't parent to player - leave in scene root as Cinemachine recommends
                cameraGO.transform.SetParent(null);
                Debug.LogWarning($"[CinemachineCameraManager] No virtualCameraParent set. Creating {cameraName} in scene root.");
            }
            
            // Position virtual camera at the main camera's location
            if (mainCamera != null)
            {
                cameraGO.transform.position = mainCamera.transform.position;
                cameraGO.transform.rotation = mainCamera.transform.rotation;
                if (cameraProfile?.EnableDebugLogging == true)
                {
                    Debug.Log($"[CinemachineCameraManager] Positioned {cameraName} at camera location: {mainCamera.transform.position}");
                }
            }
            else
            {
                Debug.LogWarning($"[CinemachineCameraManager] No main camera found for positioning {cameraName}");
            }
            
            var vcam = cameraGO.AddComponent<CinemachineCamera>();
            vcam.Priority = 0; // Start with low priority
            
            // Add CinemachineRotateWithFollowTarget for first-person camera behavior
            // This makes the virtual camera inherit rotation from the follow target (camera rig)
            vcam.gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
            
            // Add noise component for head bob
            var noise = vcam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (cameraProfile?.EnableDebugLogging == true)
            {
                Debug.Log($"[CinemachineCameraManager] Created virtual camera: {cameraName}");
            }
            
            return vcam;
        }

        private void SetupCameraReferences()
        {
            if (stateToCamera == null)
            {
                Debug.LogError("[CinemachineCameraManager] stateToCamera dictionary is null! Call EnsureInitialized() first.");
                return;
            }

            stateToCamera[MovementStateType.Standing] = standingCamera;
            stateToCamera[MovementStateType.Walking] = walkingCamera;
            stateToCamera[MovementStateType.Sprinting] = sprintingCamera;
            stateToCamera[MovementStateType.Crouching] = crouchingCamera;
            stateToCamera[MovementStateType.Sliding] = slidingCamera;
            stateToCamera[MovementStateType.Airborne] = airborneCamera;
            
            // Cache noise components
            foreach (var kvp in stateToCamera)
            {
                if (kvp.Value != null)
                {
                    var noise = kvp.Value.GetComponent<CinemachineBasicMultiChannelPerlin>();
                    if (noise != null)
                    {
                        cameraNoiseComponents[kvp.Value] = noise;
                    }
                }
            }
        }
        #endregion

        #region Profile Management
        public void SetProfile(MovementStateCameraProfile newProfile)
        {
            if (newProfile == null)
            {
                Debug.LogWarning("[CinemachineCameraManager] Attempted to set null camera profile!");
                return;
            }
            
            cameraProfile = newProfile;
            
            if (IsConfigured)
            {
                ApplyProfileSettings();
            }
        }

        private void ApplyProfileSettings()
        {
            if (cameraProfile == null) return;
            
            ApplyStateSettings(MovementStateType.Standing, cameraProfile.StandingState);
            ApplyStateSettings(MovementStateType.Walking, cameraProfile.WalkingState);
            ApplyStateSettings(MovementStateType.Sprinting, cameraProfile.SprintingState);
            ApplyStateSettings(MovementStateType.Crouching, cameraProfile.CrouchingState);
            ApplyStateSettings(MovementStateType.Sliding, cameraProfile.SlidingState);
            ApplyStateSettings(MovementStateType.Airborne, cameraProfile.AirborneState);
            
            ConfigureShakeSources();
        }

        private void ApplyStateSettings(MovementStateType stateType, MovementCameraState stateConfig)
        {
            if (!stateToCamera.TryGetValue(stateType, out var vcam) || vcam == null) return;
            
            // Apply FOV
            vcam.Lens.FieldOfView = stateConfig.fieldOfView;
            
            // Apply follow target only - no LookAt for first-person cameras
            vcam.Follow = followTarget;
            vcam.LookAt = null; // First-person cameras should not have LookAt targets
            
            if (cameraProfile?.EnableDebugLogging == true)
            {
                Debug.Log($"[CinemachineCameraManager] {vcam.name} - Follow: {(followTarget?.name ?? "null")}, LookAt: {(lookAtTarget?.name ?? "null")}");
                if (followTarget != null)
                {
                    Debug.Log($"[CinemachineCameraManager] Follow target position: {followTarget.position}");
                }
            }
            
            // Configure noise for head bob
            if (cameraNoiseComponents.TryGetValue(vcam, out var noise))
            {
                ApplyNoiseSettings(noise, stateConfig.noiseSettings, stateConfig.enableHeadBob);
            }
            
            if (cameraProfile.EnableDebugLogging)
            {
                Debug.Log($"[CinemachineCameraManager] Applied settings for {stateType}: FOV={stateConfig.fieldOfView}, HeadBob={stateConfig.enableHeadBob}");
            }
        }

        private void ApplyNoiseSettings(CinemachineBasicMultiChannelPerlin noise, GameFrameworkNoiseSettings noiseSettings, bool enabled)
        {
            if (!enabled)
            {
                noise.AmplitudeGain = 0f;
                return;
            }
            
            noise.NoiseProfile = noiseSettings.noiseProfile;
            noise.AmplitudeGain = noiseSettings.amplitudeGain * cameraProfile.GlobalIntensity;
            noise.FrequencyGain = noiseSettings.frequencyGain;
        }

        private void ConfigureShakeSources()
        {
            if (landingShakeSource != null)
            {
                // Configure default impulse settings for landing
                landingShakeSource.ImpulseDefinition.TimeEnvelope.DecayTime = cameraProfile.ShakeProfile.decayTime;
                landingShakeSource.DefaultVelocity = Vector3.up * cameraProfile.ShakeProfile.landingIntensity;
            }
            
            if (customShakeSource != null)
            {
                customShakeSource.ImpulseDefinition.TimeEnvelope.DecayTime = cameraProfile.ShakeProfile.decayTime;
                customShakeSource.DefaultVelocity = Vector3.one * cameraProfile.ShakeProfile.defaultShakeIntensity;
            }
        }
        #endregion

        #region State Management
        public void SetCameraState(MovementStateType newState, bool forceUpdate = false)
        {
            if (currentState == newState && !forceUpdate) return;
            
            previousState = currentState;
            currentState = newState;
            
            // Update camera priorities
            UpdateCameraPriorities();
            
            // Update active camera reference
            if (stateToCamera.TryGetValue(currentState, out var newActiveCamera))
            {
                activeCamera = newActiveCamera;
            }
            
            // Invoke events
            OnCameraStateChanged?.Invoke(currentState);
            
            if (cameraProfile?.EnableDebugLogging == true)
            {
                Debug.Log($"[CinemachineCameraManager] Camera state changed: {previousState} -> {currentState}");
            }
        }

        private void UpdateCameraPriorities()
        {
            // Set all cameras to low priority
            foreach (var kvp in stateToCamera)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Priority = 0;
                }
            }
            
            // Set active camera to high priority
            if (stateToCamera.TryGetValue(currentState, out var activeVCam) && activeVCam != null)
            {
                activeVCam.Priority = 10;
            }
        }

        public void NotifyMovementState(string stateName, bool moving, bool sprinting, float movementSpeed)
        {
            isMoving = moving;
            isSprinting = sprinting;
            currentMovementSpeed = movementSpeed;
            
            // Determine camera state based on movement
            var newState = DetermineStateFromMovement(stateName, moving, sprinting);
            SetCameraState(newState);
        }

        private MovementStateType DetermineStateFromMovement(string stateName, bool moving, bool sprinting)
        {
            return stateName.ToLower() switch
            {
                "standing" => moving ? (sprinting ? MovementStateType.Sprinting : MovementStateType.Walking) : MovementStateType.Standing,
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

        #region Camera Effects
        private void UpdateCameraEffects()
        {
            if (activeCamera == null || cameraProfile == null) return;
            
            UpdateHeadBobIntensity();
            UpdatePerformanceMode();
        }

        private void UpdateHeadBobIntensity()
        {
            if (!cameraNoiseComponents.TryGetValue(activeCamera, out var noise)) return;
            
            var stateConfig = cameraProfile.GetStateConfiguration(currentState);
            if (!stateConfig.enableHeadBob) return;
            
            // Scale noise based on movement
            float intensityScale = 1f;
            if (stateConfig.noiseSettings.scaleWithMovement)
            {
                float normalizedSpeed = Mathf.Clamp01(currentMovementSpeed / 8f); // Assume max speed of 8
                intensityScale = Mathf.Lerp(
                    stateConfig.noiseSettings.minimumIntensity,
                    stateConfig.noiseSettings.maximumIntensity,
                    normalizedSpeed
                );
            }
            
            float finalAmplitude = stateConfig.noiseSettings.amplitudeGain * intensityScale * cameraProfile.GlobalIntensity;
            noise.AmplitudeGain = finalAmplitude;
        }

        private void UpdateCameraRoll()
        {
            var stateConfig = cameraProfile.GetStateConfiguration(currentState);
            if (!stateConfig.enableCameraRoll)
            {
                targetRoll = 0f;
            }
            else
            {
                // Calculate roll based on strafe input
                float strafeInput = currentMovementInput.x;
                float rollInfluence = Mathf.Clamp01(currentMovementSpeed / 5f); // Scale with movement
                targetRoll = strafeInput * stateConfig.maxRollAngle * rollInfluence;
                
                if (stateConfig.invertRoll)
                    targetRoll = -targetRoll;
            }
            
            // Smooth roll transition
            currentRoll = stateConfig.transitionEasing.Lerp(currentRoll, targetRoll, stateConfig.rollSpeed * Time.deltaTime);
            
            // Apply roll to active camera
            if (activeCamera != null && Mathf.Abs(currentRoll) > 0.01f)
            {
                var rotation = activeCamera.transform.localEulerAngles;
                rotation.z = currentRoll;
                activeCamera.transform.localEulerAngles = rotation;
            }
        }

        private void UpdatePerformanceMode()
        {
            if (!cameraProfile.EnablePerformanceMode) return;
            
            if (mainCamera != null && followTarget != null)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, followTarget.position);
                isInPerformanceMode = distance > performanceModeDistance;
            }
        }

        private bool ShouldSkipUpdate()
        {
            if (!isInPerformanceMode) return false;
            
            return Time.time - lastUpdateTime < distantUpdateFrequency;
        }
        #endregion

        #region Camera Shake
        public void TriggerLandingShake(float landingVelocity)
        {
            if (!cameraProfile.ShakeProfile.enableLandingShake) return;
            if (Mathf.Abs(landingVelocity) < cameraProfile.ShakeProfile.velocityThreshold) return;
            if (landingShakeSource == null) return;
            
            // Scale shake intensity based on velocity
            float velocityRatio = Mathf.Abs(landingVelocity) / 20f; // Normalize against expected max velocity
            float shakeIntensity = cameraProfile.ShakeProfile.landingIntensity * velocityRatio * cameraProfile.GlobalIntensity;
            
            landingShakeSource.DefaultVelocity = Vector3.up * shakeIntensity;
            landingShakeSource.GenerateImpulse();
            
            OnCameraShake?.Invoke(shakeIntensity);
            
            if (cameraProfile.EnableDebugLogging)
            {
                Debug.Log($"[CinemachineCameraManager] Landing shake triggered: velocity={landingVelocity:F1}, intensity={shakeIntensity:F2}");
            }
        }

        public void TriggerCustomShake(Vector3 direction, float intensity = 1f)
        {
            if (!cameraProfile.ShakeProfile.enableCustomShake) return;
            if (customShakeSource == null) return;
            
            float finalIntensity = intensity * cameraProfile.GlobalIntensity;
            customShakeSource.DefaultVelocity = direction * finalIntensity;
            customShakeSource.GenerateImpulse();
            
            OnCameraShake?.Invoke(finalIntensity);
        }

        public void TriggerCustomShake(float intensity = 1f)
        {
            TriggerCustomShake(Vector3.one, intensity);
        }
        #endregion

        #region Public Interface
        public void UpdateMovementInput(Vector2 movementInput)
        {
            currentMovementInput = movementInput;
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            
            // Update all cameras
            foreach (var vcam in stateToCamera.Values)
            {
                if (vcam != null)
                {
                    vcam.Follow = followTarget;
                }
            }
        }

        public void SetLookAtTarget(Transform target)
        {
            lookAtTarget = target;
            
            // For first-person cameras, we don't set LookAt targets as they should 
            // inherit rotation directly from the follow target (camera rig)
            // Keep the reference for potential third-person mode in the future
            Debug.Log($"[CinemachineCameraManager] LookAt target stored: {(target?.name ?? "null")} (not applied to first-person cameras)");
        }

        public void ConfigureCameras()
        {
            if (cameraProfile == null)
            {
                Debug.LogWarning("[CinemachineCameraManager] Cannot configure cameras - no profile assigned!");
                return;
            }
            
            // Ensure initialization is complete before configuring
            EnsureInitialized();
            
            // Create any missing virtual cameras
            if (autoCreateCameras)
            {
                CreateMissingCameras();
            }
            
            SetupCameraReferences();
            ApplyProfileSettings();
            
            Debug.Log("[CinemachineCameraManager] Cameras configured successfully!");
        }

        private void EnsureInitialized()
        {
            if (stateToCamera == null || cameraNoiseComponents == null)
            {
                InitializeComponents();
            }
        }
        #endregion

        #region Editor Utilities
        [ContextMenu("Auto-Configure Cameras")]
        private void EditorConfigureCameras()
        {
            if (autoCreateCameras)
            {
                CreateMissingCameras();
            }
            ConfigureCameras();
        }

        [ContextMenu("Test Camera Shake")]
        private void EditorTestShake()
        {
            TriggerCustomShake(1f);
        }

        private void OnValidate()
        {
            if (Application.isPlaying && IsConfigured && cameraProfile != null)
            {
                ApplyProfileSettings();
            }
        }
        #endregion
    }
}