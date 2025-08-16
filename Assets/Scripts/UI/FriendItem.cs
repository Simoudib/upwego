using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

namespace UpWeGo
{
    public class FriendItem : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statusText;
        public Button inviteButton;
        
        private CSteamID friendSteamID;
        
        [ContextMenu("Test Button Click")]
        public void TestButtonClick()
        {
            Debug.Log("🧪 TEST: Button click test called!");
            OnInviteClicked();
        }
        
        public void SetupFriendItem(CSteamID steamID, string friendName, string status)
        {
            friendSteamID = steamID;
            nameText.text = friendName;
            statusText.text = status;
            
            Debug.Log($"Setting up FriendItem for {friendName}");
            
            if (inviteButton == null)
            {
                Debug.LogError($"inviteButton is NULL for {friendName}! Check inspector references.");
                return;
            }
            
            inviteButton.onClick.RemoveAllListeners();
            inviteButton.onClick.AddListener(OnInviteClicked);
            
            Debug.Log($"Added OnInviteClicked listener to button for {friendName}");
        }
        
        private void OnInviteClicked()
        {
            Debug.Log($"🔥 BUTTON CLICKED! OnInviteClicked called for {nameText.text}");
            
            if (SteamLobby.Instance == null)
            {
                Debug.LogError("SteamLobby.Instance is null!");
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowErrorNotification("SteamLobby not found!");
                }
                else
                {
                    Debug.LogError("❌ SteamLobby not found! (NotificationManager not available)");
                }
                return;
            }
            
            if (SteamLobby.Instance.lobbyID == 0)
            {
                Debug.LogError("No active lobby to invite to! LobbyID is 0");
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowErrorNotification("No active lobby to invite to!");
                }
                else
                {
                    Debug.LogError("❌ No active lobby to invite to! (NotificationManager not available)");
                }
                return;
            }
            
            Debug.Log($"Attempting to invite {nameText.text} to lobby {SteamLobby.Instance.lobbyID}");
            
            CSteamID lobbyID = new CSteamID(SteamLobby.Instance.lobbyID);
            bool success = SteamMatchmaking.InviteUserToLobby(lobbyID, friendSteamID);
            
            Debug.Log($"InviteUserToLobby returned: {success}");
            
            if (success)
            {
                Debug.Log($"Successfully invited {nameText.text} to lobby");
                inviteButton.interactable = false;
                inviteButton.GetComponentInChildren<TextMeshProUGUI>().text = "Invited";
                
                // Use NotificationManager (should be on OverlayCanvas now)
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowSuccessNotification($"Invitation sent to {nameText.text}");
                }
                else
                {
                    Debug.Log($"✅ Invitation sent to {nameText.text} (NotificationManager not available)");
                }
            }
            else
            {
                Debug.LogError($"Failed to invite {nameText.text} to lobby");
                // Use NotificationManager (should be on OverlayCanvas now)
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowErrorNotification($"Failed to invite {nameText.text}");
                }
                else
                {
                    Debug.LogError($"❌ Failed to invite {nameText.text} (NotificationManager not available)");
                }
            }
        }
    }
}