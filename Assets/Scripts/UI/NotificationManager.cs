using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace UpWeGo
{
    public class NotificationManager : MonoBehaviour
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
        
        public static NotificationManager Instance;
        
        private CanvasGroup notificationCanvasGroup;
        private Coroutine currentNotificationCoroutine;
        
        void Awake()
        {
            Debug.Log($"NotificationManager Awake called on {gameObject.name}");
            
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("✅ NotificationManager.Instance set successfully");
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"Duplicate NotificationManager found on {gameObject.name}, destroying...");
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            Debug.Log("NotificationManager Start called");
            
            if (notificationPanel == null)
            {
                Debug.LogError("❌ notificationPanel is null! Please assign it in the inspector.");
                return;
            }
            
            // Ensure the GameObject is active so we can get/add components
            bool wasActive = notificationPanel.activeInHierarchy;
            if (!wasActive)
            {
                notificationPanel.SetActive(true);
                Debug.Log("Temporarily activated notification panel for setup");
            }
            
            notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup == null)
            {
                notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to notification panel");
            }
            
            // Hide notification initially (this will deactivate it)
            HideNotification();
            Debug.Log("✅ NotificationManager setup complete");
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
            Debug.Log($"🔔 Showing notification: {message}");
            
            if (notificationPanel == null)
            {
                Debug.LogError("❌ Cannot show notification - notificationPanel is null!");
                Debug.Log($"💡 Fallback: {message}"); // Fallback to console log
                return;
            }
            
            // Check if this GameObject is active
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("❌ Cannot start notification coroutine - NotificationManager GameObject is inactive!");
                Debug.Log($"💡 Fallback: {message}"); // Fallback to console log
                
                // Try to show a simple notification without animation
                ShowSimpleNotification(message, backgroundColor);
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
            else
            {
                Debug.LogError("❌ notificationText is null!");
            }
            
            if (notificationBackground != null)
            {
                notificationBackground.color = backgroundColor;
            }
            else
            {
                Debug.LogWarning("⚠️ notificationBackground is null - notification will have default color");
            }
            
            // Start notification animation
            currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
        }
        
        // Fallback method for when coroutines can't be started
        private void ShowSimpleNotification(string message, Color backgroundColor)
        {
            Debug.Log("🔄 Using simple notification fallback...");
            
            if (notificationPanel == null) return;
            
            // Set content
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            if (notificationBackground != null)
            {
                notificationBackground.color = backgroundColor;
            }
            
            // Show immediately without animation
            notificationPanel.SetActive(true);
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.alpha = 1f;
            }
            
            // Hide after delay using Invoke instead of coroutine
            Invoke(nameof(HideNotificationSimple), displayDuration);
        }
        
        private void HideNotificationSimple()
        {
            HideNotification();
        }
        
        private IEnumerator ShowNotificationCoroutine()
        {
            Debug.Log("Starting notification animation...");
            
            // Show notification
            notificationPanel.SetActive(true);
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
            Debug.Log("✅ Notification fade-in complete");
            
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
            Debug.Log("✅ Notification animation complete");
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
            Debug.Log("🧪 Testing success notification...");
            ShowSuccessNotification("Test success message!");
        }
        
        [ContextMenu("Test Error Notification")]
        public void TestErrorNotification()
        {
            Debug.Log("🧪 Testing error notification...");
            ShowErrorNotification("Test error message!");
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Debug.Log("NotificationManager.Instance cleared");
            }
        }
    }
}