using UnityEngine;
using Mirror;
using Steamworks;

namespace UpWeGo
{
    /// <summary>
    /// Manages Steam player names for the name display system
    /// Uses the same Steam API calls as your existing friend invite system
    /// </summary>
    public class SteamPlayerNameManager : MonoBehaviour
    {
        [Header("Steam Integration")]
        [SerializeField] private bool debugSteamNames = true;
        
        private static SteamPlayerNameManager instance;
        public static SteamPlayerNameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SteamPlayerNameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SteamPlayerNameManager");
                        instance = go.AddComponent<SteamPlayerNameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Gets the Steam name for a player using their Steam ID
        /// Uses the same method as your friend invite system
        /// </summary>
        public static string GetSteamPlayerName(CSteamID steamID)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("⚠️ Steam not initialized - cannot get player name");
                return null;
            }
            
            if (!steamID.IsValid() || steamID.m_SteamID == 0)
            {
                Debug.LogWarning("⚠️ Invalid Steam ID - cannot get player name");
                return null;
            }
            
            try
            {
                string playerName = SteamFriends.GetFriendPersonaName(steamID);
                
                if (string.IsNullOrEmpty(playerName))
                {
                    Debug.LogWarning($"⚠️ Could not get Steam name for ID {steamID.m_SteamID}");
                    return null;
                }
                
                Debug.Log($"🏷️ Got Steam name: {playerName} for ID: {steamID.m_SteamID}");
                return playerName;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error getting Steam name: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Steam name for the local player
        /// </summary>
        public static string GetLocalPlayerSteamName()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("⚠️ Steam not initialized - cannot get local player name");
                return null;
            }
            
            try
            {
                string playerName = SteamFriends.GetPersonaName();
                
                if (string.IsNullOrEmpty(playerName))
                {
                    Debug.LogWarning("⚠️ Could not get local Steam name");
                    return null;
                }
                
                Debug.Log($"🏷️ Got local Steam name: {playerName}");
                return playerName;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error getting local Steam name: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the Steam ID for the local player
        /// </summary>
        public static CSteamID GetLocalPlayerSteamID()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("⚠️ Steam not initialized - cannot get local Steam ID");
                return CSteamID.Nil;
            }
            
            try
            {
                CSteamID steamID = SteamUser.GetSteamID();
                Debug.Log($"🏷️ Got local Steam ID: {steamID.m_SteamID}");
                return steamID;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error getting local Steam ID: {e.Message}");
                return CSteamID.Nil;
            }
        }
        
        /// <summary>
        /// Checks if Steam is available and initialized
        /// </summary>
        public static bool IsSteamAvailable()
        {
            return SteamManager.Initialized;
        }
        
        void OnGUI()
        {
            if (debugSteamNames)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 150));
                GUILayout.Label($"Steam Available: {IsSteamAvailable()}");
                
                if (IsSteamAvailable())
                {
                    string localName = GetLocalPlayerSteamName();
                    CSteamID localID = GetLocalPlayerSteamID();
                    GUILayout.Label($"Local Name: {localName ?? "Not available"}");
                    GUILayout.Label($"Local ID: {localID.m_SteamID}");
                }
                else
                {
                    GUILayout.Label("Steam not available");
                }
                
                GUILayout.EndArea();
            }
        }
    }
}

