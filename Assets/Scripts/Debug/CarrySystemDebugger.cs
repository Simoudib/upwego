using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class CarrySystemDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugUI = true;
        public bool showCarryRadiusAlways = false;

        private EnhancedPlayerMovement localPlayer;

        void Start()
        {
            // Find local player
            if (NetworkClient.localPlayer != null)
            {
                localPlayer = NetworkClient.localPlayer.GetComponent<EnhancedPlayerMovement>();
            }
        }

        void Update()
        {
            // Try to find local player if not found
            if (localPlayer == null && NetworkClient.localPlayer != null)
            {
                localPlayer = NetworkClient.localPlayer.GetComponent<EnhancedPlayerMovement>();
            }
        }

        void OnGUI()
        {
            if (!showDebugUI || localPlayer == null) return;

            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
            GUILayout.Label("=== CARRY SYSTEM DEBUG ===");
            
            // Player status
            GUILayout.Label($"Status: {localPlayer.CarryStatus}");
            GUILayout.Label($"Is Carrying: {localPlayer.IsCarrying}");
            GUILayout.Label($"Is Being Carried: {localPlayer.IsBeingCarried}");
            
            // Controls
            GUILayout.Label($"Press E to carry/toss players");
            GUILayout.Label($"Carry Radius: {localPlayer.carryRadius:F1}m");

            // Find nearby players
            EnhancedPlayerMovement[] allPlayers = FindObjectsOfType<EnhancedPlayerMovement>();
            int nearbyCount = 0;
            foreach (var player in allPlayers)
            {
                if (player != localPlayer && !player.IsBeingCarried)
                {
                    float distance = Vector3.Distance(localPlayer.transform.position, player.transform.position);
                    if (distance <= localPlayer.carryRadius)
                    {
                        nearbyCount++;
                    }
                }
            }
            GUILayout.Label($"Nearby carriable players: {nearbyCount}");

            GUILayout.EndArea();
        }

        void OnDrawGizmos()
        {
            if (!showCarryRadiusAlways) return;

            // Show carry radius for all players
            EnhancedPlayerMovement[] allPlayers = FindObjectsOfType<EnhancedPlayerMovement>();
            foreach (var player in allPlayers)
            {
                if (player.IsBeingCarried)
                {
                    // Draw carried player in different color
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(player.transform.position, 1f);
                }
                else
                {
                    // Draw carry radius
                    Gizmos.color = player.IsCarrying ? Color.blue : Color.yellow;
                    Gizmos.DrawWireSphere(player.transform.position, player.carryRadius);
                }

                // Draw carry position if carrying someone
                if (player.IsCarrying && player.carryPosition != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(player.carryPosition.position, 0.5f);
                    Gizmos.DrawLine(player.transform.position, player.carryPosition.position);
                }
            }
        }

        [ContextMenu("Test Carry System")]
        public void TestCarrySystem()
        {
            if (localPlayer == null)
            {
                Debug.LogError("❌ No local player found!");
                return;
            }

            Debug.Log("🧪 === CARRY SYSTEM TEST ===");
            Debug.Log($"Local Player: {localPlayer.name}");
            Debug.Log($"Status: {localPlayer.CarryStatus}");
            Debug.Log($"Carry Radius: {localPlayer.carryRadius}");

            // Find all players
            EnhancedPlayerMovement[] allPlayers = FindObjectsOfType<EnhancedPlayerMovement>();
            Debug.Log($"Total players in scene: {allPlayers.Length}");

            foreach (var player in allPlayers)
            {
                if (player != localPlayer)
                {
                    float distance = Vector3.Distance(localPlayer.transform.position, player.transform.position);
                    Debug.Log($"   Player: {player.name} - Distance: {distance:F1}m - Carriable: {!player.IsBeingCarried}");
                }
            }
        }
    }
}
