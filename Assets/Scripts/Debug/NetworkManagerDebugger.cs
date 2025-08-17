using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class NetworkManagerDebugger : MonoBehaviour
    {
        [ContextMenu("Debug NetworkManager Issues")]
        public void DebugNetworkManagerIssues()
        {
            Debug.Log("=== NETWORK MANAGER DEBUG ===");
            
            // Find all NetworkManager instances
            NetworkManager[] networkManagers = FindObjectsOfType<NetworkManager>();
            Debug.Log($"Found {networkManagers.Length} NetworkManager instances in scene");
            
            for (int i = 0; i < networkManagers.Length; i++)
            {
                var nm = networkManagers[i];
                Debug.Log($"NetworkManager {i + 1}:");
                Debug.Log($"  GameObject: {nm.gameObject.name}");
                Debug.Log($"  Active: {nm.gameObject.activeInHierarchy}");
                Debug.Log($"  DontDestroyOnLoad: {nm.dontDestroyOnLoad}");
                Debug.Log($"  Is Singleton: {NetworkManager.singleton == nm}");
                
                // Check for DontDestroyOnLoad flag
                if (nm.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    Debug.Log($"  ✅ In DontDestroyOnLoad scene");
                }
                else
                {
                    Debug.Log($"  ⚠️ In scene: {nm.gameObject.scene.name}");
                }
            }
            
            // Check Mirror singleton
            if (NetworkManager.singleton != null)
            {
                Debug.Log($"✅ NetworkManager.singleton exists on: {NetworkManager.singleton.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ NetworkManager.singleton is NULL!");
            }
            
            // Check SteamLobby reference
            if (SteamLobby.Instance != null)
            {
                Debug.Log($"✅ SteamLobby.Instance exists");
                if (SteamLobby.Instance.networkManager != null)
                {
                    Debug.Log($"✅ SteamLobby has NetworkManager reference: {SteamLobby.Instance.networkManager.gameObject.name}");
                }
                else
                {
                    Debug.LogError("❌ SteamLobby.networkManager is NULL!");
                }
            }
            else
            {
                Debug.LogError("❌ SteamLobby.Instance is NULL!");
            }
            
            Debug.Log("=== END DEBUG ===");
        }
        
        [ContextMenu("Check DontDestroyOnLoad Objects")]
        public void CheckDontDestroyOnLoadObjects()
        {
            Debug.Log("=== DONT DESTROY ON LOAD DEBUG ===");
            
            // Find all objects in DontDestroyOnLoad scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int dontDestroyCount = 0;
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.scene.name == "DontDestroyOnLoad")
                {
                    dontDestroyCount++;
                    Debug.Log($"DontDestroyOnLoad object: {obj.name}");
                    
                    // Check for components that might cause conflicts
                    NetworkManager nm = obj.GetComponent<NetworkManager>();
                    if (nm != null)
                    {
                        Debug.Log($"  - Has NetworkManager component");
                    }
                    
                    SteamLobby sl = obj.GetComponent<SteamLobby>();
                    if (sl != null)
                    {
                        Debug.Log($"  - Has SteamLobby component");
                    }
                    
                    InvitationOverlayManager iom = obj.GetComponent<InvitationOverlayManager>();
                    if (iom != null)
                    {
                        Debug.Log($"  - Has InvitationOverlayManager component");
                    }
                }
            }
            
            Debug.Log($"Total DontDestroyOnLoad objects: {dontDestroyCount}");
            Debug.Log("=== END DEBUG ===");
        }
        
        [ContextMenu("Fix NetworkManager Issues")]
        public void FixNetworkManagerIssues()
        {
            Debug.Log("🔧 Attempting to fix NetworkManager issues...");
            
            NetworkManager[] networkManagers = FindObjectsOfType<NetworkManager>();
            
            if (networkManagers.Length == 0)
            {
                Debug.LogError("❌ No NetworkManager found in scene!");
                return;
            }
            
            if (networkManagers.Length > 1)
            {
                Debug.LogWarning($"⚠️ Found {networkManagers.Length} NetworkManagers, keeping only one...");
                
                // Keep the first one, destroy others
                for (int i = 1; i < networkManagers.Length; i++)
                {
                    Debug.Log($"Destroying duplicate NetworkManager on: {networkManagers[i].gameObject.name}");
                    DestroyImmediate(networkManagers[i].gameObject);
                }
            }
            
            NetworkManager keepManager = networkManagers[0];
            
            // Make sure it's set as singleton
            if (NetworkManager.singleton != keepManager)
            {
                Debug.Log("Setting NetworkManager singleton...");
                // This might require restarting play mode
            }
            
            // Update SteamLobby reference
            if (SteamLobby.Instance != null)
            {
                SteamLobby.Instance.networkManager = keepManager;
                Debug.Log("✅ Updated SteamLobby.networkManager reference");
            }
            
            Debug.Log("✅ NetworkManager fix complete!");
        }
        
        void Start()
        {
            // Auto-debug on start if there are issues
            if (NetworkManager.singleton == null)
            {
                Debug.LogWarning("⚠️ NetworkManager.singleton is null on start, running auto-debug...");
                DebugNetworkManagerIssues();
            }
        }
    }
}