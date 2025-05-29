using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GameFramework.Camera
{
    /// <summary>
    /// Comprehensive camera shake system using Cinemachine Impulse Sources.
    /// Provides designer-friendly presets and advanced shake configuration options.
    /// </summary>
    public class CameraShakeManager : MonoBehaviour
    {
        [Header("Impulse Sources")]
        [SerializeField] private CinemachineImpulseSource primaryShakeSource;
        [SerializeField] private CinemachineImpulseSource secondaryShakeSource;
        [SerializeField] private CinemachineImpulseSource environmentalShakeSource;
        
        [Header("Shake Presets")]
        [SerializeField] private CameraShakePreset[] shakePresets = new CameraShakePreset[]
        {
            new CameraShakePreset("Landing_Light", new Vector3(0.5f, 1f, 0.2f), 0.3f, 1f),
            new CameraShakePreset("Landing_Medium", new Vector3(1f, 2f, 0.5f), 0.5f, 1.2f),
            new CameraShakePreset("Landing_Heavy", new Vector3(2f, 3f, 1f), 0.8f, 1.5f),
            new CameraShakePreset("Explosion_Small", new Vector3(1.5f, 1f, 1.5f), 0.6f, 2f),
            new CameraShakePreset("Explosion_Large", new Vector3(3f, 2f, 3f), 1.2f, 2.5f),
            new CameraShakePreset("Footstep", new Vector3(0.1f, 0.2f, 0.05f), 0.1f, 0.8f),
            new CameraShakePreset("Environmental", new Vector3(0.3f, 0.5f, 0.2f), 2f, 1f)
        };
        
        [Header("Global Settings")]
        [Range(0f, 2f)]
        [Tooltip("Global multiplier for all shake effects")]
        [SerializeField] private float globalShakeIntensity = 1f;
        
        [Tooltip("Enable shake effects")]
        [SerializeField] private bool enableShake = true;
        
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool enableDebugLogging = false;
        
        [Header("Performance")]
        [Tooltip("Maximum number of simultaneous shake effects")]
        [SerializeField] private int maxSimultaneousShakes = 3;
        
        [Tooltip("Minimum time between shake effects of the same type")]
        [SerializeField] private float shakeThrottleTime = 0.1f;
        
        [Header("Auto-Setup")]
        [Tooltip("Create impulse sources automatically if missing")]
        [SerializeField] private bool autoCreateSources = true;

        // Runtime state
        private Dictionary<string, CameraShakePreset> presetLookup;
        private Dictionary<CinemachineImpulseSource, float> lastShakeTime;
        private Queue<PendingShake> pendingShakes;
        private int activeShakeCount = 0;

        // Events
        public event Action<string, float> OnShakeTriggered;
        public event Action<float> OnGlobalIntensityChanged;

        #region Properties
        public float GlobalShakeIntensity 
        { 
            get => globalShakeIntensity; 
            set 
            {
                globalShakeIntensity = Mathf.Clamp(value, 0f, 2f);
                OnGlobalIntensityChanged?.Invoke(globalShakeIntensity);
            }
        }
        
        public bool ShakeEnabled 
        { 
            get => enableShake; 
            set => enableShake = value; 
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeSystem();
        }

        private void Start()
        {
            if (autoCreateSources)
            {
                CreateMissingSources();
            }
            
            ValidateSources();
            BuildPresetLookup();
        }

        private void Update()
        {
            ProcessPendingShakes();
        }
        #endregion

        #region Initialization
        private void InitializeSystem()
        {
            presetLookup = new Dictionary<string, CameraShakePreset>();
            lastShakeTime = new Dictionary<CinemachineImpulseSource, float>();
            pendingShakes = new Queue<PendingShake>();
        }

        private void CreateMissingSources()
        {
            if (primaryShakeSource == null)
            {
                primaryShakeSource = CreateImpulseSource("PrimaryShakeSource", ImpulseSourceType.Primary);
            }
            
            if (secondaryShakeSource == null)
            {
                secondaryShakeSource = CreateImpulseSource("SecondaryShakeSource", ImpulseSourceType.Secondary);
            }
            
            if (environmentalShakeSource == null)
            {
                environmentalShakeSource = CreateImpulseSource("EnvironmentalShakeSource", ImpulseSourceType.Environmental);
            }
        }

        private CinemachineImpulseSource CreateImpulseSource(string sourceName, ImpulseSourceType sourceType)
        {
            var sourceGO = new GameObject(sourceName);
            sourceGO.transform.SetParent(transform);
            
            var impulseSource = sourceGO.AddComponent<CinemachineImpulseSource>();
            
            // Configure based on source type
            switch (sourceType)
            {
                case ImpulseSourceType.Primary:
                    ConfigurePrimarySource(impulseSource);
                    break;
                case ImpulseSourceType.Secondary:
                    ConfigureSecondarySource(impulseSource);
                    break;
                case ImpulseSourceType.Environmental:
                    ConfigureEnvironmentalSource(impulseSource);
                    break;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[CameraShakeManager] Created impulse source: {sourceName}");
            }
            
            return impulseSource;
        }

        private void ConfigurePrimarySource(CinemachineImpulseSource source)
        {
            source.ImpulseDefinition.ImpulseDuration = 0.5f;
            source.ImpulseDefinition.TimeEnvelope.DecayTime = 1f;
            source.DefaultVelocity = Vector3.one;
        }

        private void ConfigureSecondarySource(CinemachineImpulseSource source)
        {
            source.ImpulseDefinition.ImpulseDuration = 0.3f;
            source.ImpulseDefinition.TimeEnvelope.DecayTime = 0.8f;
            source.DefaultVelocity = Vector3.one * 0.7f;
        }

        private void ConfigureEnvironmentalSource(CinemachineImpulseSource source)
        {
            source.ImpulseDefinition.ImpulseDuration = 1f;
            source.ImpulseDefinition.TimeEnvelope.DecayTime = 2f;
            source.DefaultVelocity = Vector3.one * 0.5f;
        }

        private void ValidateSources()
        {
            if (primaryShakeSource == null)
            {
                Debug.LogWarning("[CameraShakeManager] Primary shake source is missing!");
            }
            
            if (secondaryShakeSource == null)
            {
                Debug.LogWarning("[CameraShakeManager] Secondary shake source is missing!");
            }
        }

        private void BuildPresetLookup()
        {
            presetLookup.Clear();
            
            foreach (var preset in shakePresets)
            {
                if (!string.IsNullOrEmpty(preset.name))
                {
                    presetLookup[preset.name] = preset;
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[CameraShakeManager] Built preset lookup with {presetLookup.Count} presets");
            }
        }
        #endregion

        #region Shake Triggering
        /// <summary>
        /// Trigger a shake effect using a preset name
        /// </summary>
        public void TriggerShake(string presetName, float intensityMultiplier = 1f)
        {
            if (!enableShake) return;
            
            if (presetLookup.TryGetValue(presetName, out var preset))
            {
                TriggerShake(preset, intensityMultiplier);
            }
            else
            {
                Debug.LogWarning($"[CameraShakeManager] Shake preset '{presetName}' not found!");
            }
        }

        /// <summary>
        /// Trigger a shake effect using a preset object
        /// </summary>
        public void TriggerShake(CameraShakePreset preset, float intensityMultiplier = 1f)
        {
            if (!enableShake || preset == null) return;
            
            float finalIntensity = intensityMultiplier * globalShakeIntensity;
            Vector3 finalVelocity = preset.velocity * finalIntensity;
            
            var shake = new PendingShake
            {
                velocity = finalVelocity,
                duration = preset.duration,
                frequency = preset.frequency,
                presetName = preset.name,
                sourceType = DetermineOptimalSource(preset)
            };
            
            if (CanTriggerShake(shake))
            {
                pendingShakes.Enqueue(shake);
            }
        }

        /// <summary>
        /// Trigger a custom shake effect
        /// </summary>
        public void TriggerCustomShake(Vector3 velocity, float duration = 0.5f, float frequency = 1f, ImpulseSourceType sourceType = ImpulseSourceType.Primary)
        {
            if (!enableShake) return;
            
            Vector3 finalVelocity = velocity * globalShakeIntensity;
            
            var shake = new PendingShake
            {
                velocity = finalVelocity,
                duration = duration,
                frequency = frequency,
                presetName = "Custom",
                sourceType = sourceType
            };
            
            pendingShakes.Enqueue(shake);
        }

        /// <summary>
        /// Trigger a landing shake based on impact velocity
        /// </summary>
        public void TriggerLandingShake(float impactVelocity, float velocityThreshold = 5f)
        {
            if (!enableShake || Mathf.Abs(impactVelocity) < velocityThreshold) return;
            
            // Determine shake intensity based on velocity
            string presetName;
            if (Mathf.Abs(impactVelocity) < 10f)
            {
                presetName = "Landing_Light";
            }
            else if (Mathf.Abs(impactVelocity) < 15f)
            {
                presetName = "Landing_Medium";
            }
            else
            {
                presetName = "Landing_Heavy";
            }
            
            // Scale intensity based on velocity
            float velocityRatio = Mathf.Clamp01(Mathf.Abs(impactVelocity) / 20f);
            TriggerShake(presetName, velocityRatio);
        }

        private bool CanTriggerShake(PendingShake shake)
        {
            // Check active shake limit
            if (activeShakeCount >= maxSimultaneousShakes) return false;
            
            // Check throttling
            var source = GetSourceForType(shake.sourceType);
            if (source != null && lastShakeTime.TryGetValue(source, out float lastTime))
            {
                if (Time.time - lastTime < shakeThrottleTime) return false;
            }
            
            return true;
        }

        private void ProcessPendingShakes()
        {
            while (pendingShakes.Count > 0 && activeShakeCount < maxSimultaneousShakes)
            {
                var shake = pendingShakes.Dequeue();
                ExecuteShake(shake);
            }
        }

        private void ExecuteShake(PendingShake shake)
        {
            var source = GetSourceForType(shake.sourceType);
            if (source == null) return;
            
            // Configure source for this shake
            source.DefaultVelocity = shake.velocity;
            source.ImpulseDefinition.ImpulseDuration = shake.duration;
            
            // Trigger the impulse
            source.GenerateImpulse();
            
            // Update tracking
            lastShakeTime[source] = Time.time;
            activeShakeCount++;
            
            // Schedule shake completion
            StartCoroutine(OnShakeComplete(shake.duration));
            
            // Invoke events
            OnShakeTriggered?.Invoke(shake.presetName, shake.velocity.magnitude);
            
            if (enableDebugLogging)
            {
                Debug.Log($"[CameraShakeManager] Executed shake '{shake.presetName}' with velocity {shake.velocity.magnitude:F2}");
            }
        }

        private System.Collections.IEnumerator OnShakeComplete(float duration)
        {
            yield return new WaitForSeconds(duration);
            activeShakeCount = Mathf.Max(0, activeShakeCount - 1);
        }
        #endregion

        #region Utilities
        private ImpulseSourceType DetermineOptimalSource(CameraShakePreset preset)
        {
            // Logic to determine best source based on preset characteristics
            if (preset.duration > 1f)
            {
                return ImpulseSourceType.Environmental;
            }
            else if (preset.velocity.magnitude > 2f)
            {
                return ImpulseSourceType.Primary;
            }
            else
            {
                return ImpulseSourceType.Secondary;
            }
        }

        private CinemachineImpulseSource GetSourceForType(ImpulseSourceType sourceType)
        {
            return sourceType switch
            {
                ImpulseSourceType.Primary => primaryShakeSource,
                ImpulseSourceType.Secondary => secondaryShakeSource,
                ImpulseSourceType.Environmental => environmentalShakeSource,
                _ => primaryShakeSource
            };
        }

        /// <summary>
        /// Stop all active shake effects
        /// </summary>
        public void StopAllShakes()
        {
            // Clear impulses - updated for newer Cinemachine API
            CinemachineImpulseManager.Instance?.Clear();
            activeShakeCount = 0;
            pendingShakes.Clear();
            
            if (enableDebugLogging)
            {
                Debug.Log("[CameraShakeManager] All shakes stopped");
            }
        }

        /// <summary>
        /// Get all available preset names
        /// </summary>
        public string[] GetAvailablePresets()
        {
            var presetNames = new string[shakePresets.Length];
            for (int i = 0; i < shakePresets.Length; i++)
            {
                presetNames[i] = shakePresets[i].name;
            }
            return presetNames;
        }
        #endregion

        #region Editor Utilities
        [ContextMenu("Test Light Landing")]
        private void TestLightLanding()
        {
            TriggerShake("Landing_Light");
        }

        [ContextMenu("Test Heavy Landing")]
        private void TestHeavyLanding()
        {
            TriggerShake("Landing_Heavy");
        }

        [ContextMenu("Test Explosion")]
        private void TestExplosion()
        {
            TriggerShake("Explosion_Large");
        }

        [ContextMenu("Stop All Shakes")]
        private void EditorStopAllShakes()
        {
            StopAllShakes();
        }
        #endregion
    }

    /// <summary>
    /// Configuration for a camera shake preset
    /// </summary>
    [System.Serializable]
    public class CameraShakePreset
    {
        [Tooltip("Name of the shake preset")]
        public string name;
        
        [Tooltip("Velocity vector for the shake")]
        public Vector3 velocity;
        
        [Tooltip("Duration of the shake effect")]
        public float duration;
        
        [Tooltip("Frequency of the shake oscillation")]
        public float frequency;

        public CameraShakePreset(string presetName, Vector3 shakeVelocity, float shakeDuration, float shakeFrequency)
        {
            name = presetName;
            velocity = shakeVelocity;
            duration = shakeDuration;
            frequency = shakeFrequency;
        }
    }

    /// <summary>
    /// Types of impulse sources for different shake categories
    /// </summary>
    public enum ImpulseSourceType
    {
        Primary,        // Main gameplay shakes (landing, impacts)
        Secondary,      // Lighter effects (footsteps, small impacts)
        Environmental   // Ambient effects (explosions, environmental)
    }

    /// <summary>
    /// Internal structure for queued shake effects
    /// </summary>
    internal struct PendingShake
    {
        public Vector3 velocity;
        public float duration;
        public float frequency;
        public string presetName;
        public ImpulseSourceType sourceType;
    }
}