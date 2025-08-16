using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

namespace UpWeGo
{
    public class SteamFriendsUIManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject friendsPopup;
        public Transform friendsListParent;
        public GameObject friendItemPrefab;
        public Button closeButton;
        
        [Header("Panel Management")]
        public PanelSwapper panelSwapper;
        
        private List<GameObject> friendItems = new List<GameObject>();
        private string previousPanel = "LobbyPanel";
        
        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseFriendsPopup);
                
            friendsPopup.SetActive(false);
        }
        
        public void ShowFriendsPopup()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            // Use PanelSwapper to properly manage panel visibility
            if (panelSwapper != null)
            {
                panelSwapper.SwapPanel("FriendsPanel");
            }
            else
            {
                // Fallback to direct activation if PanelSwapper is not assigned
                friendsPopup.SetActive(true);
            }
            
            PopulateFriendsList();
        }
        
        public void CloseFriendsPopup()
        {
            ClearFriendsList();
            
            // Return to the previous panel using PanelSwapper
            if (panelSwapper != null)
            {
                panelSwapper.SwapPanel(previousPanel);
            }
            else
            {
                // Fallback to direct deactivation if PanelSwapper is not assigned
                friendsPopup.SetActive(false);
            }
        }
        
        private void PopulateFriendsList()
        {
            ClearFriendsList();
            
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            
            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string friendName = SteamFriends.GetFriendPersonaName(friendID);
                EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendID);
                
                // Only show online friends
                if (friendState != EPersonaState.k_EPersonaStateOffline)
                {
                    CreateFriendItem(friendID, friendName, friendState);
                }
            }
        }
        
        private void CreateFriendItem(CSteamID friendID, string friendName, EPersonaState friendState)
        {
            GameObject friendItem = Instantiate(friendItemPrefab, friendsListParent);
            friendItems.Add(friendItem);
            
            // Setup using FriendItem component
            FriendItem friendItemComponent = friendItem.GetComponent<FriendItem>();
            if (friendItemComponent != null)
            {
                friendItemComponent.SetupFriendItem(friendID, friendName, GetStatusText(friendState));
            }
            else
            {
                // Fallback for manual setup if FriendItem component is not used
                TextMeshProUGUI nameText = friendItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statusText = friendItem.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
                Button inviteButton = friendItem.transform.Find("InviteButton").GetComponent<Button>();
                
                if (nameText != null) nameText.text = friendName;
                if (statusText != null) statusText.text = GetStatusText(friendState);
                if (inviteButton != null) inviteButton.onClick.AddListener(() => InviteFriend(friendID, friendName));
            }
        }
        
        private string GetStatusText(EPersonaState state)
        {
            switch (state)
            {
                case EPersonaState.k_EPersonaStateOnline:
                    return "Online";
                case EPersonaState.k_EPersonaStateBusy:
                    return "Busy";
                case EPersonaState.k_EPersonaStateAway:
                    return "Away";
                case EPersonaState.k_EPersonaStateSnooze:
                    return "Snooze";
                case EPersonaState.k_EPersonaStateLookingToTrade:
                    return "Looking to Trade";
                case EPersonaState.k_EPersonaStateLookingToPlay:
                    return "Looking to Play";
                default:
                    return "Online";
            }
        }
        
        private void InviteFriend(CSteamID friendID, string friendName)
        {
            if (SteamLobby.Instance == null || SteamLobby.Instance.lobbyID == 0)
            {
                Debug.LogError("No active lobby to invite to!");
                return;
            }
            
            CSteamID lobbyID = new CSteamID(SteamLobby.Instance.lobbyID);
            bool success = SteamMatchmaking.InviteUserToLobby(lobbyID, friendID);
            
            if (success)
            {
                Debug.Log($"Invited {friendName} to lobby");
                // You could show a success message here
            }
            else
            {
                Debug.LogError($"Failed to invite {friendName} to lobby");
                // You could show an error message here
            }
        }
        
        private void ClearFriendsList()
        {
            foreach (GameObject item in friendItems)
            {
                if (item != null)
                    Destroy(item);
            }
            friendItems.Clear();
        }
        
        public void ShowFriendsPopupFromPanel(string fromPanel)
        {
            previousPanel = fromPanel;
            ShowFriendsPopup();
        }
        
        void OnDestroy()
        {
            ClearFriendsList();
        }
    }
}