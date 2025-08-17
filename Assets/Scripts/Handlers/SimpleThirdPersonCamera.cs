using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class SimpleThirdPersonCamera : NetworkBehaviour
    {
        [Header("Camera Settings")]
        public float mouseSensitivity = 2f;
        public float distance = 5f;
        public float height = 2f;
        public float rotationSpeed = 5f;

        [Header("Camera Limits")]
        public float minVerticalAngle = -40f;
        public float maxVerticalAngle = 80f;

        private Transform target;
        private Camera cam;
        private float horizontalAngle = 0f;
        private float verticalAngle = 0f;

        void Start()
        {
            if (!isLocalPlayer)
            {
                enabled = false;
                return;
            }

            target = transform;
            cam = Camera.main;
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Initialize angles
            horizontalAngle = target.eulerAngles.y;
            verticalAngle = 0f;
        }

        void LateUpdate()
        {
            if (!isLocalPlayer || target == null || cam == null) return;

            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Update angles
            horizontalAngle += mouseX;
            verticalAngle -= mouseY;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

            // Calculate camera position
            Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * distance);
            position.y += height;

            // Set camera position and rotation
            cam.transform.position = position;
            cam.transform.LookAt(target.position + Vector3.up * height);

            // Rotate player horizontally with camera
            target.rotation = Quaternion.Euler(0, horizontalAngle, 0);
        }

        void OnDestroy()
        {
            if (isLocalPlayer)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
