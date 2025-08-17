using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    public class AutoSetupHelper : MonoBehaviour
    {
        [Header("Auto-Setup References")]
        public GameObject invitationOverlay;
        public GameObject notificationPanel;
        
        [ContextMenu("Auto-Setup InvitationOverlayManager")]
        public void AutoSetupInvitationOverlay()
        {
            Debug.Log("🔧 Auto-setting up InvitationOverlayManager...");
            
            if (invitationOverlay == null)
            {
                Debug.LogError("❌ invitationOverlay reference is null! Assign it in the inspector first.");
                return;
            }
            
            // Add InvitationOverlayManager if it doesn't exist
            InvitationOverlayManager manager = invitationOverlay.GetComponent<InvitationOverlayManager>();
            if (manager == null)
            {
                manager = invitationOverlay.AddComponent<InvitationOverlayManager>();
                Debug.Log("✅ Added InvitationOverlayManager component");
            }
            
            // Try to auto-assign references
            manager.invitationOverlay = invitationOverlay;
            
            // Look for text components
            TextMeshProUGUI[] texts = invitationOverlay.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                manager.inviterNameText = texts[0];
                manager.invitationMessageText = texts[1];
                Debug.Log($"✅ Auto-assigned text components: {texts[0].name}, {texts[1].name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find enough TextMeshProUGUI components. You need at least 2.");
            }
            
            // Look for buttons
            Button[] buttons = invitationOverlay.GetComponentsInChildren<Button>();
            if (buttons.Length >= 2)
            {
                manager.acceptButton = buttons[0];
                manager.declineButton = buttons[1];
                Debug.Log($"✅ Auto-assigned buttons: {buttons[0].name}, {buttons[1].name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find enough Button components. You need at least 2 (Accept/Decline).");
            }
            
            Debug.Log("🎉 InvitationOverlayManager setup complete!");
        }
        
        [ContextMenu("Auto-Setup NotificationManager")]
        public void AutoSetupNotificationManager()
        {
            Debug.Log("🔧 Auto-setting up NotificationManager...");
            
            if (notificationPanel == null)
            {
                Debug.LogError("❌ notificationPanel reference is null! Assign it in the inspector first.");
                return;
            }
            
            // Add NotificationManager if it doesn't exist
            NotificationManager manager = notificationPanel.GetComponent<NotificationManager>();
            if (manager == null)
            {
                manager = notificationPanel.AddComponent<NotificationManager>();
                Debug.Log("✅ Added NotificationManager component");
            }
            
            // Try to auto-assign references
            manager.notificationPanel = notificationPanel;
            
            // Look for text component
            TextMeshProUGUI text = notificationPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                manager.notificationText = text;
                Debug.Log($"✅ Auto-assigned text: {text.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find TextMeshProUGUI component for notification text.");
            }
            
            // Look for image component
            Image image = notificationPanel.GetComponent<Image>();
            if (image != null)
            {
                manager.notificationBackground = image;
                Debug.Log($"✅ Auto-assigned background image: {image.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find Image component for notification background.");
            }
            
            Debug.Log("🎉 NotificationManager setup complete!");
        }
        
        [ContextMenu("Create Simple Test UI")]
        public void CreateSimpleTestUI()
        {
            Debug.Log("🔧 Creating simple test UI...");
            
            // Create a simple invitation overlay for testing
            if (invitationOverlay == null)
            {
                GameObject canvas = GameObject.Find("Canvas");
                if (canvas == null)
                {
                    Debug.LogError("❌ No Canvas found in scene!");
                    return;
                }
                
                // Create invitation overlay
                invitationOverlay = new GameObject("TestInvitationOverlay");
                invitationOverlay.transform.SetParent(canvas.transform, false);
                
                // Add RectTransform and make it full screen
                RectTransform rect = invitationOverlay.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                // Add background
                Image bg = invitationOverlay.AddComponent<Image>();
                bg.color = new Color(0, 0, 0, 0.8f);
                
                // Add inviter name text
                GameObject nameTextObj = new GameObject("InviterNameText");
                nameTextObj.transform.SetParent(invitationOverlay.transform, false);
                TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
                nameText.text = "Player Name";
                nameText.fontSize = 24;
                nameText.alignment = TextAlignmentOptions.Center;
                RectTransform nameRect = nameTextObj.GetComponent<RectTransform>();
                nameRect.anchoredPosition = new Vector2(0, 50);
                nameRect.sizeDelta = new Vector2(400, 50);
                
                // Add message text
                GameObject msgTextObj = new GameObject("InvitationMessageText");
                msgTextObj.transform.SetParent(invitationOverlay.transform, false);
                TextMeshProUGUI msgText = msgTextObj.AddComponent<TextMeshProUGUI>();
                msgText.text = "wants you to join their lobby";
                msgText.fontSize = 18;
                msgText.alignment = TextAlignmentOptions.Center;
                RectTransform msgRect = msgTextObj.GetComponent<RectTransform>();
                msgRect.anchoredPosition = new Vector2(0, 0);
                msgRect.sizeDelta = new Vector2(400, 50);
                
                // Add Accept button
                GameObject acceptBtnObj = new GameObject("AcceptButton");
                acceptBtnObj.transform.SetParent(invitationOverlay.transform, false);
                Button acceptBtn = acceptBtnObj.AddComponent<Button>();
                Image acceptImg = acceptBtnObj.AddComponent<Image>();
                acceptImg.color = Color.green;
                RectTransform acceptRect = acceptBtnObj.GetComponent<RectTransform>();
                acceptRect.anchoredPosition = new Vector2(-100, -50);
                acceptRect.sizeDelta = new Vector2(80, 30);
                
                // Add Decline button
                GameObject declineBtnObj = new GameObject("DeclineButton");
                declineBtnObj.transform.SetParent(invitationOverlay.transform, false);
                Button declineBtn = declineBtnObj.AddComponent<Button>();
                Image declineImg = declineBtnObj.AddComponent<Image>();
                declineImg.color = Color.red;
                RectTransform declineRect = declineBtnObj.GetComponent<RectTransform>();
                declineRect.anchoredPosition = new Vector2(100, -50);
                declineRect.sizeDelta = new Vector2(80, 30);
                
                invitationOverlay.SetActive(false);
                
                Debug.Log("✅ Created simple test invitation overlay!");
            }
            
            // Auto-setup the manager
            AutoSetupInvitationOverlay();
        }
    }
}