using UnityEngine;
using Steamworks;

namespace UpWeGo
{
    public class SteamInviteDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        public KeyCode debugKey = KeyCode.F1;
        
        void Update()
        {
            if (Input.GetKeyDown(debugKey))
            {
                DebugSteamInviteSystem();
            }
        }
        
        [ContextMenu("Debug Steam Invite System")]
        public void DebugSteamInviteSystem()
        {
            Debug.Log("=== STEAM INVITE SYSTEM DEBUG ===");
            
            // Check Steam initialization
            Debug.Log($"Steam Initialized: {SteamManager.Initialized}");
            
            // Check SteamLobby instance
            if (SteamLobby.Instance != null)
            {
                Debug.Log($"SteamLobby.Instance found");
                Debug.Log($"Current Lobby ID: {SteamLobby.Instance.lobbyID}");
                Debug.Log($"Is in lobby: {SteamLobby.Instance.lobbyID != 0}");
            }
            else
            {
                Debug.LogError("SteamLobby.Instance is NULL!");
            }
            
            // Check managers
            Debug.Log($"InvitationOverlayManager.Instance: {(InvitationOverlayManager.Instance != null ? "Found" : "NULL")}");
            Debug.Log($"NotificationManager.Instance: {(NotificationManager.Instance != null ? "Found" : "NULL")}");
            
            // Check Steam friends
            if (SteamManager.Initialized)
            {
                int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                Debug.Log($"Steam Friends Count: {friendCount}");
                
                for (int i = 0; i < Mathf.Min(friendCount, 5); i++) // Show first 5 friends
                {
                    CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    string friendName = SteamFriends.GetFriendPersonaName(friendID);
                    EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendID);
                    Debug.Log($"  Friend {i}: {friendName} - {friendState}");
                }
            }
            
            Debug.Log("=== END DEBUG ===");
        }
        
        [ContextMenu("Test Invitation Overlay")]
        public void TestInvitationOverlay()
        {
            if (InvitationOverlayManager.Instance == null)
            {
                Debug.LogError("InvitationOverlayManager.Instance is null! Make sure it's set up in your scene.");
                return;
            }
            
            // Simulate receiving an invitation from yourself
            CSteamID fakeLobbyID = new CSteamID(12345678901234567);
            CSteamID myID = SteamUser.GetSteamID();
            
            Debug.Log("🧪 Testing invitation overlay with fake data...");
            InvitationOverlayManager.Instance.ShowInvitationOverlay(fakeLobbyID, myID);
        }
        
        [ContextMenu("Test Invite to First Online Friend")]
        public void TestInviteToFirstOnlineFriend()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam not initialized!");
                return;
            }
            
            if (SteamLobby.Instance == null || SteamLobby.Instance.lobbyID == 0)
            {
                Debug.LogError("No active lobby!");
                return;
            }
            
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            
            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendID);
                
                if (friendState != EPersonaState.k_EPersonaStateOffline)
                {
                    string friendName = SteamFriends.GetFriendPersonaName(friendID);
                    Debug.Log($"Testing invite to {friendName}");
                    
                    CSteamID lobbyID = new CSteamID(SteamLobby.Instance.lobbyID);
                    bool success = SteamMatchmaking.InviteUserToLobby(lobbyID, friendID);
                    
                    Debug.Log($"Invite result: {success}");
                    return;
                }
            }
            
            Debug.LogWarning("No online friends found to invite");
        }
    }
}