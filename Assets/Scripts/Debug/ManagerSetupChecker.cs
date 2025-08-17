using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    public class ManagerSetupChecker : MonoBehaviour
    {
        [ContextMenu("Check All Manager Setup")]
        public void CheckAllManagerSetup()
        {
            Debug.Log("=== MANAGER SETUP CHECK ===");
            
            CheckInvitationOverlayManager();
            CheckNotificationManager();
            CheckPanelSwapper();
            
            Debug.Log("=== END MANAGER CHECK ===");
        }
        
        private void CheckInvitationOverlayManager()
        {
            Debug.Log("--- Checking InvitationOverlayManager ---");
            
            if (InvitationOverlayManager.Instance == null)
            {
                Debug.LogError("❌ InvitationOverlayManager.Instance is NULL!");
                Debug.LogError("   → Add InvitationOverlayManager script to InvitationOverlay GameObject");
                return;
            }
            
            Debug.Log("✅ InvitationOverlayManager.Instance found");
            
            var manager = InvitationOverlayManager.Instance;
            
            // Check invitationOverlay
            if (manager.invitationOverlay == null)
            {
                Debug.LogError("❌ invitationOverlay is NULL!");
            }
            else
            {
                Debug.Log($"✅ invitationOverlay assigned: {manager.invitationOverlay.name}");
                Debug.Log($"   Active: {manager.invitationOverlay.activeInHierarchy}");
            }
            
            // Check inviterNameText
            if (manager.inviterNameText == null)
            {
                Debug.LogError("❌ inviterNameText is NULL!");
            }
            else
            {
                Debug.Log($"✅ inviterNameText assigned: {manager.inviterNameText.name}");
            }
            
            // Check invitationMessageText
            if (manager.invitationMessageText == null)
            {
                Debug.LogError("❌ invitationMessageText is NULL!");
            }
            else
            {
                Debug.Log($"✅ invitationMessageText assigned: {manager.invitationMessageText.name}");
            }
            
            // Check acceptButton
            if (manager.acceptButton == null)
            {
                Debug.LogError("❌ acceptButton is NULL!");
            }
            else
            {
                Debug.Log($"✅ acceptButton assigned: {manager.acceptButton.name}");
            }
            
            // Check declineButton
            if (manager.declineButton == null)
            {
                Debug.LogError("❌ declineButton is NULL!");
            }
            else
            {
                Debug.Log($"✅ declineButton assigned: {manager.declineButton.name}");
            }
        }
        
        private void CheckNotificationManager()
        {
            Debug.Log("--- Checking NotificationManager ---");
            
            if (NotificationManager.Instance == null)
            {
                Debug.LogWarning("⚠️ NotificationManager.Instance is NULL!");
                Debug.LogWarning("   → Add NotificationManager script to NotificationPanel GameObject");
                Debug.LogWarning("   → This is optional - system works without it");
                return;
            }
            
            Debug.Log("✅ NotificationManager.Instance found");
            
            var manager = NotificationManager.Instance;
            
            // Check notificationPanel
            if (manager.notificationPanel == null)
            {
                Debug.LogError("❌ notificationPanel is NULL!");
            }
            else
            {
                Debug.Log($"✅ notificationPanel assigned: {manager.notificationPanel.name}");
            }
            
            // Check notificationText
            if (manager.notificationText == null)
            {
                Debug.LogError("❌ notificationText is NULL!");
            }
            else
            {
                Debug.Log($"✅ notificationText assigned: {manager.notificationText.name}");
            }
            
            // Check notificationBackground
            if (manager.notificationBackground == null)
            {
                Debug.LogError("❌ notificationBackground is NULL!");
            }
            else
            {
                Debug.Log($"✅ notificationBackground assigned: {manager.notificationBackground.name}");
            }
        }
        
        private void CheckPanelSwapper()
        {
            Debug.Log("--- Checking PanelSwapper ---");
            
            PanelSwapper panelSwapper = FindObjectOfType<PanelSwapper>();
            if (panelSwapper == null)
            {
                Debug.LogError("❌ No PanelSwapper found in scene!");
                return;
            }
            
            Debug.Log($"✅ PanelSwapper found on: {panelSwapper.name}");
            Debug.Log($"   Panel count: {panelSwapper.panels.Count}");
            
            bool foundFriendsPanel = false;
            foreach (var panel in panelSwapper.panels)
            {
                Debug.Log($"   Panel: {panel.PanelName} on {panel.name}");
                if (panel.PanelName == "FriendsPanel")
                {
                    foundFriendsPanel = true;
                }
            }
            
            if (!foundFriendsPanel)
            {
                Debug.LogWarning("⚠️ No 'FriendsPanel' found in PanelSwapper!");
            }
        }
        
        [ContextMenu("Test Invitation Overlay Manually")]
        public void TestInvitationOverlayManually()
        {
            Debug.Log("🧪 Testing Invitation Overlay Manually...");
            
            // Try to find the manager even if Instance is null
            InvitationOverlayManager manager = InvitationOverlayManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("Instance is null, trying to find manager in scene...");
                manager = FindObjectOfType<InvitationOverlayManager>();
            }
            
            if (manager == null)
            {
                Debug.LogError("❌ Cannot test - No InvitationOverlayManager found in scene!");
                Debug.LogError("   Make sure InvitationOverlayManager script is attached to a GameObject");
                return;
            }
            
            Debug.Log($"✅ Found InvitationOverlayManager on: {manager.gameObject.name}");
            
            // Create fake data for testing
            var fakeLobbyID = new Steamworks.CSteamID(123456789);
            var fakeInviterID = new Steamworks.CSteamID(987654321);
            
            Debug.Log("Calling ShowInvitationOverlay...");
            manager.ShowInvitationOverlay(fakeLobbyID, fakeInviterID);
            Debug.Log("ShowInvitationOverlay called - check if overlay appears!");
        }
        
        [ContextMenu("Test Notification Manually")]
        public void TestNotificationManually()
        {
            Debug.Log("🧪 Testing Notification Manually...");
            
            // Try to find the manager even if Instance is null
            NotificationManager manager = NotificationManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("Instance is null, trying to find manager in scene...");
                manager = FindObjectOfType<NotificationManager>();
            }
            
            if (manager == null)
            {
                Debug.LogWarning("⚠️ Cannot test - No NotificationManager found in scene!");
                Debug.LogWarning("   NotificationManager is optional - system works without it");
                return;
            }
            
            Debug.Log($"✅ Found NotificationManager on: {manager.gameObject.name}");
            
            Debug.Log("Calling ShowSuccessNotification...");
            manager.ShowSuccessNotification("Test notification message!");
            Debug.Log("ShowSuccessNotification called - check if notification appears!");
        }
    }
}