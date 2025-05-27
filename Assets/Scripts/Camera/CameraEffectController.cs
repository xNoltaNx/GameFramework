using UnityEngine;
using GameFramework.Core;

namespace GameFramework.Camera
{
    [System.Serializable]
    public class HeadBobSettings
    {
        [Header("Vertical Bob")]
        public float verticalAmplitude = 0.1f;
        public float verticalFrequency = 2f;
        public EasingSettings verticalEasing = new EasingSettings(EasingType.EaseInOutSine);
        
        [Header("Horizontal Bob")]
        public float horizontalAmplitude = 0.05f;
        public float horizontalFrequency = 1f;
        public EasingSettings horizontalEasing = new EasingSettings(EasingType.EaseInOutSine);
        
        [Header("Timing")]
        public float speedThreshold = 0.1f;
        public float fadeInSpeed = 5f;
        public float fadeOutSpeed = 8f;
    }

    [System.Serializable]
    public class CameraRollSettings
    {
        public float maxRollAngle = 2f;
        public float rollSpeed = 3f;
        public EasingSettings rollEasing = new EasingSettings(EasingType.EaseOutQuad);
        public bool invertRoll = false;
    }

    [System.Serializable]
    public class FOVEffectSettings
    {
        public float baseFOV = 75f;
        public float targetFOV = 80f;
        public float transitionSpeed = 2f;
        public EasingSettings fovEasing = new EasingSettings(EasingType.EaseInOutQuad);
    }

    [System.Serializable]
    public class CameraShakeSettings
    {
        public float amplitude = 0.5f;
        public float frequency = 10f;
        public float duration = 0.2f;
        public EasingSettings shakeEasing = new EasingSettings(EasingType.EaseOutExpo);
    }

    [System.Serializable]
    public class StateCameraEffects
    {
        [Header("Head Bob")]
        public bool enableHeadBob = true;
        public HeadBobSettings headBob = new HeadBobSettings();
        
        [Header("Camera Roll")]
        public bool enableCameraRoll = false;
        public CameraRollSettings cameraRoll = new CameraRollSettings();
        
        [Header("Field of View")]
        public bool enableFOVEffect = false;
        public FOVEffectSettings fovEffect = new FOVEffectSettings();
        
        [Header("Camera Shake")]
        public bool enableLandingShake = false;
        public CameraShakeSettings landingShake = new CameraShakeSettings();
    }

    public class CameraEffectController : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private StateCameraEffects standingEffects;
        [SerializeField] private StateCameraEffects walkingEffects;
        [SerializeField] private StateCameraEffects sprintingEffects;
        [SerializeField] private StateCameraEffects crouchingEffects;
        [SerializeField] private StateCameraEffects slidingEffects;
        [SerializeField] private StateCameraEffects airborneEffects;
        
        [Header("Global Settings")]
        [SerializeField] private float globalIntensityMultiplier = 1f;
        [SerializeField] private bool enableEffects = true;
        [SerializeField] private CameraEffectSettings effectSettings;
        
        [Header("Debug")]
        [SerializeField] private bool debugCameraEffects = false;
        
        [Header("References")]
        [SerializeField] private FirstPersonCameraController cameraController;
        [SerializeField] private Transform cameraTransform;
        
        // Runtime state
        private StateCameraEffects currentEffects;
        private Vector3 originalLocalPosition;
        private Vector3 originalLocalRotation;
        private float originalFOV;
        
        // Head bob state
        private float bobTimer;
        private float bobIntensity;
        private Vector3 bobOffset;
        
        // Camera roll state
        private float currentRoll;
        private float targetRoll;
        
        // FOV state
        private float currentFOV;
        private float targetFOV;
        
        // Shake state
        private float shakeTimer;
        private float shakeDuration;
        private Vector3 shakeOffset;
        private CameraShakeSettings currentShake;

        public float GlobalIntensity
        {
            get => globalIntensityMultiplier;
            set => globalIntensityMultiplier = Mathf.Clamp01(value);
        }

        public bool EffectsEnabled
        {
            get => enableEffects;
            set => enableEffects = value;
        }

        public CameraEffectSettings Settings
        {
            get => effectSettings;
            set
            {
                effectSettings = value;
                ApplyUserSettings();
            }
        }

        private void Awake()
        {
            ValidateReferences();
            InitializeEffects();
        }
        
        private void Start()
        {
            // Set initial camera effects to standing state
            SetStandingEffects();
            
            // Ensure we have movement data initialized
            UpdateMovementData(0f, Vector2.zero, false, false);
        }

