using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections;
using Mirror;

namespace UpWeGo
{
    public class InvitationOverlayManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject invitationOverlay;
        public TextMeshProUGUI inviterNameText;
        public TextMeshProUGUI invitationMessageText;
        public Button acceptButton;
        public Button declineButton;
        
        [Header("Animation Settings")]
        public float fadeInDuration = 0.3f;
        public float displayDuration = 10f; // Auto-decline after 10 seconds
        
        private CSteamID currentLobbyInvite;
        private CSteamID inviterSteamID;
        private CanvasGroup overlayCanvasGroup;
        private Coroutine autoDeclineCoroutine;
        
        public static InvitationOverlayManager Instance;
        
        void Awake()
        {
            Debug.Log($"InvitationOverlayManager Awake called on {gameObject.name}");
            
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("✅ InvitationOverlayManager.Instance set successfully");
                // Only use DontDestroyOnLoad if this is a persistent UI element
                // Comment this out if your UI is part of a scene-specific Canvas
                // DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"Duplicate InvitationOverlayManager found on {gameObject.name}, destroying...");
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            Debug.Log("InvitationOverlayManager Start called");
            
            // Auto-assign invitationOverlay if not set
            if (invitationOverlay == null)
            {
                invitationOverlay = gameObject;
                Debug.Log("✅ Auto-assigned invitationOverlay to this GameObject");
            }
            
            // Auto-find UI references if not assigned
            FindUIReferences();
            
            overlayCanvasGroup = invitationOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = invitationOverlay.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to invitation overlay");
            }
            
            // Setup button listeners
            SetupButtonListeners();
            
