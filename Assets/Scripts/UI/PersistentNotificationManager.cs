using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace UpWeGo
{
    /// <summary>
    /// A persistent notification manager that stays active even when panels are switched.
    /// This should be placed on a GameObject that doesn't get deactivated by PanelSwapper.
    /// </summary>
    public class PersistentNotificationManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject notificationPanel;
        public TextMeshProUGUI notificationText;
        public Image notificationBackground;
        
        [Header("Notification Colors")]
        public Color successColor = Color.green;
        public Color errorColor = Color.red;
        public Color infoColor = Color.blue;
        
        [Header("Animation Settings")]
        public float fadeInDuration = 0.3f;
        public float displayDuration = 3f;
        public float fadeOutDuration = 0.3f;
        
        public static PersistentNotificationManager Instance;
        
        private CanvasGroup notificationCanvasGroup;
        private Coroutine currentNotificationCoroutine;
        
        void Awake()
        {
            Debug.Log($"PersistentNotificationManager Awake called on {gameObject.name}");
            
            if (Instance == null)
            {
                Instance = this;
                // Don't use DontDestroyOnLoad here - let the parent OverlayCanvasManager handle persistence
                // DontDestroyOnLoad(gameObject);
                Debug.Log("✅ PersistentNotificationManager.Instance set successfully");
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"Duplicate PersistentNotificationManager found on {gameObject.name}, destroying...");
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            SetupNotificationPanel();
        }
        
        private void SetupNotificationPanel()
        {
            if (notificationPanel == null)
            {
                Debug.LogError("❌ notificationPanel is null! Please assign it in the inspector.");
                return;
            }
            
            // Find the notification panel if it's not assigned
            if (notificationPanel == null)
            {
                notificationPanel = GameObject.Find("NotificationPanel");
                if (notificationPanel != null)
                {
                    Debug.Log("✅ Auto-found NotificationPanel");
                }
            }
            
            if (notificationPanel != null)
            {
                // Remove any existing NotificationManager to avoid conflicts
                NotificationManager existingManager = notificationPanel.GetComponent<NotificationManager>();
                if (existingManager != null)
                {
                    Debug.LogWarning("⚠️ Removing conflicting NotificationManager from panel");
                    DestroyImmediate(existingManager);
                }
                
                notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (notificationCanvasGroup == null)
                {
                    notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                    Debug.Log("✅ Added CanvasGroup to notification panel");
                }
                
                // Auto-find components if not assigned
                if (notificationText == null)
                {
                    notificationText = notificationPanel.GetComponentInChildren<TextMeshProUGUI>();
                }
                
                if (notificationBackground == null)
                {
                    notificationBackground = notificationPanel.GetComponent<Image>();
                }
                
                HideNotification();
                Debug.Log("✅ PersistentNotificationManager setup complete");
            }
        }
        
        public void ShowSuccessNotification(string message)
        {
            ShowNotification(message, successColor);
        }
        
        public void ShowErrorNotification(string message)
        {
            ShowNotification(message, errorColor);
        }
        
        public void ShowInfoNotification(string message)
        {
            ShowNotification(message, infoColor);
        }
        
        private void ShowNotification(string message, Color backgroundColor)
        {
            Debug.Log($"🔔 [Persistent] Showing notification: {message}");
            
            // Re-setup if panel reference was lost (can happen with scene changes)
            if (notificationPanel == null)
            {
                SetupNotificationPanel();
            }
            
            if (notificationPanel == null)
            {
                Debug.LogWarning("⚠️ Cannot show notification - panel not found. Using console fallback.");
                Debug.Log($"💡 NOTIFICATION: {message}");
                return;
            }
            
            // Stop any current notification
            if (currentNotificationCoroutine != null)
            {
                StopCoroutine(currentNotificationCoroutine);
            }
            
            // Set notification content
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            if (notificationBackground != null)
            {
                notificationBackground.color = backgroundColor;
            }
            
            // Start notification animation
            currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
        }
        
        private IEnumerator ShowNotificationCoroutine()
        {
            Debug.Log("Starting persistent notification animation...");
            
            // Ensure panel is active and properly initialized
            bool wasActive = notificationPanel.activeInHierarchy;
            notificationPanel.SetActive(true);
            
            // If panel was just activated, wait a frame for initialization
            if (!wasActive)
            {
                yield return null; // Wait one frame for Awake/Start to complete
                
                // Re-setup components after activation
                if (notificationCanvasGroup == null)
                {
                    notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                    if (notificationCanvasGroup == null)
                    {
                        notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                    }
                }
                
                // Remove any conflicting NotificationManager that might have been added
                NotificationManager conflictingManager = notificationPanel.GetComponent<NotificationManager>();
                if (conflictingManager != null)
                {
                    Debug.LogWarning("⚠️ Removing conflicting NotificationManager that was added during activation");
                    DestroyImmediate(conflictingManager);
                }
            }
            
            // Start animation
            notificationCanvasGroup.alpha = 0f;
            
            // Fade in
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                notificationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            notificationCanvasGroup.alpha = 1f;
            
            // Display
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                notificationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            HideNotification();
            Debug.Log("✅ Persistent notification animation complete");
        }
        
        private void HideNotification()
        {
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.alpha = 0f;
            }
            
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
        
        [ContextMenu("Test Success Notification")]
        public void TestSuccessNotification()
        {
            Debug.Log("🧪 Testing persistent success notification...");
            ShowSuccessNotification("Persistent test success message!");
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Debug.Log("PersistentNotificationManager.Instance cleared");
            }
        }
    }
}