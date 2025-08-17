using UnityEngine;

namespace UpWeGo
{
    public class NotificationFixHelper : MonoBehaviour
    {
        [ContextMenu("Fix Notification Conflicts")]
        public void FixNotificationConflicts()
        {
            Debug.Log("🔧 Fixing notification conflicts...");
            
            // Find all NotificationManager components
            NotificationManager[] managers = FindObjectsOfType<NotificationManager>();
            
            foreach (NotificationManager manager in managers)
            {
                Debug.Log($"Found NotificationManager on: {manager.gameObject.name}");
                
                // If it's on a NotificationPanel, remove it
                if (manager.gameObject.name.Contains("NotificationPanel"))
                {
                    Debug.Log($"Removing NotificationManager from {manager.gameObject.name}");
                    DestroyImmediate(manager);
                }
            }
            
            // Find PersistentNotificationManager
            PersistentNotificationManager persistentManager = FindObjectOfType<PersistentNotificationManager>();
            if (persistentManager != null)
            {
                Debug.Log($"✅ PersistentNotificationManager found on: {persistentManager.gameObject.name}");
                
                // Make sure it has a reference to the notification panel
                if (persistentManager.notificationPanel == null)
                {
                    GameObject panel = GameObject.Find("NotificationPanel");
                    if (panel != null)
                    {
                        persistentManager.notificationPanel = panel;
                        Debug.Log("✅ Auto-assigned NotificationPanel reference");
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No PersistentNotificationManager found!");
            }
            
            Debug.Log("✅ Notification conflict fix complete!");
        }
        
        [ContextMenu("Test Notification After Fix")]
        public void TestNotificationAfterFix()
        {
            Debug.Log("🧪 Testing notification after fix...");
            
            if (PersistentNotificationManager.Instance != null)
            {
                PersistentNotificationManager.Instance.ShowSuccessNotification("Test notification after fix!");
            }
            else
            {
                Debug.LogError("❌ PersistentNotificationManager.Instance is null!");
            }
        }
    }
}