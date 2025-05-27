using UnityEngine;

namespace GameFramework.Camera
{
    [System.Serializable]
    public class CameraEffectUserSettings
    {
        [Header("Global Settings")]
        [Range(0f, 2f)]
        public float masterIntensity = 1f;
        
        [Header("Head Bob Settings")]
        public bool enableHeadBob = true;
        [Range(0f, 2f)]
        public float headBobIntensity = 1f;
        [Range(0.5f, 3f)]
        public float headBobFrequency = 1f;
        
        [Header("Camera Roll Settings")]
        public bool enableCameraRoll = true;
        [Range(0f, 2f)]
        public float cameraRollIntensity = 1f;
        
        [Header("Field of View Effects")]
        public bool enableFOVEffects = true;
        [Range(0f, 2f)]
        public float fovEffectIntensity = 1f;
        
        [Header("Camera Shake Settings")]
        public bool enableCameraShake = true;
        [Range(0f, 2f)]
        public float shakeIntensity = 1f;
        
        [Header("State-Specific Overrides")]
        public bool enableWalkingEffects = true;
        public bool enableSprintingEffects = true;
        public bool enableCrouchingEffects = true;
        public bool enableSlidingEffects = true;
        public bool enableAirborneEffects = true;
    }

    [CreateAssetMenu(fileName = "CameraEffectSettings", menuName = "GameFramework/Camera/Effect Settings")]
    public class CameraEffectSettings : ScriptableObject
    {
        [SerializeField] private CameraEffectUserSettings userSettings = new CameraEffectUserSettings();
        
        public CameraEffectUserSettings UserSettings => userSettings;
        
        [Header("Preset Configurations")]
        [SerializeField] private CameraEffectUserSettings disabledPreset;
        [SerializeField] private CameraEffectUserSettings minimalPreset;
        [SerializeField] private CameraEffectUserSettings standardPreset;
        [SerializeField] private CameraEffectUserSettings cinematicPreset;
        [SerializeField] private CameraEffectUserSettings extremePreset;

        private void OnEnable()
        {
            InitializePresets();
        }

        private void InitializePresets()
        {
            // Disabled preset - all effects off
            disabledPreset = new CameraEffectUserSettings
            {
                masterIntensity = 0f,
                enableHeadBob = false,
                enableCameraRoll = false,
                enableFOVEffects = false,
                enableCameraShake = false
            };

            // Minimal preset - very subtle effects
            minimalPreset = new CameraEffectUserSettings
            {
                masterIntensity = 0.3f,
                enableHeadBob = true,
                headBobIntensity = 0.5f,
                headBobFrequency = 0.8f,
                enableCameraRoll = false,
                enableFOVEffects = false,
                enableCameraShake = true,
                shakeIntensity = 0.5f
            };

            // Standard preset - balanced effects
            standardPreset = new CameraEffectUserSettings
            {
                masterIntensity = 1f,
                enableHeadBob = true,
                headBobIntensity = 1f,
                headBobFrequency = 1f,
                enableCameraRoll = true,
                cameraRollIntensity = 1f,
                enableFOVEffects = true,
                fovEffectIntensity = 1f,
                enableCameraShake = true,
                shakeIntensity = 1f
            };

            // Cinematic preset - enhanced dramatic effects
            cinematicPreset = new CameraEffectUserSettings
            {
                masterIntensity = 1.3f,
                enableHeadBob = true,
                headBobIntensity = 1.2f,
                headBobFrequency = 1.1f,
                enableCameraRoll = true,
                cameraRollIntensity = 1.5f,
                enableFOVEffects = true,
                fovEffectIntensity = 1.3f,
                enableCameraShake = true,
                shakeIntensity = 1.2f
            };

            // Extreme preset - maximum effects
            extremePreset = new CameraEffectUserSettings
            {
                masterIntensity = 2f,
                enableHeadBob = true,
                headBobIntensity = 2f,
                headBobFrequency = 1.5f,
                enableCameraRoll = true,
                cameraRollIntensity = 2f,
                enableFOVEffects = true,
                fovEffectIntensity = 2f,
                enableCameraShake = true,
                shakeIntensity = 2f
            };
        }

        public void ApplyPreset(CameraEffectPreset preset)
        {
            switch (preset)
            {
                case CameraEffectPreset.Disabled:
                    userSettings = CloneSettings(disabledPreset);
                    break;
                case CameraEffectPreset.Minimal:
                    userSettings = CloneSettings(minimalPreset);
                    break;
                case CameraEffectPreset.Standard:
                    userSettings = CloneSettings(standardPreset);
                    break;
                case CameraEffectPreset.Cinematic:
                    userSettings = CloneSettings(cinematicPreset);
                    break;
                case CameraEffectPreset.Extreme:
                    userSettings = CloneSettings(extremePreset);
                    break;
            }
        }

        private CameraEffectUserSettings CloneSettings(CameraEffectUserSettings source)
        {
            return new CameraEffectUserSettings
            {
                masterIntensity = source.masterIntensity,
                enableHeadBob = source.enableHeadBob,
                headBobIntensity = source.headBobIntensity,
                headBobFrequency = source.headBobFrequency,
                enableCameraRoll = source.enableCameraRoll,
                cameraRollIntensity = source.cameraRollIntensity,
                enableFOVEffects = source.enableFOVEffects,
                fovEffectIntensity = source.fovEffectIntensity,
                enableCameraShake = source.enableCameraShake,
                shakeIntensity = source.shakeIntensity,
                enableWalkingEffects = source.enableWalkingEffects,
                enableSprintingEffects = source.enableSprintingEffects,
                enableCrouchingEffects = source.enableCrouchingEffects,
                enableSlidingEffects = source.enableSlidingEffects,
                enableAirborneEffects = source.enableAirborneEffects
            };
        }

        public void ResetToDefault()
        {
            ApplyPreset(CameraEffectPreset.Standard);
        }

        public void SaveSettings()
        {
            // In a real implementation, you might save to PlayerPrefs or a save file
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public enum CameraEffectPreset
    {
        Disabled,
        Minimal,
        Standard,
        Cinematic,
        Extreme
    }
}