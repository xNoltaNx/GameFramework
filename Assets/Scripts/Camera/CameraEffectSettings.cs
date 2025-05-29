// DEPRECATED: Legacy camera effect settings - replaced with Cinemachine profiles
// This file contained the previous camera effects configuration system
//
// Previous Features:
// - User settings with master intensity controls
// - State-specific effect enable/disable toggles  
// - Preset system (Disabled, Minimal, Standard, Cinematic, Extreme)
// - Legacy multiplier-based and comprehensive absolute value configurations
//
// TODO: Replace with Cinemachine configuration:
// - Use Cinemachine Noise Settings instead of user settings
// - Create Cinemachine Impulse Settings for camera shake
// - Set up virtual camera profiles for different movement states
// - Use Cinemachine Brain for smooth transitions between cameras

using UnityEngine;

namespace GameFramework.Camera
{
    [System.Obsolete("CameraEffectSettings is deprecated. Use Cinemachine Noise Settings instead.")]
    [CreateAssetMenu(fileName = "LegacyCameraEffectSettings", menuName = "GameFramework/Legacy/Camera Effect Settings (Deprecated)")]
    public class CameraEffectSettings : ScriptableObject
    {
        [Header("DEPRECATED - Use Cinemachine Instead")]
        [TextArea(3, 5)]
        public string migrationNote = "This asset is deprecated. Please use Cinemachine virtual cameras with noise profiles for camera effects instead.";
        
        private void OnEnable()
        {
            Debug.LogWarning("[CameraEffectSettings] This ScriptableObject is deprecated. " +
                           "Please use Cinemachine virtual cameras with noise profiles instead.");
        }
    }

    [System.Obsolete("Replace with Cinemachine Noise Settings")]
    [System.Serializable]
    public class CameraEffectUserSettings
    {
        // Previous implementation: global intensity multipliers and effect toggles
        // Cinemachine equivalent: Use Noise Settings on virtual cameras
    }

    [System.Obsolete("Replace with Cinemachine virtual camera priorities")]
    public enum CameraEffectPreset
    {
        Disabled,   // Cinemachine equivalent: Low noise amplitude
        Minimal,    // Cinemachine equivalent: Subtle noise profiles
        Standard,   // Cinemachine equivalent: Balanced noise settings
        Cinematic,  // Cinemachine equivalent: Enhanced dramatic noise
        Extreme     // Cinemachine equivalent: High amplitude noise
    }
}