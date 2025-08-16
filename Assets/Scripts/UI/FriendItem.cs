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
        
        public void SetupFriendItem(CSteamID steamID, string friendName, string status)
        {
            friendSteamID = steamID;
            nameText.text = friendName;
            statusText.text = status;
            
            inviteButton.onClick.RemoveAllListeners();
            inviteButton.onClick.AddListener(OnInviteClicked);
        }
        
        private void OnInviteClicked()
        {
            Debug.Log($"OnInviteClicked called for {nameText.text}");
            
            if (SteamLobby.Instance == null)
            {
                Debug.LogError("SteamLobby.Instance is null!");
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowErrorNotification("SteamLobby not found!");
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
                
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowSuccessNotification($"Invitation sent to {nameText.text}");
                }
                else
                {
                    Debug.LogWarning("NotificationManager.Instance is null");
                }
            }
            else
            {
                Debug.LogError($"Failed to invite {nameText.text} to lobby");
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowErrorNotification($"Failed to invite {nameText.text}");
                }
                else
                {
                    Debug.LogWarning("NotificationManager.Instance is null");
                }
            }
        }
    }
}