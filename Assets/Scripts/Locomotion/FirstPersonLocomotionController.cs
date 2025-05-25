using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Locomotion
{
    public class FirstPersonLocomotionController : MonoBehaviour, ILocomotionController
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpTimeout = 0.1f;
        [SerializeField] private float fallTimeout = 0.15f;
        
        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask = 1;
        [SerializeField] private float groundCheckRadius = 0.28f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private Transform groundCheck;
        
        [Header("Debug")]
        [SerializeField] private bool debugGravity = false;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        
        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 velocity;
        private Vector3 targetVelocity;
        private bool isGrounded;
        private bool wasGrounded;
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;
        private bool isCrouching;
        private float currentHeight;

        public bool IsGrounded => isGrounded;
        public bool IsMoving => targetVelocity.magnitude > 0.1f;
        public bool IsSprinting { get; private set; }
        public bool IsCrouching => isCrouching;
        public Vector3 Velocity => velocity;

        private void Awake()
        {
            ValidateReferences();
            InitializeValues();
        }

        private void ValidateReferences()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
                if (characterController == null)
                {
                    Debug.LogError($"FirstPersonLocomotionController on {gameObject.name} requires a CharacterController component!");
                }
            }

            if (groundCheck == null)
            {
                groundCheck = transform;
                Debug.LogWarning($"FirstPersonLocomotionController on {gameObject.name}: No groundCheck assigned, using self transform.");
            }
            
            // Validate setup on startup
            if (debugGravity && characterController != null)
            {
                Debug.Log($"Character Controller Setup - Height: {characterController.height}, " +
                         $"Radius: {characterController.radius}, Center: {characterController.center}, " +
                         $"Player Layer: {gameObject.layer}, Ground Mask: {groundMask.value}");
                         
                // Check if player is on ground layer (common mistake)
                if (((1 << gameObject.layer) & groundMask) != 0)
                {
                    Debug.LogError("SETUP ERROR: Player GameObject is on a layer included in groundMask! " +
                                  "This will cause constant ground detection. Move player to a different layer.");
                }
            }
        }

        private void InitializeValues()
        {
            jumpTimeoutDelta = jumpTimeout;
            fallTimeoutDelta = fallTimeout;
            currentHeight = standingHeight;
            
            if (characterController != null)
            {
                characterController.height = currentHeight;
            }
        }

        public void Initialize(CharacterController controller, Transform camera)
        {
            characterController = controller;
            cameraTransform = camera;
            
            if (characterController != null)
            {
                characterController.height = standingHeight;
                currentHeight = standingHeight;
            }
        }

        private void Update()
        {
            GroundCheck();
            ApplyGravity();
            HandleCrouchTransition();
        }

        private void GroundCheck()
        {
            wasGrounded = isGrounded;
            
            // Calculate ground check position at the bottom of the character controller
            Vector3 spherePosition = new Vector3(transform.position.x, 
                                                transform.position.y - (characterController.height / 2f) + groundCheckDistance, 
                                                transform.position.z);
            
            // Use OverlapSphere for more detailed debugging
            Collider[] hitColliders = Physics.OverlapSphere(spherePosition, groundCheckRadius, groundMask);
            isGrounded = hitColliders.Length > 0;
            
            if (debugGravity)
            {
                Debug.Log($"Ground Check - Position: {spherePosition}, IsGrounded: {isGrounded}, " +
                         $"Hit Objects: {hitColliders.Length}, Velocity.y: {velocity.y}, " +
                         $"GroundMask: {groundMask.value}, CharHeight: {characterController.height}");
                
                if (hitColliders.Length > 0)
                {
                    foreach (var collider in hitColliders)
                    {
                        Debug.Log($"  - Hit: {collider.name} on layer {collider.gameObject.layer}");
                    }
                }
            }
        }

        private void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0.0f)
            {
                // Keep a small downward velocity to ensure we stay grounded
                velocity.y = -2f;
            }
            else if (!isGrounded)
            {
                // Apply gravity when not grounded
                velocity.y += gravity * Time.deltaTime;
            }

            // Handle fall timeout
            if (velocity.y < 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                fallTimeoutDelta = fallTimeout;
            }

            if (debugGravity)
            {
                Debug.Log($"Gravity Applied - IsGrounded: {isGrounded}, Velocity.y: {velocity.y}, Gravity: {gravity}");
            }
        }

        public void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            HandleCrouch(crouchHeld);
            
            float currentSpeed = GetCurrentMovementSpeed(sprintHeld, crouchHeld);
            IsSprinting = sprintHeld && !isCrouching && movementInput.magnitude > 0.1f;

            Vector3 inputDirection = GetMovementDirection(movementInput);
            targetVelocity = inputDirection * currentSpeed;

            Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float lerpSpeed = targetVelocity.magnitude > currentHorizontalVelocity.magnitude ? acceleration : deceleration;
            
            Vector3 newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, lerpSpeed * Time.deltaTime);
            velocity = new Vector3(newHorizontalVelocity.x, velocity.y, newHorizontalVelocity.z);

            if (characterController != null)
            {
                characterController.Move(velocity * Time.deltaTime);
            }
        }

        public void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }

            if (jumpPressed && jumpTimeoutDelta <= 0.0f && isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpTimeoutDelta = jumpTimeout;
            }
        }

        private void HandleCrouch(bool crouchHeld)
        {
            if (crouchHeld && !isCrouching)
            {
                StartCrouch();
            }
            else if (!crouchHeld && isCrouching)
            {
                if (CanStandUp())
                {
                    StopCrouch();
                }
            }
        }

        private void HandleCrouchTransition()
        {
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            
            if (characterController != null)
            {
                characterController.height = currentHeight;
            }
        }

        private void StartCrouch()
        {
            isCrouching = true;
        }

        private void StopCrouch()
        {
            isCrouching = false;
        }

        private bool CanStandUp()
        {
            Vector3 capsuleTop = transform.position + Vector3.up * standingHeight;
            return !Physics.CheckSphere(capsuleTop, characterController.radius, groundMask);
        }

        private float GetCurrentMovementSpeed(bool sprintHeld, bool crouchHeld)
        {
            if (crouchHeld)
                return crouchSpeed;
            
            return sprintHeld ? sprintSpeed : walkSpeed;
        }

        private Vector3 GetMovementDirection(Vector2 movementInput)
        {
            if (cameraTransform == null)
                return transform.TransformDirection(new Vector3(movementInput.x, 0f, movementInput.y));

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return (forward * movementInput.y + right * movementInput.x).normalized;
        }

        public void SetMovementSpeed(float newWalkSpeed, float newSprintSpeed)
        {
            walkSpeed = newWalkSpeed;
            sprintSpeed = newSprintSpeed;
        }

        public void SetJumpHeight(float newJumpHeight)
        {
            jumpHeight = newJumpHeight;
        }

        private void OnDrawGizmosSelected()
        {
            if (characterController != null)
            {
                // Draw ground check sphere at the correct position
                Vector3 spherePosition = new Vector3(transform.position.x, 
                                                    transform.position.y - (characterController.height / 2f) + groundCheckDistance, 
                                                    transform.position.z);
                
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
                
                // Draw character controller bounds
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(characterController.radius * 2, characterController.height, characterController.radius * 2));
            }
        }
    }
}