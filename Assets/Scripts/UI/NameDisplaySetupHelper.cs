using UnityEngine;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Helper script to automatically setup name display system in the scene
    /// </summary>
    public class NameDisplaySetupHelper : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnAwake = true;
        [SerializeField] private bool addToAllPlayers = true;
        
        [Header("Steam Integration")]
        [SerializeField] private bool useSteamNames = true;
        [SerializeField] private bool fallbackToConnectionId = true;
        
        void Awake()
        {
            if (setupOnAwake)
            {
                SetupNameDisplaySystem();
            }
        }
        
        void Start()
        {
            // Setup for any existing players
            if (addToAllPlayers)
            {
                SetupExistingPlayers();
            }
            
            // Steam names are handled automatically by EnhancedPlayerMovement
        }
        
        /// <summary>
        /// Sets up the name display system
        /// </summary>
        public void SetupNameDisplaySystem()
        {
            Debug.Log("🔧 Setting up Name Display System...");
            
            // The system will automatically setup when players are created
            // This method can be used for any global initialization
            
            Debug.Log("✅ Name Display System setup complete!");
        }
        
        /// <summary>
        /// Adds name display components to existing players
        /// </summary>
        void SetupExistingPlayers()
        {
            EnhancedPlayerMovement[] players = FindObjectsOfType<EnhancedPlayerMovement>();
            
            foreach (var player in players)
            {
                // The player will setup its own name display in Start()
                // This is just for logging
                Debug.Log($"🎯 Found existing player: {player.gameObject.name}");
            }
            
            if (players.Length > 0)
            {
                Debug.Log($"✅ Found {players.Length} existing players - they will auto-setup name displays");
            }
        }
        
        /// <summary>
        /// Steam names are automatically detected and used
        /// </summary>
        void SetupSteamIntegration()
        {
            Debug.Log("🔧 Steam integration: Names will be automatically detected from Steam");
            Debug.Log("📝 Fallback to connection ID if Steam names not available");
        }
        
        /// <summary>
        /// Manual setup method that can be called from inspector or other scripts
        /// </summary>
        [ContextMenu("Setup Name Display System")]
        public void ManualSetup()
        {
            SetupNameDisplaySystem();
            SetupExistingPlayers();
            SetupSteamIntegration();
        }
        
        /// <summary>
        /// Test method to assign random names to all players
        /// </summary>
        [ContextMenu("Assign Random Names to All Players")]
        public void AssignRandomNamesToAllPlayers()
        {
            EnhancedPlayerMovement[] players = FindObjectsOfType<EnhancedPlayerMovement>();
            
            string[] testNames = {
                "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry",
                "Iris", "Jack", "Kate", "Liam", "Maya", "Noah", "Olivia", "Paul"
            };
            
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].isServer || players[i].isLocalPlayer)
                {
                    string randomName = testNames[i % testNames.Length] + " " + Random.Range(100, 999);
                    players[i].SetPlayerName(randomName);
                    Debug.Log($"🎲 Assigned random name '{randomName}' to {players[i].gameObject.name}");
                }
            }
        }
    }
}
