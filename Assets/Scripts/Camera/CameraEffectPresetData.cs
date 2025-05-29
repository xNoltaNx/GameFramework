// DEPRECATED: Legacy camera effect preset data - replaced with Cinemachine configurations
// This file contained preset data for the previous custom camera effects system
//
// Previous Features:
// - Comprehensive preset configurations with absolute values
// - State-specific effect definitions (standing, walking, sprinting, etc.)
// - Master intensity scaling and effect cloning utilities
// - Preset factories for disabled/minimal/standard/cinematic/extreme configurations
//
// TODO: Replace with Cinemachine virtual camera setups:
// - Create individual virtual cameras for each movement state
// - Configure noise profiles per camera for head bob simulation
// - Set up impulse sources for camera shake effects
// - Use different FOV settings per virtual camera
// - Implement priority-based camera switching

using UnityEngine;

namespace GameFramework.Camera
{
    [System.Obsolete("CameraEffectPresetData is deprecated. Use Cinemachine virtual camera configurations instead.")]
    [System.Serializable]
    public class CameraEffectPresetData
    {
        [Header("DEPRECATED - Use Cinemachine Virtual Cameras")]
        public string presetName = "Legacy Preset (Deprecated)";
        public string description = "Replace with Cinemachine virtual camera setup";
        
        // Previous implementation contained:
        // - Master intensity controls
        // - Per-state camera effect configurations
        // - Head bob, camera roll, FOV, and shake settings
        //
        // Cinemachine equivalent:
        // - Individual virtual cameras for each movement state
        // - Noise settings for procedural camera movement
        // - Impulse sources for camera shake
        // - FOV transitions via camera switching
        
        public CameraEffectPresetData()
        {
            Debug.LogWarning("[CameraEffectPresetData] This class is deprecated. " +
                           "Use Cinemachine virtual cameras instead.");
        }
        
        public CameraEffectPresetData(string name, string desc)
        {
            presetName = name;
            description = desc;
            Debug.LogWarning("[CameraEffectPresetData] This class is deprecated. " +
                           "Use Cinemachine virtual cameras instead.");
        }
    }
}