            // Hide overlay initially
            HideOverlay();
            Debug.Log("✅ InvitationOverlayManager setup complete");
        }

        private void FindUIReferences()
        {
            // Find UI elements by name or tag within this GameObject and its children
            if (inviterNameText == null)
            {
                inviterNameText = GetComponentInChildren<TextMeshProUGUI>();
                if (inviterNameText == null)
                {
                    // Try to find by name
                    Transform inviterNameTransform = transform.Find("InviterName");
                    if (inviterNameTransform != null)
                        inviterNameText = inviterNameTransform.GetComponent<TextMeshProUGUI>();
                }
                
                if (inviterNameText != null)
                    Debug.Log("✅ Found inviterNameText automatically");
                else
                    Debug.LogWarning("⚠️ Could not find inviterNameText - please assign manually or name the GameObject 'InviterName'");
            }

            if (invitationMessageText == null)
            {
                // Find the second TextMeshProUGUI if first one was inviterNameText
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 1)
                {
                    invitationMessageText = texts[1]; // Use second text component
                }
                else
                {
                    // Try to find by name
                    Transform messageTransform = transform.Find("InvitationMessage");
                    if (messageTransform != null)
                        invitationMessageText = messageTransform.GetComponent<TextMeshProUGUI>();
                }
                
                if (invitationMessageText != null)
                    Debug.Log("✅ Found invitationMessageText automatically");
                else
                    Debug.LogWarning("⚠️ Could not find invitationMessageText - please assign manually or name the GameObject 'InvitationMessage'");
            }

            if (acceptButton == null)
            {
                // Try to find by name first
                Transform acceptTransform = transform.Find("AcceptButton");
                if (acceptTransform != null)
                {
                    acceptButton = acceptTransform.GetComponent<Button>();
                }
                else
                {
                    // Find first button
                    acceptButton = GetComponentInChildren<Button>();
                }
                
                if (acceptButton != null)
                    Debug.Log("✅ Found acceptButton automatically");
                else
                    Debug.LogWarning("⚠️ Could not find acceptButton - please assign manually or name the GameObject 'AcceptButton'");
            }

            if (declineButton == null)
            {
                // Try to find by name first
                Transform declineTransform = transform.Find("DeclineButton");
                if (declineTransform != null)
                {
                    declineButton = declineTransform.GetComponent<Button>();
                }
                else
                {
                    // Find second button if first was accept button
                    Button[] buttons = GetComponentsInChildren<Button>();
                    if (buttons.Length > 1)
                    {
                        declineButton = buttons[1]; // Use second button
                    }
                }
                
                if (declineButton != null)
                    Debug.Log("✅ Found declineButton automatically");
                else
                    Debug.LogWarning("⚠️ Could not find declineButton - please assign manually or name the GameObject 'DeclineButton'");
            }
        }

        private void SetupButtonListeners()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveAllListeners(); // Clear existing listeners
                acceptButton.onClick.AddListener(AcceptInvitation);
                Debug.Log("✅ Accept button listener added");
            }
            else
            {
                Debug.LogError("❌ acceptButton is null!");
            }
            
            if (declineButton != null)
            {
                declineButton.onClick.RemoveAllListeners(); // Clear existing listeners
                declineButton.onClick.AddListener(DeclineInvitation);
                Debug.Log("✅ Decline button listener added");
            }
            else
            {
                Debug.LogError("❌ declineButton is null!");
            }
        }
        
        public void ShowInvitationOverlay(CSteamID lobbyID, CSteamID inviterID)
        {
            Debug.Log($"🎯 ShowInvitationOverlay called - Lobby: {lobbyID}, Inviter: {inviterID}");
            
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }

            // Try to find UI references again if they're null
            if (inviterNameText == null || invitationMessageText == null || acceptButton == null || declineButton == null)
            {
                Debug.Log("🔄 Some UI references are null, attempting to find them again...");
                FindUIReferences();
                SetupButtonListeners();
            }
            
            currentLobbyInvite = lobbyID;
            inviterSteamID = inviterID;
            
            // Get inviter's name
            string inviterName = SteamFriends.GetFriendPersonaName(inviterID);
            Debug.Log($"Inviter name: {inviterName}");
            
            // Update UI text
            if (inviterNameText != null)
            {
                inviterNameText.text = inviterName;
                Debug.Log("✅ Updated inviter name text");
            }
            else
            {
                Debug.LogError("❌ inviterNameText is still null after auto-find!");
            }
            
            if (invitationMessageText != null)
            {
                invitationMessageText.text = $"wants you to join their lobby";
                Debug.Log("✅ Updated invitation message text");
            }
            else
            {
                Debug.LogError("❌ invitationMessageText is still null after auto-find!");
            }
            
            // Show overlay with animation
            Debug.Log("Starting overlay animation...");
            if (this != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(ShowOverlayAnimated());
                
                // Start auto-decline timer
                if (autoDeclineCoroutine != null)
                {
                    StopCoroutine(autoDeclineCoroutine);
                }
                autoDeclineCoroutine = StartCoroutine(AutoDeclineAfterDelay());
            }
            else
            {
                Debug.LogError("❌ Cannot start coroutines - GameObject is inactive or destroyed!");
            }
        }
        
        private IEnumerator ShowOverlayAnimated()
        {
            if (invitationOverlay == null || overlayCanvasGroup == null)
            {
                Debug.LogError("❌ Cannot show overlay - objects are null!");
                yield break;
            }

            invitationOverlay.SetActive(true);
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = true;
            overlayCanvasGroup.blocksRaycasts = true;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration && overlayCanvasGroup != null)
            {
                elapsedTime += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            if (overlayCanvasGroup != null)
                overlayCanvasGroup.alpha = 1f;
        }
        
        private IEnumerator HideOverlayAnimated()
        {
            if (overlayCanvasGroup == null)
            {
                Debug.LogWarning("⚠️ Cannot hide overlay - CanvasGroup is null!");
                yield break;
            }

            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
            
            float elapsedTime = 0f;
            float startAlpha = overlayCanvasGroup.alpha;
            
            while (elapsedTime < fadeInDuration && overlayCanvasGroup != null)
            {
                elapsedTime += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            if (overlayCanvasGroup != null)
                overlayCanvasGroup.alpha = 0f;
                
            if (invitationOverlay != null)
                invitationOverlay.SetActive(false);
        }
        
        private IEnumerator AutoDeclineAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            DeclineInvitation();
        }
        
        public void AcceptInvitation()
        {
            if (currentLobbyInvite.IsValid())
            {
                Debug.Log($"Accepting invitation to lobby: {currentLobbyInvite}");
                
                // Disconnect from current session if connected
                if (NetworkClient.isConnected || NetworkClient.active)
                {
                    Debug.Log("Disconnecting from current session before joining new lobby");
                    NetworkManager.singleton.StopClient();
                    NetworkClient.Shutdown();
                }
                
                // Join the lobby
                SteamMatchmaking.JoinLobby(currentLobbyInvite);
                
                HideOverlay();
            }
        }
        
        public void DeclineInvitation()
        {
            Debug.Log("Invitation declined");
            HideOverlay();
        }
        
        private void HideOverlay()
        {
            if (autoDeclineCoroutine != null)
            {
                StopCoroutine(autoDeclineCoroutine);
                autoDeclineCoroutine = null;
            }
            
            if (this != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(HideOverlayAnimated());
            }
            else if (invitationOverlay != null)
            {
                // If we can't run coroutine, just hide immediately
                invitationOverlay.SetActive(false);
            }
            
            // Clear invitation data
            currentLobbyInvite = CSteamID.Nil;
            inviterSteamID = CSteamID.Nil;
        }
        
        // Public method to check if overlay is currently showing
        public bool IsShowingInvitation()
        {
            if (invitationOverlay == null || overlayCanvasGroup == null)
                return false;
                
            return invitationOverlay.activeInHierarchy && overlayCanvasGroup.alpha > 0f;
        }
        
        [ContextMenu("Test Show Overlay")]
        public void TestShowOverlay()
        {
            Debug.Log("🧪 Testing overlay directly...");
            var fakeLobbyID = new CSteamID(123456789);
            var fakeInviterID = new CSteamID(987654321);
            ShowInvitationOverlay(fakeLobbyID, fakeInviterID);
        }
        
        void OnDestroy()
        {
            if (autoDeclineCoroutine != null)
            {
                StopCoroutine(autoDeclineCoroutine);
            }
            
            if (Instance == this)
            {
                Instance = null;
                Debug.Log("InvitationOverlayManager.Instance cleared");
            }
        }
    }
}