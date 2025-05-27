using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Locomotion;

namespace GameFramework.Items.Abilities
{
    public class DoubleJumpAbility : MonoBehaviour, IEquipmentAbility
    {
        [Header("Double Jump Settings")]
        [SerializeField] private float doubleJumpHeightMultiplier = 0.8f;
        [SerializeField] private bool resetDoubleJumpOnWallContact = true;
        [SerializeField] private AudioClip doubleJumpSound;
        
        [Header("Wall Jump Settings")]
        [SerializeField] private bool enableWallJump = true;
        [SerializeField] private float wallJumpForce = 8f;
        [SerializeField] private float wallJumpUpwardForce = 6f;
        [SerializeField] private float wallDetectionDistance = 0.8f;
        [SerializeField] private LayerMask wallLayers = -1;
        [SerializeField] private float wallJumpCooldown = 0.2f;
        [SerializeField] private bool wallJumpResetsDoubleJump = true;
        
        private FirstPersonLocomotionController locomotionController;
        private IInputHandler inputHandler;
        private AudioSource audioSource;
        private bool hasDoubleJumped = false;
        private bool wasGroundedLastFrame = false;
        private float lastWallJumpTime = 0f;
        private Vector3 lastWallNormal = Vector3.zero;
        
        public bool IsActive { get; private set; } = false;
        public bool HasDoubleJumped => hasDoubleJumped;
        public bool CanDoubleJump => !hasDoubleJumped && !locomotionController.IsGrounded;
        public bool CanWallJump => enableWallJump && !locomotionController.IsGrounded && Time.time - lastWallJumpTime >= wallJumpCooldown;
        
        public void OnEquipped(GameObject equipper)
        {
            locomotionController = equipper.GetComponent<FirstPersonLocomotionController>();
            inputHandler = equipper.GetComponent<IInputHandler>();
            audioSource = equipper.GetComponent<AudioSource>();
            
            if (locomotionController == null)
            {
                Debug.LogError($"DoubleJumpAbility requires a FirstPersonLocomotionController on {equipper.name}");
                return;
            }
            
            if (audioSource == null)
            {
                audioSource = equipper.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = equipper.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }
            }
            
            IsActive = true;
            hasDoubleJumped = false;
            wasGroundedLastFrame = locomotionController.IsGrounded;
            
            Debug.Log("Double Jump ability equipped!");
        }
        
        public void OnUnequipped(GameObject equipper)
        {
            IsActive = false;
            locomotionController = null;
            inputHandler = null;
            audioSource = null;
            
            Debug.Log("Double Jump ability unequipped!");
        }
        
        private void Update()
        {
            if (!IsActive || locomotionController == null) return;
            
            // Reset double jump when landing
            if (locomotionController.IsGrounded && !wasGroundedLastFrame)
            {
                hasDoubleJumped = false;
            }
            
            // Reset double jump on wall contact if enabled
            if (resetDoubleJumpOnWallContact && DetectWallContact(out Vector3 wallNormal))
            {
                hasDoubleJumped = false;
                lastWallNormal = wallNormal;
            }
            
            wasGroundedLastFrame = locomotionController.IsGrounded;
        }
        
        public bool TryDoubleJump()
        {
            if (!CanDoubleJump) return false;
            
            // Perform double jump with reduced height
            float originalJumpHeight = GetOriginalJumpHeight();
            float doubleJumpHeight = originalJumpHeight * doubleJumpHeightMultiplier;
            
            // Calculate and apply double jump velocity
            float doubleJumpVelocity = Mathf.Sqrt(doubleJumpHeight * -2f * GetGravity());
            Vector3 velocity = locomotionController.Velocity;
            velocity.y = doubleJumpVelocity;
            locomotionController.SetVelocity(velocity);
            
            hasDoubleJumped = true;
            
            // Play double jump sound
            if (doubleJumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(doubleJumpSound);
            }
            
            Debug.Log("Double jump performed!");
            return true;
        }
        