        private void ValidateReferences()
        {
            if (cameraController == null)
                cameraController = GetComponent<FirstPersonCameraController>();
            
            if (cameraTransform == null && cameraController != null)
                cameraTransform = cameraController.CameraTransform;
        }

        private void InitializeEffects()
        {
            // Capture the baseline position immediately - this should be the designer's intended position
            if (cameraTransform != null)
            {
                originalLocalPosition = cameraTransform.localPosition;
                originalLocalRotation = cameraTransform.localEulerAngles;
                
                if (debugCameraEffects)
                {
                    Debug.Log($"[CameraEffects] Captured baseline - Position: {originalLocalPosition}, Rotation: {originalLocalRotation}");
                }
            }
            
            if (cameraController != null && cameraController.Camera != null)
            {
                originalFOV = cameraController.Camera.fieldOfView;
                currentFOV = originalFOV;
                targetFOV = originalFOV;
            }
            
            // Initialize runtime state
            bobTimer = 0f;
            bobIntensity = 0f;
            bobOffset = Vector3.zero;
            currentRoll = 0f;
            targetRoll = 0f;
            shakeOffset = Vector3.zero;
            shakeTimer = 0f;
            
            // Initialize default effects
            InitializeDefaultEffects();
        }

        private void InitializeDefaultEffects()
        {
            // Standing effects - minimal head bob
            standingEffects = new StateCameraEffects
            {
                enableHeadBob = true,
                headBob = new HeadBobSettings
                {
                    verticalAmplitude = 0.02f,
                    verticalFrequency = 1.5f,
                    horizontalAmplitude = 0.01f,
                    horizontalFrequency = 0.75f
                }
            };

            // Walking effects - moderate head bob
            walkingEffects = new StateCameraEffects
            {
                enableHeadBob = true,
                headBob = new HeadBobSettings
                {
                    verticalAmplitude = 0.08f,
                    verticalFrequency = 2f,
                    horizontalAmplitude = 0.04f,
                    horizontalFrequency = 1f
                }
            };

            // Sprinting effects - increased head bob + FOV change
            sprintingEffects = new StateCameraEffects
            {
                enableHeadBob = true,
                enableFOVEffect = true,
                headBob = new HeadBobSettings
                {
                    verticalAmplitude = 0.12f,
                    verticalFrequency = 2.5f,
                    horizontalAmplitude = 0.06f,
                    horizontalFrequency = 1.25f
                },
                fovEffect = new FOVEffectSettings
                {
                    baseFOV = 75f,
                    targetFOV = 80f,
                    transitionSpeed = 3f
                }
            };

            // Crouching effects - subtle head bob
            crouchingEffects = new StateCameraEffects
            {
                enableHeadBob = true,
                headBob = new HeadBobSettings
                {
                    verticalAmplitude = 0.03f,
                    verticalFrequency = 1f,
                    horizontalAmplitude = 0.015f,
                    horizontalFrequency = 0.5f
                }
            };

            // Sliding effects - camera roll + reduced FOV
            slidingEffects = new StateCameraEffects
            {
                enableCameraRoll = true,
                enableFOVEffect = true,
                cameraRoll = new CameraRollSettings
                {
                    maxRollAngle = 5f,
                    rollSpeed = 4f
                },
                fovEffect = new FOVEffectSettings
                {
                    baseFOV = 75f,
                    targetFOV = 85f,
                    transitionSpeed = 5f
                }
            };

            // Airborne effects - minimal effects
            airborneEffects = new StateCameraEffects
            {
                enableLandingShake = true,
                landingShake = new CameraShakeSettings
                {
                    amplitude = 0.3f,
                    frequency = 15f,
                    duration = 0.15f
                }
            };
        }

        private void Update()
        {
            if (!enableEffects || cameraTransform == null)
            {
                // When effects are disabled, reset position and FOV but preserve rotation
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = originalLocalPosition;
                    // Don't reset rotation - let the camera controller handle it
                    if (cameraController?.Camera != null)
                        cameraController.Camera.fieldOfView = originalFOV;
                }
                return;
            }

            // Apply user settings if available
            float effectiveIntensity = globalIntensityMultiplier;
            if (effectSettings?.UserSettings != null)
            {
                effectiveIntensity *= effectSettings.UserSettings.masterIntensity;
            }

