using UnityEngine;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Optional helper for Steam name detection
    /// This version works without requiring Steam SDK integration
    /// </summary>
    public class SteamNameHelper : MonoBehaviour
    {
        [Header("Steam Integration")]
        [SerializeField] private bool enableSteamIntegration = true;
        [SerializeField] private bool debugSteamNames = true;
        
        void Start()
        {
            if (enableSteamIntegration)
            {
                SetupSteamIntegration();
            }
        }
        
        void SetupSteamIntegration()
        {
            Debug.Log("🔧 Steam Name Helper: Setting up Steam integration...");
            Debug.Log("ℹ️ Using Mirror's authentication data for Steam names");
            Debug.Log("💡 To add Steam SDK integration, extend this script with your Steam SDK");
        }
        
        /// <summary>
        /// Gets the current Steam player name (if available)
        /// This version uses Mirror's authentication data
        /// </summary>
        public static string GetCurrentSteamName()
        {
            // This method can be extended to use actual Steam SDK
            // For now, it returns null to use fallback names
            return null;
        }
        
        /// <summary>
        /// Checks if Steam is available and initialized
        /// </summary>
        public static bool IsSteamAvailable()
        {
            // This can be extended to check for actual Steam SDK
            return false;
        }
        
        /// <summary>
        /// Gets Steam user ID (if available)
        /// </summary>
        public static ulong GetSteamUserId()
        {
            // This can be extended to get actual Steam user ID
            return 0;
        }
        
        void OnGUI()
        {
            if (debugSteamNames && enableSteamIntegration)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label($"Steam Available: {IsSteamAvailable()}");
                
                if (IsSteamAvailable())
                {
                    string steamName = GetCurrentSteamName();
                    GUILayout.Label($"Steam Name: {steamName ?? "Not available"}");
                    GUILayout.Label($"Steam ID: {GetSteamUserId()}");
                }
                else
                {
                    GUILayout.Label("Steam not available - using fallback names");
                }
                
                GUILayout.EndArea();
            }
        }
    }
}