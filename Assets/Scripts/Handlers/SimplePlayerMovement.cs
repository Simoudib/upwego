using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class SimplePlayerMovement : NetworkBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpForce = 8f;
        public float gravity = 20f;

        private CharacterController controller;
        private Vector3 moveDirection = Vector3.zero;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            bool jump = Input.GetButtonDown("Jump");

            // Check if grounded
            bool isGrounded = controller.isGrounded;

            // Handle jumping (separate from movement)
            if (isGrounded && jump)
            {
                moveDirection.y = jumpForce;
                Debug.Log("🦘 Jump triggered!");
            }

            // Handle horizontal movement
            if (isGrounded)
            {
                // Calculate movement direction
                Vector3 horizontalMovement = new Vector3(horizontal, 0, vertical);
                horizontalMovement = transform.TransformDirection(horizontalMovement);
                
                // Apply speed
                float speed = isRunning ? runSpeed : walkSpeed;
                horizontalMovement *= speed;

                // Update only horizontal components
                moveDirection.x = horizontalMovement.x;
                moveDirection.z = horizontalMovement.z;

                // Reset Y if grounded (but preserve jump)
                if (moveDirection.y <= 0)
                {
                    moveDirection.y = -2f; // Small downward force to stay grounded
                }
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the character
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}

