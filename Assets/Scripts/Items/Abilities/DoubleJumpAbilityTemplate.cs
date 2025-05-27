using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Items.Abilities
{
    [CreateAssetMenu(fileName = "DoubleJumpAbilityTemplate", menuName = "GameFramework/Equipment Abilities/Double Jump")]
    public class DoubleJumpAbilityTemplate : ScriptableObject, IEquipmentAbility
    {
        [Header("Double Jump Settings")]
        public float doubleJumpHeightMultiplier = 0.8f;
        public bool resetDoubleJumpOnWallContact = true;
        public AudioClip doubleJumpSound;
        
        [Header("Wall Jump Settings")]
        public bool enableWallJump = true;
        public float wallJumpForce = 8f;
        public float wallJumpUpwardForce = 6f;
        public float wallDetectionDistance = 0.8f;
        public LayerMask wallLayers = -1;
        public float wallJumpCooldown = 0.2f;
        public bool wallJumpResetsDoubleJump = true;
        
        [Header("Visual Effects")]
        public GameObject jumpEffectPrefab;
        public Color jumpTrailColor = Color.cyan;
        
        public bool IsActive => false; // Templates are never active
        
        public void OnEquipped(GameObject equipper)
        {
            // This method should never be called on templates
            Debug.LogWarning("OnEquipped called on DoubleJumpAbilityTemplate. This should not happen.");
        }
        
        public void OnUnequipped(GameObject equipper)
        {
            // This method should never be called on templates
            Debug.LogWarning("OnUnequipped called on DoubleJumpAbilityTemplate. This should not happen.");
        }
        
        public DoubleJumpAbility CreateAbilityComponent(GameObject target)
        {
            DoubleJumpAbility ability = target.AddComponent<DoubleJumpAbility>();
            
            // Copy settings from template using reflection
            var doubleJumpHeightField = typeof(DoubleJumpAbility).GetField("doubleJumpHeightMultiplier", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (doubleJumpHeightField != null)
                doubleJumpHeightField.SetValue(ability, doubleJumpHeightMultiplier);
                
            var wallResetField = typeof(DoubleJumpAbility).GetField("resetDoubleJumpOnWallContact", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallResetField != null)
                wallResetField.SetValue(ability, resetDoubleJumpOnWallContact);
                
            var soundField = typeof(DoubleJumpAbility).GetField("doubleJumpSound", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (soundField != null)
                soundField.SetValue(ability, doubleJumpSound);
            
            // Copy wall jump settings
            var enableWallJumpField = typeof(DoubleJumpAbility).GetField("enableWallJump", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (enableWallJumpField != null)
                enableWallJumpField.SetValue(ability, enableWallJump);
                
            var wallJumpForceField = typeof(DoubleJumpAbility).GetField("wallJumpForce", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallJumpForceField != null)
                wallJumpForceField.SetValue(ability, wallJumpForce);
                
            var wallJumpUpwardForceField = typeof(DoubleJumpAbility).GetField("wallJumpUpwardForce", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallJumpUpwardForceField != null)
                wallJumpUpwardForceField.SetValue(ability, wallJumpUpwardForce);
                
            var wallDetectionDistanceField = typeof(DoubleJumpAbility).GetField("wallDetectionDistance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallDetectionDistanceField != null)
                wallDetectionDistanceField.SetValue(ability, wallDetectionDistance);
                
            var wallLayersField = typeof(DoubleJumpAbility).GetField("wallLayers", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallLayersField != null)
                wallLayersField.SetValue(ability, wallLayers);
                
            var wallJumpCooldownField = typeof(DoubleJumpAbility).GetField("wallJumpCooldown", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallJumpCooldownField != null)
                wallJumpCooldownField.SetValue(ability, wallJumpCooldown);
                
            var wallJumpResetsDoubleJumpField = typeof(DoubleJumpAbility).GetField("wallJumpResetsDoubleJump", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wallJumpResetsDoubleJumpField != null)
                wallJumpResetsDoubleJumpField.SetValue(ability, wallJumpResetsDoubleJump);
            
            return ability;
        }
        
    }
}