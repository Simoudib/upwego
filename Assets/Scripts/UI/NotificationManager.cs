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
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup == null)
            {
                notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            
            HideNotification();
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
            // Stop any current notification
            if (currentNotificationCoroutine != null)
            {
                StopCoroutine(currentNotificationCoroutine);
            }
            
            // Set notification content
            notificationText.text = message;
            notificationBackground.color = backgroundColor;
            
            // Start notification animation
            currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
        }
        
        private IEnumerator ShowNotificationCoroutine()
        {
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
        }
        
        private void HideNotification()
        {
            notificationCanvasGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }
    }
}