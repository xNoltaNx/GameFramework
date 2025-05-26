using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Core.StateMachine;
using GameFramework.Locomotion.States;

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
        
        [Header("Air Control Settings")]
        [SerializeField] private float airControlStrength = 0.5f;
        [SerializeField] private float airMaxSpeed = 8f;
        [SerializeField] private float airAcceleration = 10f;
        [SerializeField] private float airDeceleration = 5f;
        [SerializeField] private float airDrag = 0.98f;
        [SerializeField] private bool allowAirDirectionChange = true;
        [SerializeField] private float directionChangeThreshold = 0.8f;
        
        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask = 1;
        [SerializeField] private float groundCheckRadius = 0.28f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private Transform groundCheck;
        
        [Header("Debug")]
        [SerializeField] private bool debugGravity = false;
        [SerializeField] private bool debugMantle = false;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        
        [Header("Slide Settings")]
        [SerializeField] private float slideSpeed = 12f;
        [SerializeField] private float slideDuration = 1.5f;
        [SerializeField] private float slideDeceleration = 8f;
        [SerializeField] private float minSlideSpeed = 3f;
        
        [Header("Slide Jump Settings")]
        [SerializeField] private float slideJumpForwardBoost = 8f;
        [SerializeField] private float slideJumpHeightMultiplier = 1.2f;
        [SerializeField] private bool maintainSlideDirection = true;
        
        [Header("Mantle Settings")]
        [SerializeField] private float mantleHeight = 2.5f;
        [SerializeField] private float mantleReach = 0.75f;
        [SerializeField] private float mantleDetectionHeight = 0.8f;
        [SerializeField] private float mantleDuration = 0.8f;
        [SerializeField] private float minMantleVelocity = 2f;
        [SerializeField] private LayerMask mantleLayers = 1;
        
        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 velocity;
        private bool isGrounded;
        private bool wasGrounded;
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;
        private bool isCrouching;
        private float currentHeight;
        private bool canSlide = true;
        
        private StateMachine<LocomotionState> stateMachine;
        private StandingState standingState;
        private CrouchingState crouchingState;
        private SlidingState slidingState;
        private JumpingState jumpingState;
        private FallingState fallingState;
        private MantleState mantleState;

        public bool IsGrounded => isGrounded;
        public bool IsMoving => new Vector3(velocity.x, 0f, velocity.z).magnitude > 0.1f;
        public bool IsSprinting { get; set; }
        public bool IsCrouching => isCrouching;
        public bool IsSliding => stateMachine?.IsInState<SlidingState>() ?? false;
        public CharacterController CharacterController => characterController;
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float CrouchSpeed => crouchSpeed;
        public float Deceleration => deceleration;
        public float SlideSpeed => slideSpeed;
        public float SlideDuration => slideDuration;
        public float SlideDeceleration => slideDeceleration;
        public float MinSlideSpeed => minSlideSpeed;
        public float SlideJumpForwardBoost => slideJumpForwardBoost;
        public float SlideJumpHeightMultiplier => slideJumpHeightMultiplier;
        public bool MaintainSlideDirection => maintainSlideDirection;
        public Vector3 Velocity => velocity;
        public bool CanSlide => canSlide;
        public float MantleHeight => mantleHeight;
        public float MantleReach => mantleReach;
        public float MantleDetectionHeight => mantleDetectionHeight;
        public float MantleDuration => mantleDuration;
        public float MinMantleVelocity => minMantleVelocity;
        public LayerMask MantleLayers => mantleLayers;
        public float AirControlStrength => airControlStrength;
        public float AirMaxSpeed => airMaxSpeed;
        public float AirAcceleration => airAcceleration;
        public float AirDrag => airDrag;
        public bool AllowAirDirectionChange => allowAirDirectionChange;
        public float DirectionChangeThreshold => directionChangeThreshold;

        private void Awake()
        {
            ValidateReferences();
            InitializeValues();
            InitializeStateMachine();
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
        
        private void InitializeStateMachine()
        {
            stateMachine = new StateMachine<LocomotionState>();
            
            standingState = new StandingState(this);
            crouchingState = new CrouchingState(this);
            slidingState = new SlidingState(this);
            jumpingState = new JumpingState(this);
            fallingState = new FallingState(this);
            mantleState = new MantleState(this);
            
            stateMachine.RegisterState(standingState);
            stateMachine.RegisterState(crouchingState);
            stateMachine.RegisterState(slidingState);
            stateMachine.RegisterState(jumpingState);
            stateMachine.RegisterState(fallingState);
            stateMachine.RegisterState(mantleState);
            
            stateMachine.ChangeState(standingState);
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
            // Skip ground check and crouch transitions during mantling to ensure atomic movement
            if (!stateMachine.IsInState<MantleState>())
            {
                GroundCheck();
                HandleCrouchTransition();
            }
            
            stateMachine?.Update();
        }

        private void GroundCheck()
        {
            wasGrounded = isGrounded;
            
            Vector3 center = transform.position;
            float checkDistance = (characterController.height / 2f) + groundCheckDistance;
            
            // Primary raycast check straight down with surface normal validation
            bool raycastGrounded = false;
            if (Physics.Raycast(center, Vector3.down, out RaycastHit hit, checkDistance, groundMask))
            {
                // Only consider it ground if the surface is reasonably flat (not a wall)
                float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
                raycastGrounded = surfaceAngle <= 45f; // Allow up to 45 degree slopes
                
                if (debugGravity && !raycastGrounded)
                {
                    Debug.Log($"Surface too steep: {surfaceAngle:F1}° (max 45°)");
                }
            }
            
            // Secondary sphere check at character bottom for edge cases
            Vector3 spherePosition = new Vector3(center.x, center.y - (characterController.height / 2f) + groundCheckDistance, center.z);
            Collider[] hitColliders = Physics.OverlapSphere(spherePosition, groundCheckRadius, groundMask);
            
            bool sphereGrounded = false;
            if (hitColliders.Length > 0)
            {
                // Validate each hit with a raycast to check surface normal
                foreach (var collider in hitColliders)
                {
                    Vector3 directionToCollider = (collider.ClosestPoint(spherePosition) - spherePosition).normalized;
                    if (Physics.Raycast(spherePosition, directionToCollider, out RaycastHit sphereHit, groundCheckRadius + 0.1f, groundMask))
                    {
                        float surfaceAngle = Vector3.Angle(sphereHit.normal, Vector3.up);
                        if (surfaceAngle <= 45f)
                        {
                            sphereGrounded = true;
                            break;
                        }
                    }
                }
            }
            
            isGrounded = raycastGrounded || sphereGrounded;
            
            if (debugGravity)
            {
                Debug.Log($"Ground Check - Raycast: {raycastGrounded}, Sphere: {sphereGrounded}, " +
                         $"Final: {isGrounded}, Velocity.y: {velocity.y}");
            }
        }


        public void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            stateMachine?.CurrentState?.HandleMovement(movementInput, sprintHeld, crouchHeld);
        }

        public void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }

            stateMachine?.CurrentState?.HandleJump(jumpPressed, jumpHeld);
        }

        public void ChangeToStandingState()
        {
            stateMachine?.ChangeState<StandingState>();
        }
        
        public void ChangeToCrouchingState()
        {
            stateMachine?.ChangeState<CrouchingState>();
        }
        
        public void ChangeToSlidingState(Vector2 movementInput)
        {
            stateMachine?.ChangeState<SlidingState>();
            slidingState?.StartSlide(movementInput);
            canSlide = false;
        }
        
        public void ChangeToJumpingState()
        {
            stateMachine?.ChangeState<JumpingState>();
        }
        
        public void ChangeToFallingState()
        {
            stateMachine?.ChangeState<FallingState>();
        }
        
        public void ChangeToMantleState(Vector3 mantleTarget)
        {
            stateMachine?.ChangeState<MantleState>();
            mantleState?.StartMantle(mantleTarget);
        }
        
        public void ChangeToGroundedState()
        {
            if (isCrouching)
            {
                stateMachine?.ChangeState<CrouchingState>();
            }
            else
            {
                stateMachine?.ChangeState<StandingState>();
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

        public void SetCrouching(bool crouching)
        {
            isCrouching = crouching;
        }

        public bool CanStandUp()
        {
            Vector3 capsuleTop = transform.position + Vector3.up * standingHeight;
            return !Physics.CheckSphere(capsuleTop, characterController.radius, groundMask);
        }

        public bool CanJump()
        {
            return jumpTimeoutDelta <= 0.0f && isGrounded;
        }
        
        public void ApplyGravity()
        {
            if (isGrounded && velocity.y < 0.0f)
            {
                velocity.y = -2f;
            }
            else if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }

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
        
        public void HandleAirMovement(Vector2 movementInput)
        {
            Vector3 inputDirection = GetMovementDirection(movementInput);
            Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            
            if (movementInput.magnitude > 0.1f)
            {
                Vector3 targetVelocity = inputDirection * airMaxSpeed;
                
                if (allowAirDirectionChange)
                {
                    float dot = Vector3.Dot(currentHorizontalVelocity.normalized, inputDirection);
                    if (dot < directionChangeThreshold)
                    {
                        Vector3 velocityChange = targetVelocity * airControlStrength * Time.deltaTime * airAcceleration;
                        currentHorizontalVelocity += velocityChange;
                    }
                    else
                    {
                        currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, 
                                                                       airControlStrength * airAcceleration * Time.deltaTime);
                    }
                }
                else
                {
                    currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, 
                                                                   airControlStrength * airAcceleration * Time.deltaTime);
                }
                
                if (currentHorizontalVelocity.magnitude > airMaxSpeed)
                {
                    currentHorizontalVelocity = currentHorizontalVelocity.normalized * airMaxSpeed;
                }
            }
            else
            {
                currentHorizontalVelocity *= airDrag;
            }
            
            velocity = new Vector3(currentHorizontalVelocity.x, velocity.y, currentHorizontalVelocity.z);
            MoveCharacter();
        }
        
        public void ApplyMovement(Vector3 targetVelocity)
        {
            Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float lerpSpeed = targetVelocity.magnitude > currentHorizontalVelocity.magnitude ? acceleration : deceleration;
            
            Vector3 newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, lerpSpeed * Time.deltaTime);
            velocity = new Vector3(newHorizontalVelocity.x, velocity.y, newHorizontalVelocity.z);
            
            MoveCharacter();
        }
        
        public void SetVelocity(Vector3 newVelocity)
        {
            velocity = newVelocity;
        }
        
        public void MoveCharacter()
        {
            if (characterController != null)
            {
                characterController.Move(velocity * Time.deltaTime);
            }
        }

        public void PerformNormalJump()
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpTimeoutDelta = jumpTimeout;
        }

        public void PerformSlideJump(Vector3 slideDirection)
        {
            float enhancedJumpHeight = jumpHeight * slideJumpHeightMultiplier;
            velocity.y = Mathf.Sqrt(enhancedJumpHeight * -2f * gravity);
            
            Vector3 forwardBoost;
            if (maintainSlideDirection && slideDirection != Vector3.zero)
            {
                forwardBoost = slideDirection * slideJumpForwardBoost;
            }
            else
            {
                Vector3 currentHorizontal = new Vector3(velocity.x, 0f, velocity.z);
                if (currentHorizontal.magnitude > 0.1f)
                {
                    forwardBoost = currentHorizontal.normalized * slideJumpForwardBoost;
                }
                else
                {
                    forwardBoost = transform.forward * slideJumpForwardBoost;
                }
            }
            
            velocity.x += forwardBoost.x;
            velocity.z += forwardBoost.z;
            jumpTimeoutDelta = jumpTimeout;
        }


        public Vector3 GetMovementDirection(Vector2 movementInput)
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
        
        public void SetAirControlSettings(float controlStrength, float maxSpeed, float acceleration)
        {
            airControlStrength = Mathf.Clamp01(controlStrength);
            airMaxSpeed = Mathf.Max(0f, maxSpeed);
            airAcceleration = Mathf.Max(0f, acceleration);
        }
        
        public void SetAirControlAdvanced(float drag, bool allowDirectionChange, float directionThreshold)
        {
            airDrag = Mathf.Clamp01(drag);
            allowAirDirectionChange = allowDirectionChange;
            directionChangeThreshold = Mathf.Clamp01(directionThreshold);
        }
        
        public void ResetSlideAvailability()
        {
            canSlide = true;
        }
        
        public bool CanMantle(Vector2 movementInput, out Vector3 mantleTarget)
        {
            mantleTarget = Vector3.zero;
            
            if (isCrouching)
                return false;
            
            if (movementInput.magnitude < 0.1f)
                return false;
                
            Vector3 forwardVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (forwardVelocity.magnitude < minMantleVelocity)
                return false;
            
            // Check if player is trying to move backwards or sideways relative to camera
            if (movementInput.y < -0.1f) // Backpedaling
                return false;
            
            if (Mathf.Abs(movementInput.y) < 0.1f && Mathf.Abs(movementInput.x) > 0.1f) // Pure side strafing
                return false;
            
            Vector3 moveDirection = GetMovementDirection(movementInput);
            Vector3 velocityDirection = forwardVelocity.normalized;
            
            // Ensure player is moving towards the mantle direction (within 45 degrees)
            float directionAlignment = Vector3.Dot(velocityDirection, moveDirection);
            if (directionAlignment < 0.7f) // cos(45°) ≈ 0.7
                return false;
            
            Vector3 detectionStart = transform.position + Vector3.up * mantleDetectionHeight;
            
            if (Physics.Raycast(detectionStart, moveDirection, out RaycastHit wallHit, mantleReach, mantleLayers))
            {
                Vector3 ledgeCheckStart = wallHit.point + Vector3.up * mantleHeight + moveDirection * 0.1f;
                
                if (Physics.Raycast(ledgeCheckStart, Vector3.down, out RaycastHit ledgeHit, mantleHeight + 0.1f, mantleLayers))
                {
                    float mantleHeightCheck = ledgeHit.point.y - transform.position.y;

                    if (mantleHeightCheck > 0.5f && mantleHeightCheck <= mantleHeight)
                    {
                        // Ensure adequate clearance from ledge edge for safe landing
                        float safeDistance = characterController.radius + 0.6f; // Increased clearance
                        Vector3 finalPosition = ledgeHit.point + moveDirection * safeDistance + Vector3.up * (characterController.height / 2f);
                        Vector3 capsuleBottom = finalPosition - Vector3.up * (characterController.height / 2f);
                        Vector3 capsuleTop = finalPosition + Vector3.up * (characterController.height / 2f);
                        
                        Collider[] allBlockingColliders = Physics.OverlapCapsule(capsuleBottom, capsuleTop, characterController.radius, mantleLayers);
                        Collider[] blockingColliders = System.Array.FindAll(allBlockingColliders, c => c != wallHit.collider);
                        
                        if (blockingColliders.Length == 0)
                        {
                            mantleTarget = finalPosition;
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (characterController != null)
            {
                Vector3 center = transform.position;
                float checkDistance = (characterController.height / 2f) + groundCheckDistance;
                
                // Draw primary raycast check
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Vector3 rayEnd = center + Vector3.down * checkDistance;
                Gizmos.DrawLine(center, rayEnd);
                Gizmos.DrawWireSphere(rayEnd, 0.1f);
                
                // Draw sphere check at character bottom
                Vector3 spherePosition = new Vector3(center.x, center.y - (characterController.height / 2f) + groundCheckDistance, center.z);
                Gizmos.color = isGrounded ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
                
                // Draw character controller bounds
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(characterController.radius * 2, characterController.height, characterController.radius * 2));
                
                // Draw mantle debug visualization
                if (debugMantle)
                {
                    DrawMantleDebug();
                }
            }
        }
        
        private void DrawMantleDebug()
        {
            Vector3 detectionStart = transform.position + Vector3.up * mantleDetectionHeight;
            Vector3 forward = transform.forward;
            
            // Draw detection ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(detectionStart, forward * mantleReach);
            Gizmos.DrawWireSphere(detectionStart, 0.1f);
            
            // Draw mantle height range
            Gizmos.color = Color.cyan;
            Vector3 minMantlePos = transform.position + Vector3.up * 0.5f;
            Vector3 maxMantlePos = transform.position + Vector3.up * mantleHeight;
            Gizmos.DrawWireCube(minMantlePos, new Vector3(0.2f, 0.1f, 0.2f));
            Gizmos.DrawWireCube(maxMantlePos, new Vector3(0.2f, 0.1f, 0.2f));
            Gizmos.DrawLine(minMantlePos, maxMantlePos);
            
            // Draw potential mantle area
            Gizmos.color = Color.green;
            Vector3 mantleAreaCenter = detectionStart + forward * mantleReach + Vector3.up * (mantleHeight * 0.5f);
            Gizmos.DrawWireCube(mantleAreaCenter, new Vector3(mantleReach * 2f, mantleHeight, mantleReach * 2f));
        }
    }
}