using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class TossPhysicsDebugger : NetworkBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugUI = true;
        public bool logTossEvents = true;

        private EnhancedPlayerMovement playerMovement;
        private GUIStyle debugStyle;

        void Start()
        {
            playerMovement = GetComponent<EnhancedPlayerMovement>();
            
            // Setup debug GUI style
            debugStyle = new GUIStyle();
            debugStyle.fontSize = 16;
            debugStyle.normal.textColor = Color.white;
        }

        void OnGUI()
        {
            if (!showDebugUI || !isLocalPlayer || playerMovement == null) return;

            // Create debug panel
            GUILayout.BeginArea(new Rect(10, 200, 400, 300));
            GUILayout.BeginVertical("box");

            GUILayout.Label("🚀 TOSS PHYSICS DEBUG", debugStyle);
            GUILayout.Space(10);

            // Show current state
            GUILayout.Label($"Is Being Tossed: {playerMovement.IsBeingTossed}", debugStyle);
            GUILayout.Label($"Is Being Carried: {playerMovement.IsBeingCarried}", debugStyle);
            GUILayout.Label($"Is Carrying: {playerMovement.IsCarrying}", debugStyle);
            
            GUILayout.Space(10);

            // Show physics info
            CharacterController controller = playerMovement.GetComponent<CharacterController>();
            if (controller != null)
            {
                GUILayout.Label($"Grounded: {controller.isGrounded}", debugStyle);
                GUILayout.Label($"Velocity: {playerMovement.CurrentVelocity}", debugStyle);
            }

            GUILayout.Space(10);

            // Debug buttons
            if (GUILayout.Button("🧪 Test Toss (Self)"))
            {
                TestToss();
            }

            if (GUILayout.Button("🔄 Reset State"))
            {
                ResetTossState();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        [ContextMenu("Test Toss")]
        void TestToss()
        {
            Debug.Log("🧪 Use the actual carry/toss system to test tossing");
        }

        [ContextMenu("Reset Toss State")]
        void ResetTossState()
        {
            Debug.Log("🔄 Toss state is managed automatically by the system");
        }

        void OnDrawGizmos()
        {
            if (playerMovement == null || !playerMovement.IsBeingTossed) return;

            // Draw toss trajectory
            Gizmos.color = Color.cyan;
            Vector3 currentPos = transform.position;
            Vector3 currentVel = playerMovement.CurrentVelocity;
            
            for (int i = 0; i < 20; i++)
            {
                Vector3 nextPos = currentPos + currentVel * 0.1f;
                currentVel.y -= 20f * 0.1f; // Use default gravity
                
                Gizmos.DrawLine(currentPos, nextPos);
                currentPos = nextPos;
                
                if (currentPos.y <= transform.position.y - 2f) break;
            }
        }
    }
}
