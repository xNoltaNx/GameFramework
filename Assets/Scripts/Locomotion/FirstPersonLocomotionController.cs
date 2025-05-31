using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Core.StateMachine;
using GameFramework.Core;
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
        
        // Ground check now handled by Unity's CharacterController.isGrounded
        
        [Header("Debug")]
        [SerializeField] private bool debugGravity = false;
        [SerializeField] private bool debugMantle = false;
        [SerializeField] private bool debugStateTransitions = false;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        
        [Header("Slide Settings")]
        [SerializeField] private float slideSpeed = 12f;
        [SerializeField] private float slideDuration = 1.5f;
        [SerializeField] private float slideDeceleration = 8f;
        [SerializeField] private float minSlideSpeed = 3f;
        [Range(0.5f, 1f)]
        [Tooltip("Minimum sprint speed percentage required to initiate a slide (0.85 = 85% of max sprint speed)")]
        [SerializeField] private float slideSpeedThreshold = 0.85f;
        
        [Header("Slide Jump Settings")]
        [SerializeField] private float slideJumpForwardBoost = 8f;
        [SerializeField] private float slideJumpBoostDuration = 1.0f; // Total boost duration
        [SerializeField] private float slideJumpBoostFadeTime = 0.5f; // Time to fade from boost to normal
        [SerializeField] private float slideJumpHeightMultiplier = 1.2f;
        [SerializeField] private bool maintainSlideDirection = true;
        
        [Header("Mantle Settings")]
        [SerializeField] private float mantleHeight = 2.5f;
        [SerializeField] private float mantleReach = 0.75f;
        [SerializeField] private float mantleDetectionHeight = 0.8f;
        [SerializeField] private float mantleDuration = 0.8f;
        [SerializeField] private float minMantleVelocity = 2f;
        [SerializeField] private LayerMask mantleLayers = 1;
        [SerializeField] private EasingSettings mantleEasing = new EasingSettings(EasingType.EaseOutQuart);
        
        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 velocity;
        private float slideJumpBoostTime = 0f;
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;
        private bool isCrouching;
        private float currentHeight;
        private bool canSlide = true;
        
        private StateMachine<LocomotionState> stateMachine;
        private LocomotionStateFactory stateFactory;
        private StateTransitionManager transitionManager;
        
        [Header("Configuration")]
        [SerializeField] private LocomotionConfiguration locomotionConfig;

        public bool IsGrounded => characterController?.isGrounded ?? false;
        public bool IsMoving => new Vector3(velocity.x, 0f, velocity.z).magnitude > (locomotionConfig?.VelocityThreshold ?? 0.1f);
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
        public float SlideSpeedThreshold => slideSpeedThreshold;
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
        public EasingSettings MantleEasing => mantleEasing;
        public float AirControlStrength => airControlStrength;
        public float AirMaxSpeed => airMaxSpeed;
        public float AirAcceleration => airAcceleration;
        public float AirDrag => airDrag;
        public bool AllowAirDirectionChange => allowAirDirectionChange;
        public float DirectionChangeThreshold => directionChangeThreshold;
        
        // Configuration access
        public LocomotionConfiguration Config => locomotionConfig;

        private void Awake()
        {
            ValidateReferences();
            InitializeConfiguration();
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

            // Validate setup on startup
            if (debugGravity && characterController != null)
            {
                Debug.Log($"Character Controller Setup - Height: {characterController.height}, " +
                         $"Radius: {characterController.radius}, Center: {characterController.center}");
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
        
        private void InitializeConfiguration()
        {
            if (locomotionConfig == null)
            {
                locomotionConfig = LocomotionConfiguration.CreateDefault();
                Debug.LogWarning($"No LocomotionConfiguration assigned to {gameObject.name}. Using default configuration.");
            }
        }
        
        private void InitializeStateMachine()
        {
            try
            {
                stateMachine = new StateMachine<LocomotionState>();
                stateFactory = new LocomotionStateFactory();
                
                // Initialize factory with this controller
                stateFactory.InitializeStates(this);
                
                // Register all states with the state machine
                stateMachine.RegisterState(stateFactory.GetState<StandingState>());
                stateMachine.RegisterState(stateFactory.GetState<CrouchingState>());
                stateMachine.RegisterState(stateFactory.GetState<SlidingState>());
                stateMachine.RegisterState(stateFactory.GetState<JumpingState>());
                stateMachine.RegisterState(stateFactory.GetState<FallingState>());
                stateMachine.RegisterState(stateFactory.GetState<MantleState>());
                
                // Initialize transition manager
                transitionManager = new StateTransitionManager(stateMachine, stateFactory, this, locomotionConfig);
                
                // Start in standing state
                stateMachine.ChangeState(stateFactory.GetState<StandingState>());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize state machine for {gameObject.name}: {ex.Message}");
                enabled = false; // Disable the component if initialization fails
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
            try
            {
                // Skip crouch transitions during mantling to ensure atomic movement
                if (!(transitionManager?.IsInState<MantleState>() ?? false))
                {
                    HandleCrouchTransition();
                }
                
                stateMachine?.Update();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in LocomotionController Update for {gameObject.name}: {ex.Message}");
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
            transitionManager?.TryTransitionToState<StandingState>();
        }
        
        public void ChangeToCrouchingState()
        {
            transitionManager?.TryTransitionToState<CrouchingState>();
        }
        
        public void ChangeToSlidingState(Vector2 movementInput)
        {
            if (transitionManager?.TryTransitionToSliding(movementInput) == true)
            {
                canSlide = false;
            }
        }
        
        public void ChangeToJumpingState()
        {
            transitionManager?.TryTransitionToState<JumpingState>();
        }
        
        public void ChangeToFallingState()
        {
            transitionManager?.TryTransitionToState<FallingState>();
        }
        
        public void ChangeToMantleState(Vector3 mantleTarget)
        {
            transitionManager?.TryTransitionToMantle(mantleTarget);
        }
        
        public void ChangeToGroundedState()
        {
            if (debugStateTransitions)
            {
                Debug.Log($"Transitioning to grounded state. IsGrounded: {IsGrounded}, Velocity.y: {velocity.y}, IsCrouching: {isCrouching}");
            }
            
            transitionManager?.TransitionToGroundedState();
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
            return !Physics.CheckSphere(capsuleTop, characterController.radius);
        }

        public bool CanJump()
        {
            bool canJump = jumpTimeoutDelta <= 0.0f && IsGrounded;
            if (debugStateTransitions)
            {
                Debug.Log($"CanJump - Timeout: {jumpTimeoutDelta:F2}, IsGrounded: {IsGrounded}, Result: {canJump}");
            }
            return canJump;
        }
        
        public void ApplyGravity()
        {
            bool grounded = IsGrounded;
            
            if (grounded && velocity.y < 0.0f)
            {
                velocity.y = -2f;
            }
            else if (!grounded)
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
                Debug.Log($"Gravity Applied - IsGrounded: {grounded}, Velocity.y: {velocity.y}, Gravity: {gravity}");
            }
        }
        
        public void HandleAirMovement(Vector2 movementInput)
        {
            // Update slide jump boost timer
            if (slideJumpBoostTime > 0f)
            {
                slideJumpBoostTime -= Time.deltaTime;
            }
            
            Vector3 inputDirection = GetMovementDirection(movementInput);
            Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            
            if (movementInput.magnitude > locomotionConfig.AirControlMinInput)
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
                
                // Handle slide jump boost fade transition
                if (currentHorizontalVelocity.magnitude > airMaxSpeed)
                {
                    if (slideJumpBoostTime > 0f)
                    {
                        // Calculate fade factor - full boost at start, fades to normal over fadeTime
                        float fadeStartTime = slideJumpBoostFadeTime;
                        if (slideJumpBoostTime <= fadeStartTime)
                        {
                            // In fade period - lerp between current speed and airMaxSpeed
                            float fadeProgress = (fadeStartTime - slideJumpBoostTime) / fadeStartTime;
                            float currentSpeed = currentHorizontalVelocity.magnitude;
                            float targetSpeed = Mathf.Lerp(currentSpeed, airMaxSpeed, fadeProgress);
                            
                            if (debugStateTransitions)
                            {
                                Debug.Log($"Slide boost fade - Progress: {fadeProgress:F2}, Speed: {currentSpeed:F1} -> {targetSpeed:F1}");
                            }
                            
                            currentHorizontalVelocity = currentHorizontalVelocity.normalized * targetSpeed;
                        }
                        // Else: still in full boost period, don't clamp
                    }
                    else
                    {
                        // No boost active, apply normal air speed limit
                        currentHorizontalVelocity = currentHorizontalVelocity.normalized * airMaxSpeed;
                    }
                }
            }
            else
            {
                // Apply drag only if velocity is above threshold to prevent jitter
                if (currentHorizontalVelocity.magnitude > locomotionConfig.AirDragThreshold)
                {
                    currentHorizontalVelocity *= airDrag;
                }
                else
                {
                    currentHorizontalVelocity = Vector3.zero;
                }
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
            try
            {
                if (characterController != null)
                {
                    characterController.Move(velocity * Time.deltaTime);
                }
                else
                {
                    Debug.LogWarning($"CharacterController is null on {gameObject.name}. Cannot move character.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error moving character on {gameObject.name}: {ex.Message}");
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
            
            // Set boost window to preserve slide jump velocity with smooth fade
            slideJumpBoostTime = slideJumpBoostDuration;
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
            
            if (movementInput.magnitude < locomotionConfig.MovementInputDeadzone)
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

                    if (mantleHeightCheck > locomotionConfig.MantleMinHeight && mantleHeightCheck <= mantleHeight)
                    {
                        // Ensure adequate clearance from ledge edge for safe landing
                        float safeDistance = characterController.radius + locomotionConfig.MantleSafeClearanceDistance;
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
                // Draw character controller bounds
                Gizmos.color = IsGrounded ? Color.green : Color.red;
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