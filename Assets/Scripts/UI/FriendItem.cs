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
            if (SteamLobby.Instance == null || SteamLobby.Instance.lobbyID == 0)
            {
                Debug.LogError("No active lobby to invite to!");
                return;
            }
            
            CSteamID lobbyID = new CSteamID(SteamLobby.Instance.lobbyID);
            bool success = SteamMatchmaking.InviteUserToLobby(lobbyID, friendSteamID);
            
            if (success)
            {
                Debug.Log($"Invited {nameText.text} to lobby");
                inviteButton.interactable = false;
                inviteButton.GetComponentInChildren<TextMeshProUGUI>().text = "Invited";
            }
            else
            {
                Debug.LogError($"Failed to invite {nameText.text} to lobby");
            }
        }
    }
}