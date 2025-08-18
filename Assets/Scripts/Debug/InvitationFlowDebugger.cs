using UnityEngine;
using Steamworks;
using System.Linq;

namespace UpWeGo
{
    public class InvitationFlowDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        public KeyCode testOverlayKey = KeyCode.T;
        public KeyCode logSteamStateKey = KeyCode.L;

        void Update()
        {
            if (Input.GetKeyDown(testOverlayKey))
            {
                TestInvitationOverlay();
            }

            if (Input.GetKeyDown(logSteamStateKey))
            {
                LogSteamState();
            }
        }

        [ContextMenu("Test Invitation Overlay")]
        public void TestInvitationOverlay()
        {
            Debug.Log("🧪 MANUAL TEST: Testing invitation overlay...");
            
            if (DynamicInvitationManager.Instance == null)
            {
                Debug.LogError("❌ DynamicInvitationManager.Instance is null!");
                return;
            }

            // Use current user's Steam ID for testing
            CSteamID fakeLobbyID = new CSteamID(999999999);
            CSteamID fakeInviterID = SteamUser.GetSteamID();

            Debug.Log($"Testing with Lobby: {fakeLobbyID}, Inviter: {fakeInviterID}");
            DynamicInvitationManager.Instance.TestShowInvitationForced(fakeLobbyID, fakeInviterID);
        }

        [ContextMenu("Log Steam State")]
        public void LogSteamState()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("❌ Steam is not initialized!");
                return;
            }

            Debug.Log("🔍 STEAM STATE DEBUG:");
            Debug.Log($"   Steam ID: {SteamUser.GetSteamID()}");
            Debug.Log($"   Steam Name: {SteamFriends.GetPersonaName()}");
            Debug.Log($"   App ID: {SteamUtils.GetAppID()}");
            Debug.Log($"   Overlay Enabled: {SteamUtils.IsOverlayEnabled()}");
            Debug.Log($"   Lobby ID: {SteamLobby.Instance?.lobbyID ?? 0}");
            Debug.Log($"   NetworkServer.active: {Mirror.NetworkServer.active}");
            Debug.Log($"   NetworkClient.isConnected: {Mirror.NetworkClient.isConnected}");
            
            // Check if game was launched from Steam
            bool launchedFromSteam = System.Environment.GetCommandLineArgs().Any(arg => arg.Contains("steam"));
            Debug.Log($"   Launched from Steam: {launchedFromSteam}");
            
            if (SteamLobby.Instance != null && SteamLobby.Instance.lobbyID != 0)
            {
                CSteamID lobbyID = new CSteamID(SteamLobby.Instance.lobbyID);
                Debug.Log($"   Lobby Owner: {SteamMatchmaking.GetLobbyOwner(lobbyID)}");
                Debug.Log($"   Lobby Members: {SteamMatchmaking.GetNumLobbyMembers(lobbyID)}");
            }

            // Check overlay manager state
            if (DynamicInvitationManager.Instance != null)
            {
                Debug.Log($"   DynamicInvitationManager: ✅ Available");
                Debug.Log($"   Currently showing invitation: {DynamicInvitationManager.Instance.IsShowingInvitation()}");
            }
            else
            {
                Debug.LogError("   DynamicInvitationManager: ❌ Not found!");
            }

            // Important note about testing
            Debug.Log("📝 IMPORTANT: For invitations to work properly:");
            Debug.Log("   1. Game should be launched from Steam (not Unity Editor)");
            Debug.Log("   2. Game should be added to Steam as non-Steam game");
            Debug.Log("   3. Both sender and receiver should have game launched from Steam");
        }

        [ContextMenu("Force Show Test Overlay")]
        public void ForceShowTestOverlay()
        {
            Debug.Log("🚀 FORCING test overlay display...");
            
            if (DynamicInvitationManager.Instance != null)
            {
                CSteamID testLobby = new CSteamID(123456789);
                CSteamID testInviter = new CSteamID(987654321);
                DynamicInvitationManager.Instance.TestShowInvitationForced(testLobby, testInviter);
            }
            else
            {
                Debug.LogError("❌ DynamicInvitationManager.Instance not found!");
            }
        }

        void OnGUI()
        {
            // Simple on-screen debug info
            if (!SteamManager.Initialized) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Steam: {SteamFriends.GetPersonaName()}");
            GUILayout.Label($"Lobby: {SteamLobby.Instance?.lobbyID ?? 0}");
            
            if (DynamicInvitationManager.Instance != null)
            {
                GUILayout.Label($"Invitation Showing: {DynamicInvitationManager.Instance.IsShowingInvitation()}");
            }

            if (GUILayout.Button("Test Overlay"))
            {
                TestInvitationOverlay();
            }

            if (GUILayout.Button("Log Steam State"))
            {
                LogSteamState();
            }

            if (GUILayout.Button("Close Overlay"))
            {
                if (DynamicInvitationManager.Instance != null)
                {
                    DynamicInvitationManager.Instance.ManualCloseOverlay();
                }
            }

            GUILayout.EndArea();
        }
    }
}
