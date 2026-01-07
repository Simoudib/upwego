using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace UpWeGo
{
    public class EnhancedPlayerMovement : NetworkBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 9f;
        public float crouchSpeed = 2f;
        public float jumpForce = 7f;
        public float gravity = 16f;

        [Header("Carry System")]
        public float carryRadius = 3f;
        public KeyCode carryKey = KeyCode.E;
        public Transform carryPosition; // Where the carried player will be positioned
        public LayerMask playerLayerMask = 1; // Layer mask for detecting other players

        [Header("Toss Settings")]
        public float tossDistance = 26f; // How far to throw (like throwing a ball)
        public float tossHeight = 8f; // Maximum arc height
        public float tossCooldown = 2f; // Cooldown after tossing
        public bool useGravityForToss = true; // Use physics gravity for natural arc

        [Header("Network Smoothing")]
        public float localCarriedLerpSpeed = 15f; // Speed for local player being carried
        public float remoteCarriedLerpSpeed = 12f; // Speed for remote players being carried
        public float carrierUpdateSpeed = 15f; // Speed for carrier updating carried player
        public float predictionStrength = 0.5f; // How much prediction to apply (0-1)

        [Header("Animation")]
        public bool useAnimations = true;
        public string runningAnimationName = "Running"; // Name of your Mixamo running animation
        public string walkingAnimationName = "Walking"; // Name of your Mixamo walking animation
        public string jumpingAnimationName = "Jumping"; // Name of your Mixamo jumping animation
        public string idleAnimationName = "Idle"; // Name of your idle animation (optional)
        public string crouchIdleAnimationName = "CrouchIdle"; // Name of your crouch idle animation
        public string crouchWalkingAnimationName = "CrouchWalking"; // Name of your crouch walking animation
        public string carryIdleAnimationName = "CarryIdle"; // Name of your carry idle animation
        public string carryWalkingAnimationName = "CarryWalking"; // Name of your carry walking animation
        public string beingCarriedIdleAnimationName = "BeingCarriedIdle"; // Name of being carried idle animation
        public string throwingAnimationName = "Throwing"; // Name of your throwing animation
        
        [Header("Crouch Settings")]
        public KeyCode crouchKey = KeyCode.LeftControl;
        public float standingHeight = 2f;
        public float crouchingHeight = 1f;
        public float standingRadius = 0.5f;
        public float crouchingRadius = 0.5f;
        
        [Header("Player Identity")]
        //[SyncVar(hook = nameof(OnPlayerNameChanged))]
        public string playerDisplayName = "";
        
        [Header("Debug")]
        public bool showCarryRadius = true;
        public bool debugTossPhysics = true;

        private CharacterController controller;
        private Vector3 velocity = Vector3.zero;
        private Animator animator;
        
        // Animation state tracking
        private bool lastIsRunning = false;
        private bool lastIsMoving = false;
        private bool lastIsJumping = false;
        private bool lastIsGrounded = true;
        private bool lastIsCrouching = false;
        private bool lastIsCarrying = false;
        private bool lastIsBeingCarried = false;
        private bool lastIsThrowing = false;
        private float lastLandingTime = 0f;
        
        // Crouch system
        private bool isCrouching = false;
        private float originalHeight;
        private float originalRadius;
        private Vector3 originalCenter;

        // Carry system variables
        [SyncVar] private bool isBeingCarried = false;
        [SyncVar] private uint carrierNetId = 0; // NetworkInstanceId of the player carrying us
        [SyncVar] private uint carriedPlayerNetId = 0; // NetworkInstanceId of the player we're carrying

        // Network sync for position when being carried
        [SyncVar(hook = nameof(OnCarryPositionChanged))] private Vector3 networkCarryPosition;
        [SyncVar(hook = nameof(OnCarryRotationChanged))] private Quaternion networkCarryRotation;
        
        // Prediction and interpolation for smooth carry movement
        private Vector3 predictedCarryPosition;
        private Quaternion predictedCarryRotation;
        private Vector3 carrierVelocity;
        private Vector3 lastCarrierPosition;
        private float lastCarryUpdateTime;
        private float lastTossTime = 0f; // Track toss cooldown
        
        // Toss state management
        private bool isBeingTossed = false;
        private float tossStartTime = 0f;
        private float tossDuration = 3f; // How long toss physics should last

        private EnhancedPlayerMovement carriedPlayer; // Reference to the player we're carrying
        private EnhancedPlayerMovement carrier; // Reference to the player carrying us
        
        // Component management for conflict prevention
        private NetworkTransformBase carriedPlayerNetworkTransform;
        private Rigidbody carriedPlayerRigidbody;
        private bool wasRigidbodyKinematic = false;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // Store original controller dimensions for crouch system
            if (controller != null)
            {
                originalHeight = controller.height;
                originalRadius = controller.radius;
                originalCenter = controller.center;
                
                // Set default values if not configured
                if (standingHeight == 0) standingHeight = originalHeight;
                if (standingRadius == 0) standingRadius = originalRadius;
            }
            
            // Try to find Animator component (on this GameObject or in children)
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    Debug.Log($"🔍 Found Animator on child GameObject: {animator.gameObject.name}");
                }
            }
            else
            {
                Debug.Log($"🔍 Found Animator on main GameObject: {animator.gameObject.name}");
            }
            
            // Auto-create carry position if not assigned
            if (carryPosition == null)
            {
                GameObject carryPos = new GameObject("CarryPosition");
                carryPos.transform.SetParent(transform);
                carryPos.transform.localPosition = new Vector3(0f, 1.5f, 1f); // Above and in front
                carryPosition = carryPos.transform;
            }
            
            // Check if animator is set up correctly
            if (useAnimations && animator == null)
            {
                Debug.LogWarning($"⚠️ {gameObject.name}: useAnimations is enabled but no Animator component found! Please add an Animator component to this GameObject or its children.");
            }
            else if (useAnimations && animator != null)
            {
                Debug.Log($"✅ {gameObject.name}: Animator found and ready for animations!");
                Debug.Log($"🎯 Animator is on GameObject: {animator.gameObject.name}");
                Debug.Log($"🎮 Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL - PLEASE ASSIGN CONTROLLER!")}");
                Debug.Log($"👤 Avatar: {(animator.avatar != null ? animator.avatar.name : "NULL - AVATAR MISSING!")}");
                
                // Check avatar configuration
                if (animator.avatar == null)
                {
                    Debug.LogWarning("⚠️ No Avatar assigned! For humanoid animations, you need to assign an Avatar to the Animator.");
                }
                
                // Check if the animator has the required parameters
                bool hasIsRunning = false;
                bool hasSpeed = false;
                bool hasIsJumping = false;
                bool hasIsGrounded = false;
                bool hasIsCrouching = false;
                bool hasIsCarrying = false;
                bool hasIsBeingCarried = false;
                bool hasIsThrowing = false;
                for (int i = 0; i < animator.parameterCount; i++)
                {
                    AnimatorControllerParameter param = animator.GetParameter(i);
                    if (param.name == "IsRunning") hasIsRunning = true;
                    if (param.name == "Speed") hasSpeed = true;
                    if (param.name == "IsJumping") hasIsJumping = true;
                    if (param.name == "IsGrounded") hasIsGrounded = true;
                    if (param.name == "IsCrouching") hasIsCrouching = true;
                    if (param.name == "IsCarrying") hasIsCarrying = true;
                    if (param.name == "IsBeingCarried") hasIsBeingCarried = true;
                    if (param.name == "IsThrowing") hasIsThrowing = true;
                    Debug.Log($"📊 Animator Parameter: {param.name} ({param.type})");
                }
                
                if (!hasIsRunning) Debug.LogError("❌ Missing 'IsRunning' parameter in Animator Controller!");
                if (!hasSpeed) Debug.LogError("❌ Missing 'Speed' parameter in Animator Controller!");
                if (!hasIsJumping) Debug.LogError("❌ Missing 'IsJumping' parameter in Animator Controller!");
                if (!hasIsGrounded) Debug.LogError("❌ Missing 'IsGrounded' parameter in Animator Controller!");
                if (!hasIsCrouching) Debug.LogError("❌ Missing 'IsCrouching' parameter in Animator Controller!");
                if (!hasIsCarrying) Debug.LogError("❌ Missing 'IsCarrying' parameter in Animator Controller!");
                if (!hasIsBeingCarried) Debug.LogError("❌ Missing 'IsBeingCarried' parameter in Animator Controller!");
                if (!hasIsThrowing) Debug.LogError("❌ Missing 'IsThrowing' parameter in Animator Controller!");
            }
            
            // Initialize player name on server
            if (isServer && string.IsNullOrEmpty(playerDisplayName))
            {
                // Try to get Steam name first, fallback to connection ID
                string steamName = GetSteamPlayerName();
                if (!string.IsNullOrEmpty(steamName))
                {
                    playerDisplayName = steamName;
                    Debug.Log($"🏷️ Using Steam name: {playerDisplayName}");
                }
                else
                {
                    // Fallback to connection ID if Steam name not available
                    string defaultName = $"Player {netId}";
                    if (connectionToClient != null)
                    {
                        defaultName = $"Player {connectionToClient.connectionId}";
                    }
                    playerDisplayName = defaultName;
                    Debug.Log($"🏷️ Using fallback name: {playerDisplayName}");
                }
            }
            
            // Setup name display component
            SetupNameDisplay();
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            // Handle carry input
            HandleCarryInput();

            // Handle toss physics state first
            if (isBeingTossed)
            {
                HandleTossPhysics();
                return; // Don't process other movement while being tossed
            }

            // Only handle movement if not being carried
            if (!isBeingCarried)
            {
                HandleMovement();
            }
            else
            {
                HandleBeingCarried();
            }
        }

        void HandleMovement()
        {
            // Don't handle movement if being tossed
            if (isBeingTossed) return;

            // Ensure CharacterController is enabled for normal movement
            if (controller != null && !controller.enabled)
            {
                controller.enabled = true;
            }

            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            bool jump = Input.GetButtonDown("Jump");
            bool crouchInput = Input.GetKey(crouchKey);
            
            // Clean debug - only essential info
            
            // Check if we're carrying someone (restrict actions)
            bool isCarryingSomeone = (carriedPlayerNetId != 0);
            
            // Apply carry restrictions
            if (isCarryingSomeone)
            {
                isRunning = false; // Can't sprint while carrying
                jump = false; // Can't jump while carrying
                
                if (jump && Input.GetButtonDown("Jump"))
                {
                    Debug.Log("❌ Can't jump while carrying someone!");
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log("❌ Can't sprint while carrying someone!");
                }
                if (Input.GetKey(crouchKey))
                {
                    Debug.Log("❌ Can't crouch while carrying someone!");
                }
            }
            
            // Handle crouching - always call it, let it handle restrictions internally
            HandleCrouching(crouchInput);
            
            // Minimal input debug

            // Check if grounded
            bool isGrounded = controller.isGrounded;

            // Handle jumping (works while standing still!)
            if (isGrounded)
            {
                // Reset vertical velocity when grounded
                if (velocity.y < 0)
                {
                    velocity.y = -2f; // Small downward force to keep grounded
                }

                // Jump regardless of horizontal movement
                if (jump)
                {
                    velocity.y = jumpForce;
                    Debug.Log("🦘 Jump triggered!");
                }
            }

            // Handle horizontal movement relative to camera/player facing direction
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool isMoving = inputDirection.magnitude >= 0.1f;
            
            if (isMoving)
            {
                // Choose speed based on movement state (crouch overrides running)
                float currentSpeed;
                if (isCrouching)
                {
                    currentSpeed = crouchSpeed; // Crouching is slowest
                }
                else if (isRunning)
                {
                    currentSpeed = runSpeed; // Running is fastest
                }
                else
                {
                    currentSpeed = walkSpeed; // Walking is normal speed
                }
                
                // Get camera forward and right directions (player is already rotated by camera)
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                
                // Remove Y component to keep movement on horizontal plane
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                
                // Calculate movement direction based on camera orientation
                Vector3 moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;
                
                // Apply movement (no rotation needed since camera already rotates the player)
                Vector3 horizontalMovement = moveDirection * currentSpeed;
                controller.Move(horizontalMovement * Time.deltaTime);
            }
            
            // Update animations
            UpdateAnimations(isMoving, isRunning, isGrounded, jump, isCrouching, isCarryingSomeone, isBeingCarried);
            
            // Sync animations across network
            if (isLocalPlayer)
            {
                CmdSyncAnimations(isMoving, isRunning, isGrounded, jump, isCrouching, isCarryingSomeone, isBeingCarried);
            }

            // Apply gravity
            velocity.y -= gravity * Time.deltaTime;

            // Apply vertical movement (gravity and jumping)
            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

            // Update carried player position if we're carrying someone
            if (carriedPlayer != null && carriedPlayerNetId != 0)
            {
                UpdateCarriedPlayerPosition();
            }
        }

        void HandleBeingCarried()
        {
            // All conflicting components are disabled, so we have full control
            // Don't process input while being carried
            if (carrier != null && carrier.carryPosition != null)
            {
                if (isLocalPlayer)
                {
                    // For local player being carried - INSTANT position (no fighting with other systems)
                    transform.position = carrier.carryPosition.position;
                    transform.rotation = carrier.carryPosition.rotation;
                }
                else
                {
                    // For remote players, use smooth interpolation
                    Vector3 targetPosition = GetPredictedCarryPosition();
                    Quaternion targetRotation = predictedCarryRotation;
                    
                    transform.position = Vector3.Lerp(transform.position, targetPosition, 25f * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 25f * Time.deltaTime);
                }
            }
            else if (!isLocalPlayer && networkCarryPosition != Vector3.zero)
            {
                // Fallback to network position if no carrier reference
                transform.position = Vector3.Lerp(transform.position, networkCarryPosition, 20f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkCarryRotation, 20f * Time.deltaTime);
            }
        }

        void HandleTossPhysics()
        {
            // Check if toss duration has elapsed
            if (Time.time - tossStartTime > tossDuration)
            {
                isBeingTossed = false;
                
                // Ensure CharacterController is re-enabled
                if (controller != null && !controller.enabled)
                {
                    controller.enabled = true;
                }
                
                Debug.Log("🏁 Toss physics ended, resuming normal movement");
                return;
            }

            // Apply gravity manually during toss
            velocity.y -= gravity * Time.deltaTime;

            // Move the character using velocity (pure physics)
            controller.Move(velocity * Time.deltaTime);

            // Check if we hit the ground
            if (controller.isGrounded && velocity.y <= 0)
            {
                isBeingTossed = false;
                velocity.y = 0; // Stop vertical movement
                
                // Ensure CharacterController is re-enabled for normal movement
                if (controller != null && !controller.enabled)
                {
                    controller.enabled = true;
                }
                
                Debug.Log("🎯 Tossed player landed!");
            }

            if (debugTossPhysics)
            {
                Debug.Log($"🚀 Toss physics: velocity={velocity}, grounded={controller.isGrounded}");
            }
        }

        void HandleCarryInput()
        {
            if (Input.GetKeyDown(carryKey))
            {
                if (isBeingCarried)
                {
                    // Can't carry while being carried
                    Debug.Log("❌ Can't carry while being carried!");
                    return;
                }

                if (carriedPlayerNetId != 0)
                {
                    // We're carrying someone - toss them (with cooldown check)
                    if (Time.time - lastTossTime >= tossCooldown)
                    {
                        // Start throw animation
                        if (animator != null)
                        {
                            animator.SetBool("IsThrowing", true);
                            Debug.Log("🎯 Starting throw animation!");
                        }
                        
                        CmdTossPlayer();
                    }
                    else
                    {
                        float remaining = tossCooldown - (Time.time - lastTossTime);
                        Debug.Log($"⏰ Toss on cooldown! {remaining:F1}s remaining");
                    }
                }
                else
                {
                    // Try to pick up a nearby player
                    EnhancedPlayerMovement nearestPlayer = FindNearestCarriablePlayer();
                    if (nearestPlayer != null)
                    {
                        CmdCarryPlayer(nearestPlayer.netId);
                    }
                    else
                    {
                        Debug.Log("💭 No players in range to carry");
                    }
                }
            }
        }

        EnhancedPlayerMovement FindNearestCarriablePlayer()
        {
            EnhancedPlayerMovement[] allPlayers = FindObjectsOfType<EnhancedPlayerMovement>();
            EnhancedPlayerMovement nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var player in allPlayers)
            {
                // Skip ourselves
                if (player == this) continue;
                
                // Skip if already being carried
                if (player.isBeingCarried) continue;
                
                // Skip if this player is already carrying someone (optional - remove if you want chain carrying)
                if (player.carriedPlayerNetId != 0) continue;

                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= carryRadius && distance < nearestDistance)
                {
                    nearest = player;
                    nearestDistance = distance;
                }
            }

            if (nearest != null)
            {
                Debug.Log($"🎯 Found nearest player: {nearest.name} at distance {nearestDistance:F1}");
            }

            return nearest;
        }

        [Command]
        void CmdCarryPlayer(uint targetNetId)
        {
            if (carriedPlayerNetId != 0)
            {
                Debug.LogWarning("Already carrying a player!");
                return;
            }

            // Find the target player
            if (NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity targetIdentity))
            {
                EnhancedPlayerMovement targetPlayer = targetIdentity.GetComponent<EnhancedPlayerMovement>();
                if (targetPlayer != null && !targetPlayer.isBeingCarried)
                {
                    // Set up carry relationship
                    carriedPlayerNetId = targetNetId;
                    targetPlayer.isBeingCarried = true;
                    targetPlayer.carrierNetId = netId;

                    // Update references on server
                    carriedPlayer = targetPlayer;
                    targetPlayer.carrier = this;

                    Debug.Log($"✅ {gameObject.name} is now carrying {targetPlayer.gameObject.name}");
                    
                    // Call RPC to update clients
                    RpcOnPlayerCarried(netId, targetNetId);
                }
            }
        }

        [Command]
        void CmdTossPlayer()
        {
            if (carriedPlayerNetId == 0)
            {
                Debug.LogWarning("Not carrying anyone to toss!");
                return;
            }

            // Check toss cooldown
            if (Time.time - lastTossTime < tossCooldown)
            {
                Debug.LogWarning("Toss on cooldown!");
                return;
            }

            // Find the carried player
            if (NetworkServer.spawned.TryGetValue(carriedPlayerNetId, out NetworkIdentity carriedIdentity))
            {
                EnhancedPlayerMovement carriedPlayerComponent = carriedIdentity.GetComponent<EnhancedPlayerMovement>();
                if (carriedPlayerComponent != null)
                {
                    // Calculate proper projectile motion for ball-like throwing
                    Vector3 tossVelocity = CalculateProjectileVelocity(
                        transform.position, 
                        transform.position + transform.forward * tossDistance,
                        tossHeight
                    );

                    // Position the player slightly in front before applying velocity
                    Vector3 tossPosition = transform.position + transform.forward * 2f + Vector3.up * 0.5f;
                    carriedPlayerComponent.transform.position = tossPosition;

                    // Apply toss velocity to carried player
                    carriedPlayerComponent.velocity = tossVelocity;

                    // Activate toss physics state
                    carriedPlayerComponent.isBeingTossed = true;
                    carriedPlayerComponent.tossStartTime = Time.time;

                    // Clear carry relationship
                    carriedPlayerComponent.isBeingCarried = false;
                    carriedPlayerComponent.carrierNetId = 0;
                    carriedPlayerComponent.carrier = null;

                    // Restore all components for normal movement
                    RestoreCarriedPlayerComponents(carriedPlayerComponent);

                    // Clear our reference
                    carriedPlayerNetId = 0;
                    carriedPlayer = null;
                    lastTossTime = Time.time;

                    Debug.Log($"🚀 {gameObject.name} tossed {carriedPlayerComponent.gameObject.name} with force: {tossVelocity}");
                    
                    // Call RPC to update clients with toss velocity
                    RpcOnPlayerTossed(netId, carriedIdentity.netId, tossVelocity, tossPosition);
                }
            }
        }

        [ClientRpc]
        void RpcOnPlayerCarried(uint carrierNetId, uint carriedNetId)
        {
            Debug.Log($"🎒 RPC: Player carried - Carrier: {carrierNetId}, Carried: {carriedNetId}");
            
            // Update local references for all clients
            if (NetworkClient.spawned.TryGetValue(carrierNetId, out NetworkIdentity carrierIdentity) &&
                NetworkClient.spawned.TryGetValue(carriedNetId, out NetworkIdentity carriedIdentity))
            {
                EnhancedPlayerMovement carrierPlayer = carrierIdentity.GetComponent<EnhancedPlayerMovement>();
                EnhancedPlayerMovement carriedPlayerComponent = carriedIdentity.GetComponent<EnhancedPlayerMovement>();

                if (carrierPlayer != null && carriedPlayerComponent != null)
                {
                    carrierPlayer.carriedPlayer = carriedPlayerComponent;
                    carriedPlayerComponent.carrier = carrierPlayer;

                    // Completely disable all movement systems on carried player
                    DisableCarriedPlayerComponents(carriedPlayerComponent);
                }
            }
        }

        [ClientRpc]
        void RpcOnPlayerTossed(uint carrierNetId, uint carriedNetId, Vector3 tossVelocity, Vector3 tossPosition)
        {
            Debug.Log($"🚀 RPC: Player tossed - Carrier: {carrierNetId}, Carried: {carriedNetId}, Velocity: {tossVelocity}");
            
            // Clear local references for all clients
            if (NetworkClient.spawned.TryGetValue(carrierNetId, out NetworkIdentity carrierIdentity) &&
                NetworkClient.spawned.TryGetValue(carriedNetId, out NetworkIdentity carriedIdentity))
            {
                EnhancedPlayerMovement carrierPlayer = carrierIdentity.GetComponent<EnhancedPlayerMovement>();
                EnhancedPlayerMovement carriedPlayerComponent = carriedIdentity.GetComponent<EnhancedPlayerMovement>();

                if (carrierPlayer != null && carriedPlayerComponent != null)
                {
                    // Clear references
                    carrierPlayer.carriedPlayer = null;
                    carriedPlayerComponent.carrier = null;

                    // Restore all components on all clients
                    RestoreCarriedPlayerComponents(carriedPlayerComponent);

                    // Apply toss effects on all clients
                    carriedPlayerComponent.transform.position = tossPosition;
                    carriedPlayerComponent.velocity = tossVelocity;
                    
                    // Activate toss physics state on all clients
                    carriedPlayerComponent.isBeingTossed = true;
                    carriedPlayerComponent.tossStartTime = Time.time;
                    
                    // Start jump animation for thrown player (they're being thrown through the air)
                    if (carriedPlayerComponent.animator != null)
                    {
                        carriedPlayerComponent.animator.SetBool("IsJumping", true);
                        carriedPlayerComponent.animator.SetBool("IsGrounded", false);
                        carriedPlayerComponent.animator.SetBool("IsBeingCarried", false);
                        Debug.Log("🎯 Thrown player: Starting jump animation!");
                    }
                    
                    Debug.Log($"✅ Applied toss: Position={tossPosition}, Velocity={tossVelocity}");
                }
                
                // If this is the carrier, reset throw animation after delay
                if (carrierPlayer != null && carrierPlayer.netId == carrierNetId)
                {
                    if (carrierPlayer.animator != null)
                    {
                        carrierPlayer.StartCoroutine(carrierPlayer.ResetThrowAnimationAfterDelay(0.8f));
                    }
                }
            }
        }

        [ClientRpc]
        void RpcUpdateCarryPosition(Vector3 newPosition, Quaternion newRotation)
        {
            // Update the carried player's position for all clients with high frequency
            if (carriedPlayer != null && !isLocalPlayer) // Don't override local player's position
            {
                predictedCarryPosition = newPosition;
                predictedCarryRotation = newRotation;
            }
        }

        void UpdateCarriedPlayerPosition()
        {
            if (carriedPlayer != null && carryPosition != null)
            {
                // Update network carry position for synchronization
                if (isServer)
                {
                    networkCarryPosition = carryPosition.position;
                    networkCarryRotation = carryPosition.rotation;
                }

                // For the carrier, update the carried player IMMEDIATELY (no lerp for zero lag)
                if (isLocalPlayer)
                {
                    // INSTANT position update - no lag, no interpolation
                    carriedPlayer.transform.position = carryPosition.position;
                    carriedPlayer.transform.rotation = carryPosition.rotation;

                    // Update velocity for prediction
                    UpdateCarrierVelocity();

                    // Send ultra-high-frequency position updates to all clients
                    if (Time.time - lastCarryUpdateTime > 0.01f) // 100Hz updates for ultra-smooth movement
                    {
                        RpcUpdateCarryPosition(carryPosition.position, carryPosition.rotation);
                        lastCarryUpdateTime = Time.time;
                    }
                }
            }
        }

        void UpdateCarrierVelocity()
        {
            if (carrier != null && Time.time > lastCarryUpdateTime)
            {
                // Calculate carrier velocity for prediction
                float deltaTime = Time.time - lastCarryUpdateTime;
                if (deltaTime > 0)
                {
                    carrierVelocity = (carrier.transform.position - lastCarrierPosition) / deltaTime;
                    lastCarrierPosition = carrier.transform.position;
                    lastCarryUpdateTime = Time.time;
                }
            }
        }

        Vector3 GetPredictedCarryPosition()
        {
            if (carrier == null) return networkCarryPosition;

            // Use carrier's current position plus prediction based on velocity
            float timeSinceLastUpdate = Time.time - lastCarryUpdateTime;
            Vector3 predictedCarrierPos = carrier.transform.position + (carrierVelocity * timeSinceLastUpdate * predictionStrength);
            
            // Calculate relative position from carrier to carry position
            Vector3 relativeCarryPos = carrier.carryPosition.position - carrier.transform.position;
            
            return predictedCarrierPos + relativeCarryPos;
        }

        // SyncVar hooks for smooth interpolation
        void OnCarryPositionChanged(Vector3 oldPos, Vector3 newPos)
        {
            if (!isLocalPlayer && isBeingCarried)
            {
                predictedCarryPosition = newPos;
            }
        }

        void OnCarryRotationChanged(Quaternion oldRot, Quaternion newRot)
        {
            if (!isLocalPlayer && isBeingCarried)
            {
                predictedCarryRotation = newRot;
            }
        }

        // Disable all components that could interfere with carrying
        void DisableCarriedPlayerComponents(EnhancedPlayerMovement carriedPlayerComponent)
        {
            // Disable CharacterController
            if (carriedPlayerComponent.controller != null)
            {
                carriedPlayerComponent.controller.enabled = false;
            }

            // Disable NetworkTransform to prevent position conflicts
            NetworkTransformBase networkTransform = carriedPlayerComponent.GetComponent<NetworkTransformBase>();
            if (networkTransform != null)
            {
                networkTransform.enabled = false;
                carriedPlayerNetworkTransform = networkTransform;
                Debug.Log("🚫 Disabled NetworkTransform to prevent position conflicts");
            }

            // Make Rigidbody kinematic to prevent physics interference
            Rigidbody rb = carriedPlayerComponent.GetComponent<Rigidbody>();
            if (rb != null)
            {
                wasRigidbodyKinematic = rb.isKinematic;
                rb.isKinematic = true;
                carriedPlayerRigidbody = rb;
                Debug.Log("🚫 Made Rigidbody kinematic to prevent physics conflicts");
            }

            Debug.Log($"✅ Disabled all conflicting components on {carriedPlayerComponent.name}");
        }

        // Re-enable all components when carry ends
        void RestoreCarriedPlayerComponents(EnhancedPlayerMovement carriedPlayerComponent)
        {
            // Re-enable CharacterController
            if (carriedPlayerComponent.controller != null)
            {
                carriedPlayerComponent.controller.enabled = true;
            }

            // Re-enable NetworkTransform
            if (carriedPlayerNetworkTransform != null)
            {
                carriedPlayerNetworkTransform.enabled = true;
                carriedPlayerNetworkTransform = null;
                Debug.Log("✅ Re-enabled NetworkTransform");
            }

            // Restore Rigidbody kinematic state
            if (carriedPlayerRigidbody != null)
            {
                carriedPlayerRigidbody.isKinematic = wasRigidbodyKinematic;
                carriedPlayerRigidbody = null;
                Debug.Log("✅ Restored Rigidbody kinematic state");
            }

            Debug.Log($"✅ Restored all components on {carriedPlayerComponent.name}");
        }

        void HandleCrouching(bool crouchInput)
        {
            // Check if we can crouch (not carrying someone)
            bool isCarryingSomeone = (carriedPlayerNetId != 0);
            
            // Test multiple ways to detect Ctrl key
            bool leftCtrl = Input.GetKey(KeyCode.LeftControl);
            bool rightCtrl = Input.GetKey(KeyCode.RightControl);
            bool anyCtrl = leftCtrl || rightCtrl;
            
            // Also test the crouchKey variable
            bool crouchKeyPressed = Input.GetKey(crouchKey);
            
            // Use any Ctrl key detection
            bool shouldCrouch = anyCtrl && !isCarryingSomeone;
            bool wasCrouching = isCrouching;
            isCrouching = shouldCrouch;
            
            // Debug crouch state when standing still  
            bool hasMovementInput = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
            if (anyCtrl && !hasMovementInput)
            {
                Debug.Log($"🔍 Standing Still + Ctrl - Crouching: {isCrouching}, Moving: {hasMovementInput}, Should go to CrouchIdle");
            }
            
            // Only log state changes
            if (wasCrouching != isCrouching)
            {
                Debug.Log($"🔍 Crouch State Changed - Now: {isCrouching}");
            }
            
            // Only update controller if crouch state changed
            if (wasCrouching != isCrouching)
            {
                UpdateControllerHeight();
                
                if (isCrouching)
                {
                    Debug.Log($"🦆 Started crouching - Height: {controller.height}, Radius: {controller.radius}");
                }
                else
                {
                    Debug.Log($"🧍 Stopped crouching - Height: {controller.height}, Radius: {controller.radius}");
                }
            }
        }

        void UpdateControllerHeight()
        {
            if (controller == null) return;
            
            if (isCrouching)
            {
                // Switch to crouching dimensions
                controller.height = crouchingHeight;
                controller.radius = crouchingRadius;
                
                // Adjust center to keep feet on ground
                // When crouching, the center moves down by half the height difference
                float heightDifference = standingHeight - crouchingHeight;
                controller.center = new Vector3(originalCenter.x, originalCenter.y - heightDifference * 0.5f, originalCenter.z);
            }
            else
            {
                // Check if there's enough space to stand up
                if (CanStandUp())
                {
                    // Switch back to standing dimensions
                    controller.height = standingHeight;
                    controller.radius = standingRadius;
                    controller.center = originalCenter;
                }
                else
                {
                    // Force stay crouched if there's not enough space
                    isCrouching = true;
                    Debug.Log("⚠️ Can't stand up - not enough space above!");
                }
            }
        }

        bool CanStandUp()
        {
            if (controller == null) return true;
            
            // Cast a sphere upward to check for obstacles
            Vector3 currentTop = transform.position + Vector3.up * (crouchingHeight * 0.5f);
            Vector3 standingTop = transform.position + Vector3.up * (standingHeight * 0.5f);
            
            float checkDistance = standingHeight - crouchingHeight;
            
            // Use SphereCast to check if there's space to stand up
            return !Physics.SphereCast(currentTop, controller.radius * 0.9f, Vector3.up, out RaycastHit hit, checkDistance);
        }

        System.Collections.IEnumerator ResetThrowAnimationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animator != null)
            {
                animator.SetBool("IsThrowing", false);
                Debug.Log($"🎯 Throw animation completed - resetting to normal state");
                Debug.Log($"🔍 Post-throw state: carriedPlayer={carriedPlayer != null}, carriedPlayerNetId={carriedPlayerNetId}, IsCarrying={IsCarrying}");
            }
        }

        [Command]
        void CmdSyncAnimations(bool isMoving, bool isRunning, bool isGrounded, bool jump, bool isCrouching, bool isCarrying, bool isBeingCarried)
        {
            // Send animation state to all clients
            RpcSyncAnimations(isMoving, isRunning, isGrounded, jump, isCrouching, isCarrying, isBeingCarried);
        }

        [ClientRpc]
        void RpcSyncAnimations(bool isMoving, bool isRunning, bool isGrounded, bool jump, bool isCrouching, bool isCarrying, bool isBeingCarried)
        {
            // Don't override local player's animations
            if (isLocalPlayer) return;
            
            // Update animations for other players
            UpdateAnimations(isMoving, isRunning, isGrounded, jump, isCrouching, isCarrying, isBeingCarried);
        }

        void UpdateAnimations(bool isMoving, bool isRunning, bool isGrounded, bool jumpPressed, bool crouching, bool carrying, bool beingCarried)
        {
            if (!useAnimations || animator == null) return;

            // Check current throwing state
            bool isThrowing = animator.GetBool("IsThrowing");

            // Check if state changed to only log when changing
            bool stateChanged = (isRunning != lastIsRunning) || (isMoving != lastIsMoving) || 
                              (isGrounded != lastIsGrounded) || jumpPressed || (crouching != lastIsCrouching) ||
                              (carrying != lastIsCarrying) || (beingCarried != lastIsBeingCarried) || 
                              (isThrowing != lastIsThrowing);

            // Handle jumping first (highest priority)
            if (jumpPressed && isGrounded)
            {
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsGrounded", false);
                if (stateChanged) Debug.Log($"🦘 Setting animation: JUMPING!");
            }
            else if (!isGrounded)
            {
                // Still in air
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsGrounded", false);
            }
            else if (isGrounded && !lastIsGrounded && Time.time - lastLandingTime > 0.5f)
            {
                // Just landed - with cooldown to prevent spam
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsGrounded", true);
                lastLandingTime = Time.time;
                Debug.Log($"🎯 Player landed! Resetting jump state.");
                
                // Clear any throwing state when landing
                if (animator.GetBool("IsThrowing"))
                {
                    animator.SetBool("IsThrowing", false);
                    Debug.Log($"🎯 Cleared throwing state on landing");
                }
            }
            else
            {
                // Normal grounded movement
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsGrounded", true);
                
                // Set all state parameters
                animator.SetBool("IsCrouching", crouching);
                animator.SetBool("IsCarrying", carrying);
                animator.SetBool("IsBeingCarried", beingCarried);
                
                if (isThrowing)
                {
                    // Throwing state (highest priority for active actions)
                    animator.SetBool("IsRunning", false);
                    animator.SetFloat("Speed", 0f);
                    if (stateChanged) Debug.Log($"🎯 Setting animation: THROWING");
                }
                else if (beingCarried)
                {
                    // Being carried state (second highest priority - only idle animation)
                    animator.SetBool("IsRunning", false);
                    animator.SetFloat("Speed", 0f);
                    if (stateChanged) Debug.Log($"🎒 Setting animation: BEING CARRIED IDLE");
                }
                else if (carrying)
                {
                    // Carrying state (third highest priority)
                    animator.SetBool("IsRunning", false);
                    
                    // Debug movement input while carrying
                    Debug.Log($"🎒 CARRY INPUT CHECK - isMoving: {isMoving}, H: {Input.GetAxis("Horizontal"):F2}, V: {Input.GetAxis("Vertical"):F2}");
                    if (isMoving)
                    {
                        animator.SetFloat("Speed", walkSpeed); // Carry walking speed
                        
                        // Detailed debugging for carry walking
                        var currentState = animator.GetCurrentAnimatorStateInfo(0);
                        string currentStateName = currentState.IsName("CarryIdle") ? "CarryIdle" : 
                                          currentState.IsName("CarryWalking") ? "CarryWalking" : 
                                          currentState.IsName("Idle") ? "Idle" : 
                                          currentState.IsName("Walking") ? "Walking" : 
                                          currentState.IsName("Running") ? "Running" : 
                                          $"Unknown({currentState.nameHash})";
                        Debug.Log($"🤝 CARRY WALKING DEBUG:");
                        Debug.Log($"   - Current State: {currentStateName} (Hash: {currentState.nameHash})");
                        Debug.Log($"   - IsCarrying: {animator.GetBool("IsCarrying")}");
                        Debug.Log($"   - Speed: {animator.GetFloat("Speed")} (Set to: {walkSpeed})");
                        Debug.Log($"   - IsMoving: {isMoving}");
                        Debug.Log($"   - StateChanged: {stateChanged}");
                    }
                    else
                    {
                        animator.SetFloat("Speed", 0f);
                        
                        // Detailed debugging for carry idle
                        var currentState = animator.GetCurrentAnimatorStateInfo(0);
                        string currentStateName = currentState.IsName("CarryIdle") ? "CarryIdle" : 
                                          currentState.IsName("CarryWalking") ? "CarryWalking" : 
                                          currentState.IsName("Idle") ? "Idle" : 
                                          currentState.IsName("Walking") ? "Walking" : 
                                          currentState.IsName("Running") ? "Running" : 
                                          $"Unknown({currentState.nameHash})";
                        Debug.Log($"🤝 CARRY IDLE DEBUG:");
                        Debug.Log($"   - Current State: {currentStateName} (Hash: {currentState.nameHash})");
                        Debug.Log($"   - IsCarrying: {animator.GetBool("IsCarrying")}");
                        Debug.Log($"   - Speed: {animator.GetFloat("Speed")} (Set to: 0)");
                        Debug.Log($"   - StateChanged: {stateChanged}");
                    }
                }
                else if (crouching)
                {
                    // Crouching state (third priority for ground movement)
                    animator.SetBool("IsRunning", false);
                    
                    // Debug crouch state and parameters
                    var currentState = animator.GetCurrentAnimatorStateInfo(0);
                    string currentStateName = currentState.IsName("CarryIdle") ? "CarryIdle" : 
                                              currentState.IsName("CarryWalking") ? "CarryWalking" : 
                                              currentState.IsName("Idle") ? "Idle" : 
                                              currentState.IsName("Walking") ? "Walking" : 
                                              currentState.IsName("Running") ? "Running" : 
                                              currentState.IsName("CrouchIdle") ? "CrouchIdle" : 
                                              currentState.IsName("CrouchWalking") ? "CrouchWalking" : 
                                              $"Unknown({currentState.nameHash})";
                    
                    if (isMoving)
                    {
                        animator.SetFloat("Speed", crouchSpeed);
                        Debug.Log($"🦆 CROUCH WALKING DEBUG:");
                        Debug.Log($"   - Current State: {currentStateName} (Hash: {currentState.nameHash})");
                        Debug.Log($"   - IsCrouching: {animator.GetBool("IsCrouching")}");
                        Debug.Log($"   - Speed: {animator.GetFloat("Speed")} (Set to: {crouchSpeed})");
                        Debug.Log($"   - StateChanged: {stateChanged}");
                    }
                    else
                    {
                        animator.SetFloat("Speed", 0f);
                        Debug.Log($"🦆 CROUCH IDLE DEBUG:");
                        Debug.Log($"   - Current State: {currentStateName} (Hash: {currentState.nameHash})");
                        Debug.Log($"   - IsCrouching: {animator.GetBool("IsCrouching")}");
                        Debug.Log($"   - Speed: {animator.GetFloat("Speed")} (Set to: 0)");
                        Debug.Log($"   - StateChanged: {stateChanged}");
                    }
                }
                else if (isMoving && isRunning)
                {
                    // Player is moving and sprinting - play running animation
                    animator.SetBool("IsRunning", true);
                    animator.SetFloat("Speed", runSpeed);
                    if (stateChanged) Debug.Log($"🏃 Setting animation: RUNNING (Speed={runSpeed})");
                }
                else if (isMoving)
                {
                    // Player is moving but not sprinting - play walking animation
                    animator.SetBool("IsRunning", false);
                    animator.SetFloat("Speed", walkSpeed);
                    if (stateChanged) Debug.Log($"🚶 Setting animation: WALKING (Speed={walkSpeed})");
                }
                else
                {
                    // Player is not moving - idle
                    animator.SetBool("IsRunning", false);
                    animator.SetFloat("Speed", 0f);
                    if (stateChanged) Debug.Log($"🧍 Setting animation: IDLE");
                }
            }
            
            lastIsRunning = isRunning;
            lastIsMoving = isMoving;
            lastIsJumping = jumpPressed;
            lastIsGrounded = isGrounded;
            lastIsCrouching = crouching;
            lastIsCarrying = carrying;
            lastIsBeingCarried = beingCarried;
            lastIsThrowing = isThrowing;
            
            // Additional debug info
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("❌ Animator Controller is NULL! Please assign PlayerAnimatorController to the Animator component.");
            }
            
            // Log current animator state info (but not too frequently to avoid spam)
            if (animator.layerCount > 0 && Time.time % 1.0f < 0.1f) // Only log every second
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
                string stateName = "Unknown";
                
                // Try to identify the state name from common hashes
                if (currentState.IsName("Idle")) stateName = "Idle";
                else if (currentState.IsName("Running")) stateName = "Running";
                else if (currentState.IsName("Walking")) stateName = "Walking";
                else if (currentState.IsName("Jumping")) stateName = "Jumping";
                else if (currentState.IsName("CrouchIdle")) stateName = "CrouchIdle";
                else if (currentState.IsName("CrouchWalking")) stateName = "CrouchWalking";
                else if (currentState.IsName("CarryIdle")) stateName = "CarryIdle";
                else if (currentState.IsName("CarryWalking")) stateName = "CarryWalking";
                else if (currentState.IsName("BeingCarriedIdle")) stateName = "BeingCarriedIdle";
                else if (currentState.IsName("Throwing")) stateName = "Throwing";
                else if (currentState.IsName("Base Layer.Idle")) stateName = "Base Layer.Idle";
                else if (currentState.IsName("Base Layer.Running")) stateName = "Base Layer.Running";
                else
                {
                    stateName = $"Unknown(Hash:{currentState.shortNameHash})";
                    if (stateChanged)
                    {
                        Debug.LogWarning($"⚠️ Unknown Animator State! Hash: {currentState.shortNameHash}, FullHash: {currentState.fullPathHash}");
                    }
                }
                
                // Only log when state actually changes to reduce spam
                if (stateChanged)
                {
                    Debug.Log($"🎭 Animator State Changed: {stateName} (Hash: {currentState.shortNameHash}, IsRunning: {animator.GetBool("IsRunning")}, Speed: {animator.GetFloat("Speed")})");
                }
            }
        }

        // Calculate projectile velocity for realistic ball-throwing arc
        Vector3 CalculateProjectileVelocity(Vector3 startPos, Vector3 targetPos, float arcHeight)
        {
            // Get the horizontal distance and direction
            Vector3 horizontalDisplacement = targetPos - startPos;
            horizontalDisplacement.y = 0; // Remove vertical component
            
            float horizontalDistance = horizontalDisplacement.magnitude;
            Vector3 horizontalDirection = horizontalDisplacement.normalized;
            
            // Calculate time of flight using projectile motion equations
            // For a projectile with arc height h, the time to reach max height is:
            // t_up = sqrt(2h/g), total flight time = 2 * t_up
            float timeToReachMaxHeight = Mathf.Sqrt(2 * arcHeight / gravity);
            float totalFlightTime = 2 * timeToReachMaxHeight;
            
            // Calculate horizontal velocity needed to cover the distance in flight time
            float horizontalVelocity = horizontalDistance / totalFlightTime;
            
            // Calculate initial vertical velocity to reach the desired arc height
            // v_y = sqrt(2 * g * h) for projectile motion
            float verticalVelocity = Mathf.Sqrt(2 * gravity * arcHeight);
            
            // Combine horizontal and vertical components
            Vector3 velocity = horizontalDirection * horizontalVelocity + Vector3.up * verticalVelocity;
            
            Debug.Log($"🎯 Projectile calculation: Distance={horizontalDistance:F1}m, Height={arcHeight:F1}m, Time={totalFlightTime:F1}s, Velocity={velocity}");
            
            return velocity;
        }

        void OnDrawGizmosSelected()
        {
            if (showCarryRadius)
            {
                // Draw carry radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, carryRadius);

                // Draw carry position
                if (carryPosition != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(carryPosition.position, 0.5f);
                    Gizmos.DrawLine(transform.position, carryPosition.position);
                }

                // Draw toss trajectory preview
                DrawTossTrajectory();
            }
        }

        void DrawTossTrajectory()
        {
            if (!Application.isPlaying) return;

            // Calculate toss target and velocity
            Vector3 startPos = transform.position + Vector3.up * 1f; // Throwing height
            Vector3 targetPos = startPos + transform.forward * tossDistance;
            
            // Draw trajectory arc
            Gizmos.color = Color.red;
            Vector3 currentPos = startPos;
            
            // Simulate projectile motion for visualization
            float timeStep = 0.05f;
            int steps = Mathf.RoundToInt(3f / timeStep); // Show 3 seconds of trajectory
            Vector3 velocity = CalculateProjectileVelocity(startPos, targetPos, tossHeight);

            for (int i = 0; i < steps; i++)
            {
                Vector3 nextPos = currentPos + velocity * timeStep;
                velocity.y -= gravity * timeStep; // Apply gravity
                
                Gizmos.DrawLine(currentPos, nextPos);
                currentPos = nextPos;

                // Stop if we hit the ground (below start height)
                if (currentPos.y <= transform.position.y) break;
            }

            // Draw target position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            
            // Draw max height indicator
            Gizmos.color = Color.magenta;
            Vector3 maxHeightPos = startPos + (targetPos - startPos) * 0.5f;
            maxHeightPos.y = startPos.y + tossHeight;
            Gizmos.DrawWireCube(maxHeightPos, Vector3.one * 0.3f);
        }

        // Public properties for UI/debugging
        public bool IsCarrying => carriedPlayerNetId != 0;
        public bool IsBeingCarried => isBeingCarried;
        public bool IsBeingTossed => isBeingTossed;
        public Vector3 CurrentVelocity => velocity;
        public float TossStartTime => tossStartTime;
        public EnhancedPlayerMovement Carrier => carrier;
        public string CarryStatus
        {
            get
            {
                if (isBeingCarried) return $"Being carried by {(carrier?.name ?? "Unknown")}";
                if (carriedPlayerNetId != 0) return $"Carrying {(carriedPlayer?.name ?? "Unknown")}";
                return "Not carrying anyone";
            }
        }
        
        // Player name management
        public string PlayerDisplayName => playerDisplayName;
        
        /// <summary>
        /// Sets the player's display name (only works on server)
        /// </summary>
        [Server]
        public void SetPlayerName(string newName)
        {
            if (!string.IsNullOrEmpty(newName))
            {
                playerDisplayName = newName;
                Debug.Log($"🏷️ Server set player name: {newName} for {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Command to set player name from client
        /// </summary>
        [Command]
        public void CmdSetPlayerName(string newName)
        {
            // Validate name on server
            if (string.IsNullOrEmpty(newName) || newName.Length < 2 || newName.Length > 20)
            {
                Debug.LogWarning($"⚠️ Invalid name received from client: '{newName}'");
                return;
            }
            
            // Clean the name (remove extra spaces, etc.)
            string cleanName = newName.Trim();
            
            // Set the name
            SetPlayerName(cleanName);
            
            Debug.Log($"🏷️ Client {connectionToClient.connectionId} set name to: {cleanName}");
        }
        
        /// <summary>
        /// Called when player name changes (SyncVar hook)
        /// </summary>
        
        
        /// <summary>
        /// Gets the Steam player name if available
        /// </summary>
        string GetSteamPlayerName()
        {
            try
            {
                // Try to get Steam name using our Steam integration
                if (SteamPlayerNameManager.IsSteamAvailable())
                {
                    // For local player, get their Steam name directly
                    if (isLocalPlayer)
                    {
                        string steamName = SteamPlayerNameManager.GetLocalPlayerSteamName();
                        if (!string.IsNullOrEmpty(steamName))
                        {
                            Debug.Log($"🏷️ Found local Steam name: {steamName}");
                            return steamName;
                        }
                    }
                    
                    // For remote players, try to get from Mirror's authentication data
                    if (connectionToClient != null && connectionToClient.authenticationData != null)
                    {
                        string authData = connectionToClient.authenticationData.ToString();
                        if (!string.IsNullOrEmpty(authData) && authData != "null")
                        {
                            Debug.Log($"🏷️ Found Steam name from auth data: {authData}");
                            return authData;
                        }
                    }
                }
                
                Debug.Log("ℹ️ Steam not available or no name found - using fallback");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Could not get Steam name: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Sets up the name display component
        /// </summary>
        void SetupNameDisplay()
        {
            // Add PlayerNameDisplay component if not already present
            PlayerNameDisplay nameDisplay = GetComponent<PlayerNameDisplay>();
            if (nameDisplay == null)
            {
                nameDisplay = gameObject.AddComponent<PlayerNameDisplay>();
                Debug.Log($"✅ Added PlayerNameDisplay component to {gameObject.name}");
            }
            
             
        }
    }
}
