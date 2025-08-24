using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class CarryConflictDebugger : NetworkBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugUI = true;
        public bool logComponentStates = true;

        private EnhancedPlayerMovement playerMovement;
        private GUIStyle debugStyle;

        // Component references to monitor
        private CharacterController characterController;
        private NetworkTransformBase networkTransform;
        private Rigidbody rigidBody;

        void Start()
        {
            playerMovement = GetComponent<EnhancedPlayerMovement>();
            characterController = GetComponent<CharacterController>();
            networkTransform = GetComponent<NetworkTransformBase>();
            rigidBody = GetComponent<Rigidbody>();
            
            // Setup debug GUI style
            debugStyle = new GUIStyle();
            debugStyle.fontSize = 14;
            debugStyle.normal.textColor = Color.white;
        }

        void OnGUI()
        {
            if (!showDebugUI || !isLocalPlayer || playerMovement == null) return;

            // Create debug panel
            GUILayout.BeginArea(new Rect(10, 350, 450, 250));
            GUILayout.BeginVertical("box");

            GUILayout.Label("🔧 CARRY CONFLICT DEBUGGER", debugStyle);
            GUILayout.Space(5);

            // Show carry state
            GUILayout.Label($"Carry State: {GetCarryStateText()}", debugStyle);
            GUILayout.Space(5);

            // Show component states
            GUILayout.Label("🧩 COMPONENT STATES:", debugStyle);
            GUILayout.Label($"CharacterController: {GetComponentState(characterController)}", debugStyle);
            GUILayout.Label($"NetworkTransform: {GetComponentState(networkTransform)}", debugStyle);
            GUILayout.Label($"Rigidbody: {GetRigidbodyState()}", debugStyle);
            
            GUILayout.Space(5);

            // Show position data
            GUILayout.Label("📍 POSITION DATA:", debugStyle);
            GUILayout.Label($"Transform Position: {transform.position}", debugStyle);
            
            if (playerMovement.IsBeingCarried && playerMovement.Carrier != null)
            {
                Vector3 carrierPos = playerMovement.Carrier.carryPosition.position;
                Vector3 distance = transform.position - carrierPos;
                GUILayout.Label($"Carrier Position: {carrierPos}", debugStyle);
                GUILayout.Label($"Distance: {distance.magnitude:F3}m", GetDistanceStyle(distance.magnitude));
            }

            GUILayout.Space(5);

            // Debug buttons
            if (GUILayout.Button("🧪 Log Component Details"))
            {
                LogComponentDetails();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        string GetCarryStateText()
        {
            if (playerMovement.IsBeingCarried) return "🎒 BEING CARRIED";
            if (playerMovement.IsCarrying) return "💪 CARRYING";
            if (playerMovement.IsBeingTossed) return "🚀 BEING TOSSED";
            return "🚶 NORMAL";
        }

        string GetComponentState(Component component)
        {
            if (component == null) return "❌ NULL";
            
            if (component is Behaviour behaviour)
            {
                return behaviour.enabled ? "✅ ENABLED" : "🚫 DISABLED";
            }
            
            return "✅ EXISTS";
        }

        string GetRigidbodyState()
        {
            if (rigidBody == null) return "❌ NULL";
            
            string kinematic = rigidBody.isKinematic ? "KINEMATIC" : "DYNAMIC";
            return $"✅ {kinematic}";
        }

        GUIStyle GetDistanceStyle(float distance)
        {
            GUIStyle style = new GUIStyle(debugStyle);
            
            if (distance < 0.1f)
                style.normal.textColor = Color.green; // Perfect
            else if (distance < 0.5f)
                style.normal.textColor = Color.yellow; // Acceptable
            else
                style.normal.textColor = Color.red; // Too far
                
            return style;
        }

        [ContextMenu("Log Component Details")]
        void LogComponentDetails()
        {
            Debug.Log("=== CARRY CONFLICT DEBUG ===");
            Debug.Log($"Player: {gameObject.name}");
            Debug.Log($"Is Being Carried: {playerMovement.IsBeingCarried}");
            Debug.Log($"Is Carrying: {playerMovement.IsCarrying}");
            Debug.Log($"Is Being Tossed: {playerMovement.IsBeingTossed}");
            
            Debug.Log("--- Component States ---");
            Debug.Log($"CharacterController: {GetComponentState(characterController)}");
            Debug.Log($"NetworkTransform: {GetComponentState(networkTransform)}");
            Debug.Log($"Rigidbody: {GetRigidbodyState()}");
            
            if (playerMovement.IsBeingCarried && playerMovement.Carrier != null)
            {
                Vector3 carrierPos = playerMovement.Carrier.carryPosition.position;
                Vector3 distance = transform.position - carrierPos;
                Debug.Log($"Distance from carrier: {distance.magnitude:F3}m");
                
                if (distance.magnitude > 0.5f)
                {
                    Debug.LogWarning("⚠️ LARGE DISTANCE DETECTED - Possible lag/conflict issue!");
                }
            }
            
            Debug.Log("==========================");
        }

        void Update()
        {
            if (!logComponentStates || !isLocalPlayer) return;

            // Log when being carried and distance is too large
            if (playerMovement.IsBeingCarried && playerMovement.Carrier != null)
            {
                Vector3 distance = transform.position - playerMovement.Carrier.carryPosition.position;
                
                if (distance.magnitude > 1f)
                {
                    Debug.LogWarning($"🚨 Large carry distance detected: {distance.magnitude:F3}m");
                    LogComponentDetails();
                }
            }
        }
    }
}
