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

        [Header("Debug")]
        public bool showCarryRadius = true;

        private CharacterController controller;
        private Vector3 velocity = Vector3.zero;

        // Carry system variables
        [SyncVar] private bool isBeingCarried = false;
        [SyncVar] private uint carrierNetId = 0; // NetworkInstanceId of the player carrying us
        [SyncVar] private uint carriedPlayerNetId = 0; // NetworkInstanceId of the player we're carrying

        // Network sync for position when being carried
        [SyncVar] private Vector3 networkCarryPosition;
        [SyncVar] private Quaternion networkCarryRotation;

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

            // Handle horizontal movement
            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            
            if (moveDirection.magnitude >= 0.1f)
            {
                // Choose speed based on running state
                float currentSpeed = isRunning ? runSpeed : walkSpeed;
                
                // Apply movement
                Vector3 horizontalMovement = moveDirection * currentSpeed;
                
                // Rotate player to face movement direction
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                
                // Apply horizontal movement
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
            // Position will be updated by the carrier
            if (carrier != null)
            {
                // Smoothly move to the carry position
                Vector3 targetPosition = carrier.carryPosition.position;
                Quaternion targetRotation = carrier.carryPosition.rotation;
                
                transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
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
                    // We're carrying someone - toss them
                    CmdTossPlayer();
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

            // Find the carried player
            if (NetworkServer.spawned.TryGetValue(carriedPlayerNetId, out NetworkIdentity carriedIdentity))
            {
                EnhancedPlayerMovement carriedPlayerComponent = carriedIdentity.GetComponent<EnhancedPlayerMovement>();
                if (carriedPlayerComponent != null)
                {
                    // Clear carry relationship
                    carriedPlayerComponent.isBeingCarried = false;
                    carriedPlayerComponent.carrierNetId = 0;
                    carriedPlayerComponent.carrier = null;

                    // Clear our reference
                    carriedPlayerNetId = 0;
                    carriedPlayer = null;

                    Debug.Log($"🚀 {gameObject.name} tossed {carriedPlayerComponent.gameObject.name}");
                    
                    // Call RPC to update clients
                    RpcOnPlayerTossed(netId, carriedIdentity.netId);
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
        void RpcOnPlayerTossed(uint carrierNetId, uint carriedNetId)
        {
            Debug.Log($"🚀 RPC: Player tossed - Carrier: {carrierNetId}, Carried: {carriedNetId}");
            
            // Clear local references for all clients
            if (NetworkClient.spawned.TryGetValue(carrierNetId, out NetworkIdentity carrierIdentity) &&
                NetworkClient.spawned.TryGetValue(carriedNetId, out NetworkIdentity carriedIdentity))
            {
                EnhancedPlayerMovement carrierPlayer = carrierIdentity.GetComponent<EnhancedPlayerMovement>();
                EnhancedPlayerMovement carriedPlayerComponent = carriedIdentity.GetComponent<EnhancedPlayerMovement>();

                if (carrierPlayer != null && carriedPlayerComponent != null)
                {
                    carrierPlayer.carriedPlayer = null;
                    carriedPlayerComponent.carrier = null;
                }
            }
        }

        void UpdateCarriedPlayerPosition()
        {
            if (carriedPlayer != null && carryPosition != null)
            {
                // Smoothly update the carried player's position
                carriedPlayer.transform.position = Vector3.Lerp(
                    carriedPlayer.transform.position, 
                    carryPosition.position, 
                    10f * Time.deltaTime
                );
                
                carriedPlayer.transform.rotation = Quaternion.Lerp(
                    carriedPlayer.transform.rotation, 
                    carryPosition.rotation, 
                    10f * Time.deltaTime
                );
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
