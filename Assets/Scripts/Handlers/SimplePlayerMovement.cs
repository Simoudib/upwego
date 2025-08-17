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

            // Movement
            if (controller.isGrounded)
            {
                // Calculate movement direction
                moveDirection = new Vector3(horizontal, 0, vertical);
                moveDirection = transform.TransformDirection(moveDirection);
                
                // Apply speed
                float speed = isRunning ? runSpeed : walkSpeed;
                moveDirection *= speed;

                // Jump
                if (jump)
                {
                    moveDirection.y = jumpForce;
                }
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the character
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}