        public bool TryWallJump()
        {
            if (!CanWallJump) return false;
            
            if (DetectWallContact(out Vector3 wallNormal))
            {
                // Calculate wall jump velocity
                Vector3 currentVelocity = locomotionController.Velocity;
                
                // Add force away from wall (horizontal)
                Vector3 wallJumpDirection = wallNormal * wallJumpForce;
                
                // Add upward force
                Vector3 upwardForce = Vector3.up * wallJumpUpwardForce;
                
                // Combine forces and add to current velocity (don't override)
                Vector3 totalWallJumpForce = wallJumpDirection + upwardForce;
                Vector3 newVelocity = currentVelocity + totalWallJumpForce;
                
                locomotionController.SetVelocity(newVelocity);
                
                // Update wall jump tracking
                lastWallJumpTime = Time.time;
                lastWallNormal = wallNormal;
                
                // Reset double jump if enabled
                if (wallJumpResetsDoubleJump)
                {
                    hasDoubleJumped = false;
                }
                
                // Play wall jump sound
                if (doubleJumpSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(doubleJumpSound, 0.8f); // Slightly quieter for wall jump
                }
                
                Debug.Log($"Wall jump performed! Wall normal: {wallNormal}, Force applied: {totalWallJumpForce}");
                return true;
            }
            
            return false;
        }
        
        private bool DetectWallContact(out Vector3 wallNormal)
        {
            wallNormal = Vector3.zero;
            CharacterController characterController = locomotionController.CharacterController;
            if (characterController == null) return false;
            
            Vector3 centerPosition = transform.position + Vector3.up * (characterController.height * 0.5f);
            float detectionRadius = characterController.radius + 0.1f;
            
            // Check multiple directions around the character
            Vector3[] directions = {
                transform.forward,
                -transform.forward,
                transform.right,
                -transform.right,
                (transform.forward + transform.right).normalized,
                (transform.forward - transform.right).normalized,
                (-transform.forward + transform.right).normalized,
                (-transform.forward - transform.right).normalized
            };
            
            foreach (Vector3 direction in directions)
            {
                if (Physics.Raycast(centerPosition, direction, out RaycastHit hit, wallDetectionDistance, wallLayers, QueryTriggerInteraction.Ignore))
                {
                    // Check if the surface is wall-like (not too horizontal)
                    float wallAngle = Vector3.Angle(Vector3.up, hit.normal);
                    if (wallAngle > 30f && wallAngle < 150f) // Between 30-150 degrees from vertical
                    {
                        wallNormal = hit.normal;
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        private float GetOriginalJumpHeight()
        {
            // Use reflection to get the jump height from the locomotion controller
            var field = typeof(FirstPersonLocomotionController).GetField("jumpHeight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(locomotionController);
            }
            return 2f; // Fallback default
        }
        
        private float GetGravity()
        {
            // Use reflection to get the gravity from the locomotion controller
            var field = typeof(FirstPersonLocomotionController).GetField("gravity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (float)field.GetValue(locomotionController);
            }
            return -20f; // Fallback default
        }
        
        // Public method that can be called by input handlers or other systems
        public void HandleJumpInput(bool jumpPressed)
        {
            if (!jumpPressed) return;
            
            // Try wall jump first, then double jump
            if (CanWallJump && TryWallJump())
            {
                return;
            }
            
            if (CanDoubleJump)
            {
                TryDoubleJump();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!IsActive || locomotionController?.CharacterController == null) return;
            
            // Draw double jump availability indicator
            Gizmos.color = CanDoubleJump ? Color.cyan : (hasDoubleJumped ? Color.red : Color.green);
            Vector3 position = transform.position + Vector3.up * 2.5f;
            Gizmos.DrawWireSphere(position, 0.2f);
            
            // Draw wall detection visualization
            if (locomotionController?.CharacterController != null)
            {
                Vector3 centerPosition = transform.position + Vector3.up * (locomotionController.CharacterController.height * 0.5f);
                
                // Draw wall detection sphere
                Gizmos.color = enableWallJump ? (CanWallJump ? Color.green : Color.yellow) : Color.gray;
                Gizmos.DrawWireSphere(centerPosition, wallDetectionDistance);
                
                // Draw detection rays in all directions
                Vector3[] directions = {
                    transform.forward,
                    -transform.forward,
                    transform.right,
                    -transform.right,
                    (transform.forward + transform.right).normalized,
                    (transform.forward - transform.right).normalized,
                    (-transform.forward + transform.right).normalized,
                    (-transform.forward - transform.right).normalized
                };
                
                Gizmos.color = Color.cyan;
                foreach (Vector3 direction in directions)
                {
                    Gizmos.DrawRay(centerPosition, direction * wallDetectionDistance);
                }
                
                // Draw last wall normal if we have one
                if (lastWallNormal != Vector3.zero)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(centerPosition, lastWallNormal * 2f);
                }
            }
        }
    }
}