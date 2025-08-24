using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace UpWeGo
{
    public class EnhancedPlayerMovement : NetworkBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpForce = 8f;
        public float gravity = 20f;

        [Header("Carry System")]
        public float carryRadius = 3f;
        public KeyCode carryKey = KeyCode.E;
        public Transform carryPosition; // Where the carried player will be positioned
        public LayerMask playerLayerMask = 1; // Layer mask for detecting other players

        [Header("Toss Settings")]
        public float tossForce = 10f; // Forward force when tossing
        public float tossUpwardForce = 5f; // Upward force when tossing
        public float tossCooldown = 1f; // Cooldown after tossing

        [Header("Network Smoothing")]
        public float localCarriedLerpSpeed = 15f; // Speed for local player being carried
        public float remoteCarriedLerpSpeed = 12f; // Speed for remote players being carried
        public float carrierUpdateSpeed = 15f; // Speed for carrier updating carried player
        public float predictionStrength = 0.5f; // How much prediction to apply (0-1)

        [Header("Debug")]
        public bool showCarryRadius = true;

        private CharacterController controller;
        private Vector3 velocity = Vector3.zero;

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

        private EnhancedPlayerMovement carriedPlayer; // Reference to the player we're carrying
        private EnhancedPlayerMovement carrier; // Reference to the player carrying us

        void Start()
        {
            controller = GetComponent<CharacterController>();
            
            // Auto-create carry position if not assigned
            if (carryPosition == null)
            {
                GameObject carryPos = new GameObject("CarryPosition");
                carryPos.transform.SetParent(transform);
                carryPos.transform.localPosition = new Vector3(0f, 1.5f, 1f); // Above and in front
                carryPosition = carryPos.transform;
            }
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            // Handle carry input
            HandleCarryInput();

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
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            bool jump = Input.GetButtonDown("Jump");

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
            
            if (inputDirection.magnitude >= 0.1f)
            {
                // Choose speed based on running state
                float currentSpeed = isRunning ? runSpeed : walkSpeed;
                
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
            // Don't process input while being carried
            if (carrier != null)
            {
                if (isLocalPlayer)
                {
                    // For local player being carried, use direct position from carrier (most responsive)
                    Vector3 targetPosition = carrier.carryPosition.position;
                    Quaternion targetRotation = carrier.carryPosition.rotation;
                    
                    transform.position = Vector3.Lerp(transform.position, targetPosition, localCarriedLerpSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, localCarriedLerpSpeed * Time.deltaTime);
                }
                else
                {
                    // For remote players, use predicted position for smoother movement
                    Vector3 targetPosition = GetPredictedCarryPosition();
                    Quaternion targetRotation = predictedCarryRotation;
                    
                    transform.position = Vector3.Lerp(transform.position, targetPosition, remoteCarriedLerpSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, remoteCarriedLerpSpeed * Time.deltaTime);
                }
            }
            else if (!isLocalPlayer)
            {
                // Fallback to network position if no carrier reference
                transform.position = Vector3.Lerp(transform.position, networkCarryPosition, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkCarryRotation, 10f * Time.deltaTime);
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
                    // Calculate toss direction (forward from carrier)
                    Vector3 tossDirection = transform.forward;
                    tossDirection.y = 0; // Keep horizontal
                    tossDirection.Normalize();

                    // Add upward component for arc
                    Vector3 tossVelocity = tossDirection * tossForce + Vector3.up * tossUpwardForce;

                    // Position the player slightly in front before applying velocity
                    Vector3 tossPosition = transform.position + transform.forward * 2f + Vector3.up * 0.5f;
                    carriedPlayerComponent.transform.position = tossPosition;

                    // Apply toss velocity to carried player
                    carriedPlayerComponent.velocity = tossVelocity;

                    // Clear carry relationship
                    carriedPlayerComponent.isBeingCarried = false;
                    carriedPlayerComponent.carrierNetId = 0;
                    carriedPlayerComponent.carrier = null;

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

                    // Apply toss effects on all clients
                    carriedPlayerComponent.transform.position = tossPosition;
                    carriedPlayerComponent.velocity = tossVelocity;
                    
                    Debug.Log($"✅ Applied toss: Position={tossPosition}, Velocity={tossVelocity}");
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

                // For the carrier, update the carried player directly (immediate response)
                if (isLocalPlayer)
                {
                    carriedPlayer.transform.position = Vector3.Lerp(
                        carriedPlayer.transform.position, 
                        carryPosition.position, 
                        carrierUpdateSpeed * Time.deltaTime
                    );
                    
                    carriedPlayer.transform.rotation = Quaternion.Lerp(
                        carriedPlayer.transform.rotation, 
                        carryPosition.rotation, 
                        carrierUpdateSpeed * Time.deltaTime
                    );

                    // Update velocity for prediction
                    UpdateCarrierVelocity();

                    // Send frequent position updates to all clients for smooth movement
                    if (Time.time - lastCarryUpdateTime > 0.02f) // 50Hz updates
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
            }
        }

        // Public properties for UI/debugging
        public bool IsCarrying => carriedPlayerNetId != 0;
        public bool IsBeingCarried => isBeingCarried;
        public string CarryStatus
        {
            get
            {
                if (isBeingCarried) return $"Being carried by {(carrier?.name ?? "Unknown")}";
                if (carriedPlayerNetId != 0) return $"Carrying {(carriedPlayer?.name ?? "Unknown")}";
                return "Not carrying anyone";
            }
        }
    }
}
