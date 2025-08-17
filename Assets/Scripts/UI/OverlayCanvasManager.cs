using UnityEngine;
using UnityEngine.UI;

namespace UpWeGo
{
    /// <summary>
    /// Manages a separate Canvas for overlays that should appear on top of all other UI.
    /// This Canvas is independent of the PanelSwapper system.
    /// </summary>
    public class OverlayCanvasManager : MonoBehaviour
    {
        [Header("Canvas Settings")]
        public Canvas overlayCanvas;
        public int sortingOrder = 100; // High value to appear on top
        
        [Header("Overlay References")]
        public GameObject invitationOverlay;
        public GameObject notificationPanel;
        
        public static OverlayCanvasManager Instance;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("✅ OverlayCanvasManager.Instance set successfully");
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"Duplicate OverlayCanvasManager found, destroying...");
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            // Move DontDestroyOnLoad to Start() to avoid race conditions with NetworkManager
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("✅ OverlayCanvasManager marked as DontDestroyOnLoad");
            }
            
            SetupOverlayCanvas();
        }
        
        private void SetupOverlayCanvas()
        {
            if (overlayCanvas == null)
            {
                overlayCanvas = GetComponent<Canvas>();
            }
            
            if (overlayCanvas == null)
            {
                Debug.LogError("❌ No Canvas component found! Please add a Canvas component.");
                return;
            }
            
            // Configure the canvas for overlays
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = sortingOrder;
            
            // Add GraphicRaycaster if missing
            if (overlayCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                overlayCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Added GraphicRaycaster to overlay canvas");
            }
            
            Debug.Log($"✅ Overlay Canvas configured with sorting order: {sortingOrder}");
            
            // Setup managers for overlays
            SetupInvitationOverlay();
            SetupNotificationPanel();
        }
        
        private void SetupInvitationOverlay()
        {
            if (invitationOverlay == null)
            {
                Debug.LogWarning("⚠️ invitationOverlay not assigned");
                return;
            }
            
            // Add InvitationOverlayManager if missing
            InvitationOverlayManager invitationManager = invitationOverlay.GetComponent<InvitationOverlayManager>();
            if (invitationManager == null)
            {
                invitationManager = invitationOverlay.AddComponent<InvitationOverlayManager>();
                Debug.Log("✅ Added InvitationOverlayManager to invitation overlay");
            }
            
            // Auto-assign the overlay reference
            invitationManager.invitationOverlay = invitationOverlay;
            
            // Hide initially
            invitationOverlay.SetActive(false);
            
            Debug.Log("✅ Invitation overlay setup complete");
        }
        
        private void SetupNotificationPanel()
        {
            if (notificationPanel == null)
            {
                Debug.LogWarning("⚠️ notificationPanel not assigned");
                return;
            }
            
            // Add PersistentNotificationManager to this GameObject (not the panel)
            PersistentNotificationManager notificationManager = GetComponent<PersistentNotificationManager>();
            if (notificationManager == null)
            {
                notificationManager = gameObject.AddComponent<PersistentNotificationManager>();
                Debug.Log("✅ Added PersistentNotificationManager to overlay canvas manager");
            }
            
            // Auto-assign the panel reference
            notificationManager.notificationPanel = notificationPanel;
            
            // Hide initially
            notificationPanel.SetActive(false);
            
            Debug.Log("✅ Notification panel setup complete");
        }
        
        [ContextMenu("Test All Overlays")]
        public void TestAllOverlays()
        {
            Debug.Log("🧪 Testing all overlays...");
            
            // Test notification
            if (PersistentNotificationManager.Instance != null)
            {
                PersistentNotificationManager.Instance.ShowSuccessNotification("Test overlay notification!");
            }
            
            // Test invitation overlay
            if (InvitationOverlayManager.Instance != null)
            {
                var fakeLobbyID = new Steamworks.CSteamID(123456789);
                var fakeInviterID = new Steamworks.CSteamID(987654321);
                InvitationOverlayManager.Instance.ShowInvitationOverlay(fakeLobbyID, fakeInviterID);
            }
        }
        
        /// <summary>
        /// Creates a basic overlay structure for testing
        /// </summary>
        [ContextMenu("Create Basic Overlay Structure")]
        public void CreateBasicOverlayStructure()
        {
            Debug.Log("🔧 Creating basic overlay structure...");
            
            if (overlayCanvas == null)
            {
                Debug.LogError("❌ No overlay canvas found!");
                return;
            }
            
            // Create notification panel if missing
            if (notificationPanel == null)
            {
                notificationPanel = CreateNotificationPanel();
            }
            
            // Create invitation overlay if missing
            if (invitationOverlay == null)
            {
                invitationOverlay = CreateInvitationOverlay();
            }
            
            // Setup the overlays
            SetupInvitationOverlay();
            SetupNotificationPanel();
            
            Debug.Log("✅ Basic overlay structure created!");
        }
        
        private GameObject CreateNotificationPanel()
        {
            GameObject panel = new GameObject("NotificationPanel");
            panel.transform.SetParent(overlayCanvas.transform, false);
            
            // Add RectTransform
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -50);
            rect.sizeDelta = new Vector2(300, 60);
            
            // Add background
            Image bg = panel.AddComponent<Image>();
            bg.color = Color.green;
            
            // Add text
            GameObject textObj = new GameObject("NotificationText");
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Notification";
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = Color.white;
            
            panel.SetActive(false);
            Debug.Log("✅ Created basic notification panel");
            
            return panel;
        }
        
        private GameObject CreateInvitationOverlay()
        {
            GameObject overlay = new GameObject("InvitationOverlay");
            overlay.transform.SetParent(overlayCanvas.transform, false);
            
            // Full screen overlay
            RectTransform rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Semi-transparent background
            Image bg = overlay.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            
            // Create content panel
            GameObject content = new GameObject("Content");
            content.transform.SetParent(overlay.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(400, 200);
            
            Image contentBg = content.AddComponent<Image>();
            contentBg.color = Color.white;
            
            // Add inviter name text
            GameObject nameText = new GameObject("InviterNameText");
            nameText.transform.SetParent(content.transform, false);
            var nameRect = nameText.AddComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, 30);
            nameRect.sizeDelta = new Vector2(350, 40);
            var nameTextComp = nameText.AddComponent<TMPro.TextMeshProUGUI>();
            nameTextComp.text = "Player Name";
            nameTextComp.alignment = TMPro.TextAlignmentOptions.Center;
            nameTextComp.fontSize = 20;
            
            // Add message text
            GameObject msgText = new GameObject("InvitationMessageText");
            msgText.transform.SetParent(content.transform, false);
            var msgRect = msgText.AddComponent<RectTransform>();
            msgRect.anchoredPosition = new Vector2(0, -10);
            msgRect.sizeDelta = new Vector2(350, 30);
            var msgTextComp = msgText.AddComponent<TMPro.TextMeshProUGUI>();
            msgTextComp.text = "wants you to join their lobby";
            msgTextComp.alignment = TMPro.TextAlignmentOptions.Center;
            msgTextComp.fontSize = 16;
            
            // Add Accept button
            GameObject acceptBtn = new GameObject("AcceptButton");
            acceptBtn.transform.SetParent(content.transform, false);
            var acceptRect = acceptBtn.AddComponent<RectTransform>();
            acceptRect.anchoredPosition = new Vector2(-80, -60);
            acceptRect.sizeDelta = new Vector2(100, 30);
            var acceptButton = acceptBtn.AddComponent<Button>();
            var acceptImg = acceptBtn.AddComponent<Image>();
            acceptImg.color = Color.green;
            
            // Add Decline button
            GameObject declineBtn = new GameObject("DeclineButton");
            declineBtn.transform.SetParent(content.transform, false);
            var declineRect = declineBtn.AddComponent<RectTransform>();
            declineRect.anchoredPosition = new Vector2(80, -60);
            declineRect.sizeDelta = new Vector2(100, 30);
            var declineButton = declineBtn.AddComponent<Button>();
            var declineImg = declineBtn.AddComponent<Image>();
            declineImg.color = Color.red;
            
            overlay.SetActive(false);
            Debug.Log("✅ Created basic invitation overlay");
            
            return overlay;
        }
    }
}