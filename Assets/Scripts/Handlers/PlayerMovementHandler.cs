using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

namespace UpWeGo
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float upperLookLimit = 80f;
        [SerializeField] private float lowerLookLimit = -80f;

        // References
        private CharacterController characterController;
        private Camera playerCamera;
        private InputSystem_Actions playerControls;
        private Transform playerTransform;

        // Movement variables
        private Vector2 currentMovementInput;
        private Vector3 currentMovement;
        private Vector3 currentRunMovement;
        private Vector3 appliedMovement;
        private bool isMovementPressed;
        private bool isRunPressed;
        private bool isJumpPressed = false;
        private bool isGrounded;
        private float verticalVelocity;

        // Camera variables
        private float cameraPitch = 0f;
        private Vector2 currentLookInput;

        private void Awake()
        {
            playerControls = new InputSystem_Actions();
            characterController = GetComponent<CharacterController>();
            playerTransform = transform;
        }

        public override void OnStartLocalPlayer()
        {
            // Only setup camera and controls for the local player
            if (!isLocalPlayer) return;

            // Setup camera
            playerCamera = Camera.main;
            if (playerCamera != null && cameraTarget != null)
            {
                playerCamera.transform.position = cameraTarget.position;
                playerCamera.transform.rotation = cameraTarget.rotation;
                playerCamera.transform.SetParent(cameraTarget);
            }

            // Setup input actions
            playerControls.Player.Move.performed += OnMoveInput;
            playerControls.Player.Move.canceled += OnMoveInput;
            playerControls.Player.Look.performed += OnLookInput;
            playerControls.Player.Look.canceled += OnLookInput;
            playerControls.Player.Jump.performed += OnJumpInput;
            playerControls.Player.Jump.canceled += OnJumpInput;
            playerControls.Player.Sprint.performed += OnSprintInput;
            playerControls.Player.Sprint.canceled += OnSprintInput;

            // Enable controls
            playerControls.Player.Enable();

            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                // Disable controls
                playerControls.Player.Disable();

                // Unlock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            HandleMovement();
            HandleCameraRotation();
        }

        #region Input Callbacks
        private void OnMoveInput(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;
            currentMovementInput = context.ReadValue<Vector2>();
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        }

        private void OnLookInput(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;
            currentLookInput = context.ReadValue<Vector2>();
        }

        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;
            isJumpPressed = context.ReadValueAsButton();
        }

        private void OnSprintInput(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;
            isRunPressed = context.ReadValueAsButton();
        }
        #endregion

        private void HandleMovement()
        {
            // Check if grounded
            isGrounded = characterController.isGrounded;

            // Calculate movement direction relative to camera orientation
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Set movement based on input
            currentMovement = (forward * currentMovementInput.y + right * currentMovementInput.x);
            currentRunMovement = currentMovement * (isRunPressed ? runSpeed : walkSpeed);

            // Apply gravity
            if (isGrounded)
            {
                verticalVelocity = -0.5f; // Small downward force when grounded
                
                // Apply jump force
                if (isJumpPressed)
                {
                    verticalVelocity = jumpForce;
                    isJumpPressed = false;
                }
            }
            else
            {
                // Apply gravity when in air
                verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }

            // Apply all movement
            appliedMovement = currentRunMovement;
            appliedMovement.y = verticalVelocity;

            // Move the character
            characterController.Move(appliedMovement * Time.deltaTime);

            // Rotate character to face movement direction if moving forward or to the sides
            if (isMovementPressed && currentMovementInput.y >= 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(currentMovement.x, 0, currentMovement.z));
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            // When moving backward, don't change the rotation, just move in reverse
            else if (isMovementPressed && currentMovementInput.y < 0)
            {
                // Keep current rotation but move in the direction of the input
            }
        }

        private void HandleCameraRotation()
        {
            if (playerCamera == null) return;

            // Apply mouse sensitivity
            Vector2 lookDelta = currentLookInput * mouseSensitivity;

            // Vertical rotation (pitch) - applied to camera target
            cameraPitch -= lookDelta.y;
            cameraPitch = Mathf.Clamp(cameraPitch, lowerLookLimit, upperLookLimit);
            cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

            // Horizontal rotation (yaw) - applied to player
            playerTransform.Rotate(Vector3.up * lookDelta.x);
        }
    }
}