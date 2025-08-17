using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    public class UISetupDebugger : MonoBehaviour
    {
        [Header("Debug References")]
        public SteamFriendsUIManager steamFriendsUIManager;
        public GameObject friendItemPrefab;
        
        [ContextMenu("Debug UI Setup")]
        public void DebugUISetup()
        {
            Debug.Log("=== UI SETUP DEBUG ===");
            
            // Check SteamFriendsUIManager
            if (steamFriendsUIManager == null)
            {
                Debug.LogError("❌ SteamFriendsUIManager reference is NULL!");
                return;
            }
            
            Debug.Log("✅ SteamFriendsUIManager found");
            
            // Check friendItemPrefab
            if (steamFriendsUIManager.friendItemPrefab == null)
            {
                Debug.LogError("❌ friendItemPrefab is NULL in SteamFriendsUIManager!");
            }
            else
            {
                Debug.Log("✅ friendItemPrefab assigned");
                DebugFriendItemPrefab(steamFriendsUIManager.friendItemPrefab);
            }
            
            // Check friendsListParent
            if (steamFriendsUIManager.friendsListParent == null)
            {
                Debug.LogError("❌ friendsListParent is NULL in SteamFriendsUIManager!");
            }
            else
            {
                Debug.Log("✅ friendsListParent assigned");
            }
            
            // Check closeButton
            if (steamFriendsUIManager.closeButton == null)
            {
                Debug.LogError("❌ closeButton is NULL in SteamFriendsUIManager!");
            }
            else
            {
                Debug.Log("✅ closeButton assigned");
            }
            
            // Check panelSwapper
            if (steamFriendsUIManager.panelSwapper == null)
            {
                Debug.LogWarning("⚠️ panelSwapper is NULL - will use fallback mode");
            }
            else
            {
                Debug.Log("✅ panelSwapper assigned");
            }
            
            Debug.Log("=== END UI DEBUG ===");
        }
        
        private void DebugFriendItemPrefab(GameObject prefab)
        {
            Debug.Log("--- Friend Item Prefab Debug ---");
            
            // Check FriendItem component
            FriendItem friendItemComponent = prefab.GetComponent<FriendItem>();
            if (friendItemComponent == null)
            {
                Debug.LogError("❌ FriendItem component missing on prefab!");
                return;
            }
            
            Debug.Log("✅ FriendItem component found");
            
            // Check nameText
            if (friendItemComponent.nameText == null)
            {
                Debug.LogError("❌ nameText is NULL in FriendItem component!");
            }
            else
            {
                Debug.Log($"✅ nameText assigned: {friendItemComponent.nameText.name}");
            }
            
            // Check statusText
            if (friendItemComponent.statusText == null)
            {
                Debug.LogError("❌ statusText is NULL in FriendItem component!");
            }
            else
            {
                Debug.Log($"✅ statusText assigned: {friendItemComponent.statusText.name}");
            }
            
            // Check inviteButton
            if (friendItemComponent.inviteButton == null)
            {
                Debug.LogError("❌ inviteButton is NULL in FriendItem component!");
            }
            else
            {
                Debug.Log($"✅ inviteButton assigned: {friendItemComponent.inviteButton.name}");
                
                // Check if button is interactable
                if (!friendItemComponent.inviteButton.interactable)
                {
                    Debug.LogWarning("⚠️ inviteButton is not interactable!");
                }
                else
                {
                    Debug.Log("✅ inviteButton is interactable");
                }
            }
        }
        
        [ContextMenu("Create Test Friend Item")]
        public void CreateTestFriendItem()
        {
            if (steamFriendsUIManager == null || steamFriendsUIManager.friendItemPrefab == null)
            {
                Debug.LogError("❌ Cannot create test item - missing references!");
                return;
            }
            
            if (steamFriendsUIManager.friendsListParent == null)
            {
                Debug.LogError("❌ Cannot create test item - friendsListParent is null!");
                return;
            }
            
            Debug.Log("🧪 Creating test friend item...");
            
            GameObject testItem = Instantiate(steamFriendsUIManager.friendItemPrefab, steamFriendsUIManager.friendsListParent);
            FriendItem friendItemComponent = testItem.GetComponent<FriendItem>();
            
            if (friendItemComponent != null)
            {
                // Use a fake SteamID for testing
                friendItemComponent.SetupFriendItem(new Steamworks.CSteamID(12345), "Test Friend", "Online");
                Debug.Log("✅ Test friend item created successfully!");
            }
            else
            {
                Debug.LogError("❌ Failed to get FriendItem component from instantiated prefab!");
            }
        }
    }
}