            UpdateEffects();
            ApplyEffects();
        }

        private void UpdateEffects()
        {
            if (currentEffects == null)
                return;

            UpdateHeadBob();
            UpdateCameraRoll();
            UpdateFOV();
            UpdateShake();
        }

        private void UpdateHeadBob()
        {
            bool userEnabledHeadBob = effectSettings?.UserSettings?.enableHeadBob ?? true;
            if (!currentEffects.enableHeadBob || !userEnabledHeadBob)
            {
                bobIntensity = Mathf.Lerp(bobIntensity, 0f, currentEffects.headBob.fadeOutSpeed * Time.deltaTime);
                bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * 8f);
                return;
            }

            // Get user settings multipliers
            float userHeadBobIntensity = effectSettings?.UserSettings?.headBobIntensity ?? 1f;
            float userHeadBobFrequency = effectSettings?.UserSettings?.headBobFrequency ?? 1f;

            // Get current movement data
            float movementSpeed = GetMovementSpeed();
            float normalizedSpeed = GetNormalizedMovementSpeed();
            bool isMoving = movementSpeed > currentEffects.headBob.speedThreshold;
            
            // Target intensity scales with movement speed
            float targetIntensity = isMoving ? normalizedSpeed : 0f;
            float lerpSpeed = isMoving ? currentEffects.headBob.fadeInSpeed : currentEffects.headBob.fadeOutSpeed;
            
            // Use faster fade-out when transitioning to standing effects or when not moving
            if (!isMoving || currentEffects == standingEffects)
            {
                lerpSpeed *= 2f; // Double the fade speed when stopping or in standing state
            }
            
            bobIntensity = Mathf.Lerp(bobIntensity, targetIntensity, lerpSpeed * Time.deltaTime);

            if (bobIntensity > 0.001f && isMoving)
            {
                // Timer advances based on actual movement speed for natural rhythm
                float speedMultiplier = Mathf.Max(normalizedSpeed, 0.1f); // Minimum speed to prevent stopping
                bobTimer += Time.deltaTime * speedMultiplier * userHeadBobFrequency;
                
                // Calculate bob components with speed-scaled frequency
                float effectiveVerticalFreq = currentEffects.headBob.verticalFrequency * userHeadBobFrequency;
                float effectiveHorizontalFreq = currentEffects.headBob.horizontalFrequency * userHeadBobFrequency;
                
                float verticalBob = CalculateBobComponent(bobTimer, effectiveVerticalFreq, currentEffects.headBob.verticalEasing);
                float horizontalBob = CalculateBobComponent(bobTimer * 0.5f, effectiveHorizontalFreq, currentEffects.headBob.horizontalEasing);
                
                // Amplitude also scales with movement speed and intensity
                float amplitudeScale = bobIntensity * globalIntensityMultiplier * userHeadBobIntensity;
                
                bobOffset = new Vector3(
                    horizontalBob * currentEffects.headBob.horizontalAmplitude * amplitudeScale,
                    verticalBob * currentEffects.headBob.verticalAmplitude * amplitudeScale,
                    0f
                );
                
                if (debugCameraEffects && Time.frameCount % 60 == 0) // Log every 60 frames
                {
                    Debug.Log($"[CameraEffects] Bob - Speed: {movementSpeed:F2}, Normalized: {normalizedSpeed:F2}, Intensity: {bobIntensity:F2}, Offset: {bobOffset}");
                }
            }
            else
            {
                bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * 8f);
            }
        }

        private float CalculateBobComponent(float timer, float frequency, EasingSettings easing)
        {
            float sinValue = Mathf.Sin(timer * frequency * Mathf.PI);
            float normalizedValue = (sinValue + 1f) * 0.5f; // Convert from [-1, 1] to [0, 1]
            return easing.Evaluate(normalizedValue) * 2f - 1f; // Convert back to [-1, 1] with easing applied
        }

        private void UpdateCameraRoll()
        {
            bool userEnabledRoll = effectSettings?.UserSettings?.enableCameraRoll ?? true;
            if (!currentEffects.enableCameraRoll || !userEnabledRoll)
            {
                targetRoll = 0f;
            }
            else
            {
                float userRollIntensity = effectSettings?.UserSettings?.cameraRollIntensity ?? 1f;
                float strafeInput = GetStrafeInput();
                float movementSpeed = GetNormalizedMovementSpeed();
                
                // Scale roll by both strafe input and movement speed
                targetRoll = strafeInput * currentEffects.cameraRoll.maxRollAngle * movementSpeed * userRollIntensity;
                if (currentEffects.cameraRoll.invertRoll)
                    targetRoll = -targetRoll;
            }

            currentRoll = currentEffects.cameraRoll.rollEasing.Lerp(
                currentRoll, 
                targetRoll, 
                currentEffects.cameraRoll.rollSpeed * Time.deltaTime
            );
        }

        private void UpdateFOV()
        {
            if (!currentEffects.enableFOVEffect || cameraController?.Camera == null)
            {
                targetFOV = originalFOV;
            }
            else
            {
                targetFOV = currentEffects.fovEffect.targetFOV;
            }

            currentFOV = currentEffects.enableFOVEffect ? 
                currentEffects.fovEffect.fovEasing.Lerp(currentFOV, targetFOV, currentEffects.fovEffect.transitionSpeed * Time.deltaTime) :
                Mathf.Lerp(currentFOV, targetFOV, 5f * Time.deltaTime);
            
            cameraController.Camera.fieldOfView = currentFOV;
        }

        private void UpdateShake()
        {
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float shakeProgress = 1f - (shakeTimer / shakeDuration);
                float shakeIntensity = currentShake.shakeEasing.Evaluate(1f - shakeProgress);
                
                float shakeX = Mathf.Sin(Time.time * currentShake.frequency) * currentShake.amplitude * shakeIntensity;
                float shakeY = Mathf.Cos(Time.time * currentShake.frequency * 1.1f) * currentShake.amplitude * shakeIntensity;
                
                shakeOffset = new Vector3(shakeX, shakeY, 0f) * globalIntensityMultiplier;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }
        }

        private void ApplyEffects()
        {
            if (cameraTransform == null) return;
            
            // Always apply effects from the original baseline to prevent accumulation
            Vector3 finalPosition = originalLocalPosition + bobOffset + shakeOffset;
            
            // Preserve current camera rotation and only add roll effect
            Vector3 currentRotation = cameraTransform.localEulerAngles;
            Vector3 finalRotation = new Vector3(currentRotation.x, currentRotation.y, currentRotation.z + currentRoll);
            
            cameraTransform.localPosition = finalPosition;
            cameraTransform.localEulerAngles = finalRotation;
        }

        public void SetCameraEffects(StateCameraEffects effects)
        {
            if (debugCameraEffects)
            {
                string previousType = currentEffects == standingEffects ? "Standing" :
                                    currentEffects == walkingEffects ? "Walking" :
                                    currentEffects == sprintingEffects ? "Sprinting" :
                                    currentEffects == crouchingEffects ? "Crouching" :
                                    currentEffects == slidingEffects ? "Sliding" :
                                    currentEffects == airborneEffects ? "Airborne" : "Unknown";
                                    
                string newType = effects == standingEffects ? "Standing" :
                               effects == walkingEffects ? "Walking" :
                               effects == sprintingEffects ? "Sprinting" :
                               effects == crouchingEffects ? "Crouching" :
                               effects == slidingEffects ? "Sliding" :
                               effects == airborneEffects ? "Airborne" : "Unknown";
                               
                Debug.Log($"[CameraEffects] Switching effects: {previousType} -> {newType}");
            }
            
            currentEffects = effects;
            
            // Reset effect state on state change to prevent accumulation
            ResetEffectState();
        }
        
        private void ResetEffectState()
        {
            // For transitions that should be more aggressive (like going to standing),
            // we can reduce bob intensity faster
            bool isTransitioningToLowerIntensity = currentEffects == standingEffects;
            
            if (isTransitioningToLowerIntensity && bobIntensity > 0.1f)
            {
                // More aggressive fade when transitioning to standing/stationary
                bobIntensity *= 0.5f; // Cut intensity in half immediately
                
                if (debugCameraEffects)
                {
                    Debug.Log($"[CameraEffects] Aggressive bob fade - intensity reduced to {bobIntensity:F2}");
                }
            }
            
            // Reset camera roll immediately for new state
            targetRoll = 0f;
            
            // Reset shake if not in progress
            if (shakeTimer <= 0f)
            {
                shakeOffset = Vector3.zero;
            }
            
            // FOV will transition naturally through UpdateFOV
        }

        public void TriggerShake(CameraShakeSettings shakeSettings)
        {
            currentShake = shakeSettings;
            shakeTimer = shakeSettings.duration;
            shakeDuration = shakeSettings.duration;
        }

        public void TriggerLandingShake()
        {
            if (currentEffects?.enableLandingShake == true)
            {
                TriggerShake(currentEffects.landingShake);
            }
        }

        // State-specific effect setters
        public void SetStandingEffects() => SetCameraEffects(standingEffects);
        public void SetWalkingEffects() => SetCameraEffects(walkingEffects);
        public void SetSprintingEffects() => SetCameraEffects(sprintingEffects);
        public void SetCrouchingEffects() => SetCameraEffects(crouchingEffects);
        public void SetSlidingEffects() => SetCameraEffects(slidingEffects);
        public void SetAirborneEffects() => SetCameraEffects(airborneEffects);

        // Movement data tracking
        private float currentMovementSpeed;
        private Vector2 currentMovementInput;
        private bool isMoving;
        private bool isSprinting;

        // Public methods for external updates
        public void UpdateMovementData(float movementSpeed, Vector2 movementInput, bool moving, bool sprinting)
        {
            currentMovementSpeed = movementSpeed;
            currentMovementInput = movementInput;
            isMoving = moving;
            isSprinting = sprinting;
        }

        // Helper methods to get input/movement data
        private float GetMovementSpeed()
        {
            // Return the actual current movement speed, not just the cached value
            return currentMovementSpeed;
        }

        private float GetStrafeInput()
        {
            return currentMovementInput.x;
        }
        
        // Get normalized movement speed for bob intensity (0-1 range)
        private float GetNormalizedMovementSpeed()
        {
            if (currentMovementSpeed <= 0.1f) return 0f;
            
            // Normalize against typical movement speeds
            float maxSpeed = 8f; // Approximate sprint speed
            return Mathf.Clamp01(currentMovementSpeed / maxSpeed);
        }

        // Public methods for runtime customization
        public void SetHeadBobSettings(HeadBobSettings settings, string stateName)
        {
            var effects = GetEffectsByStateName(stateName);
            if (effects != null)
            {
                effects.headBob = settings;
                effects.enableHeadBob = true;
            }
        }

        public void SetCameraRollSettings(CameraRollSettings settings, string stateName)
        {
            var effects = GetEffectsByStateName(stateName);
            if (effects != null)
            {
                effects.cameraRoll = settings;
                effects.enableCameraRoll = true;
            }
        }

        private StateCameraEffects GetEffectsByStateName(string stateName)
        {
            return stateName.ToLower() switch
            {
                "standing" => standingEffects,
                "walking" => walkingEffects,
                "sprinting" => sprintingEffects,
                "crouching" => crouchingEffects,
                "sliding" => slidingEffects,
                "airborne" => airborneEffects,
                _ => null
            };
        }

        private void ApplyUserSettings()
        {
            if (effectSettings?.UserSettings == null) return;

            var settings = effectSettings.UserSettings;
            
            // Apply global intensity
            globalIntensityMultiplier = settings.masterIntensity;
            
            // Apply state-specific toggles
            if (!settings.enableWalkingEffects)
            {
                walkingEffects.enableHeadBob = false;
                walkingEffects.enableCameraRoll = false;
                walkingEffects.enableFOVEffect = false;
            }
            
            if (!settings.enableSprintingEffects)
            {
                sprintingEffects.enableHeadBob = false;
                sprintingEffects.enableCameraRoll = false;
                sprintingEffects.enableFOVEffect = false;
            }
            
            if (!settings.enableCrouchingEffects)
            {
                crouchingEffects.enableHeadBob = false;
                crouchingEffects.enableCameraRoll = false;
                crouchingEffects.enableFOVEffect = false;
            }
            
            if (!settings.enableSlidingEffects)
            {
                slidingEffects.enableHeadBob = false;
                slidingEffects.enableCameraRoll = false;
                slidingEffects.enableFOVEffect = false;
            }
            
            if (!settings.enableAirborneEffects)
            {
                airborneEffects.enableLandingShake = false;
            }
        }

        public void ApplyPreset(CameraEffectPreset preset)
        {
            if (effectSettings != null)
            {
                effectSettings.ApplyPreset(preset);
                ApplyUserSettings();
            }
        }

        public void SetUserSetting(System.Action<CameraEffectUserSettings> modifier)
        {
            if (effectSettings?.UserSettings != null)
            {
                modifier(effectSettings.UserSettings);
                ApplyUserSettings();
            }
        }
        
        // Call this method if you need to recalibrate the baseline position
        public void RecalibrateBaseline()
        {
            if (cameraTransform != null)
            {
                // Reset effects first
                bobOffset = Vector3.zero;
                shakeOffset = Vector3.zero;
                currentRoll = 0f;
                
                // Apply the neutral position only (preserve rotation)
                cameraTransform.localPosition = originalLocalPosition;
                
                if (debugCameraEffects)
                {
                    Debug.Log($"[CameraEffects] Recalibrated to baseline - Position: {originalLocalPosition}");
                }
            }
        }
        
        private void OnValidate()
        {
            // Ensure proper initialization in editor
            if (Application.isPlaying && cameraTransform != null)
            {
                ValidateReferences();
            }
        }
    }
}