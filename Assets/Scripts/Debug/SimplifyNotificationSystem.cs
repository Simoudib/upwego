using UnityEngine;

namespace UpWeGo
{
    public class SimplifyNotificationSystem : MonoBehaviour
    {
        [ContextMenu("Simplify to Regular NotificationManager")]
        public void SimplifyToRegularNotificationManager()
        {
            Debug.Log("🔧 Simplifying notification system...");
            
            // Find the overlay canvas
            OverlayCanvasManager overlayManager = FindObjectOfType<OverlayCanvasManager>();
            if (overlayManager == null)
            {
                Debug.LogError("❌ No OverlayCanvasManager found!");
                return;
            }
            
            // Remove PersistentNotificationManager if it exists
            PersistentNotificationManager persistentManager = overlayManager.GetComponent<PersistentNotificationManager>();
            if (persistentManager != null)
            {
                Debug.Log("Removing PersistentNotificationManager...");
                DestroyImmediate(persistentManager);
            }
            
            // Find the notification panel
            GameObject notificationPanel = overlayManager.notificationPanel;
            if (notificationPanel == null)
            {
                notificationPanel = GameObject.Find("NotificationPanel");
            }
            
            if (notificationPanel == null)
            {
                Debug.LogError("❌ No NotificationPanel found!");
                return;
            }
            
            // Add regular NotificationManager to the overlay canvas (not the panel)
            NotificationManager regularManager = overlayManager.GetComponent<NotificationManager>();
            if (regularManager == null)
            {
                regularManager = overlayManager.gameObject.AddComponent<NotificationManager>();
                Debug.Log("✅ Added NotificationManager to OverlayCanvas");
            }
            
            // Assign the notification panel reference
            regularManager.notificationPanel = notificationPanel;
            
            // Auto-assign other references
            if (regularManager.notificationText == null)
            {
                regularManager.notificationText = notificationPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }
            
            if (regularManager.notificationBackground == null)
            {
                regularManager.notificationBackground = notificationPanel.GetComponent<UnityEngine.UI.Image>();
            }
            
            Debug.Log("✅ Simplified to regular NotificationManager!");
            Debug.Log("💡 The NotificationManager is now on the OverlayCanvas, so it won't be affected by PanelSwapper");
        }
        
        [ContextMenu("Test Simplified Notification")]
        public void TestSimplifiedNotification()
        {
            Debug.Log("🧪 Testing simplified notification...");
            
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowSuccessNotification("Simplified notification test!");
                Debug.Log("✅ Regular NotificationManager is working!");
            }
            else
            {
                Debug.LogError("❌ NotificationManager.Instance is null!");
            }
        }
    }
}