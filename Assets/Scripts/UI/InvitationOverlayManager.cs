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
            
            if (invitationOverlay == null)
            {
                Debug.LogError("❌ invitationOverlay is null! Please assign it in the inspector.");
                return;
            }
            
            overlayCanvasGroup = invitationOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = invitationOverlay.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to invitation overlay");
            }
            
            // Setup button listeners
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(AcceptInvitation);
                Debug.Log("✅ Accept button listener added");
            }
            else
            {
                Debug.LogError("❌ acceptButton is null!");
            }
            
            if (declineButton != null)
            {
                declineButton.onClick.AddListener(DeclineInvitation);
                Debug.Log("✅ Decline button listener added");
            }
            else
            {
                Debug.LogError("❌ declineButton is null!");
            }
            
            // Hide overlay initially
            HideOverlay();
            Debug.Log("✅ InvitationOverlayManager setup complete");
        }
        
        public void ShowInvitationOverlay(CSteamID lobbyID, CSteamID inviterID)
        {
            Debug.Log($"🎯 ShowInvitationOverlay called - Lobby: {lobbyID}, Inviter: {inviterID}");
            
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
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
                Debug.LogError("❌ inviterNameText is null!");
            }
            
            if (invitationMessageText != null)
            {
                invitationMessageText.text = $"wants you to join their lobby";
                Debug.Log("✅ Updated invitation message text");
            }
            else
            {
                Debug.LogError("❌ invitationMessageText is null!");
            }
            
            // Show overlay with animation
            Debug.Log("Starting overlay animation...");
            StartCoroutine(ShowOverlayAnimated());
            
            // Start auto-decline timer
            if (autoDeclineCoroutine != null)
            {
                StopCoroutine(autoDeclineCoroutine);
            }
            autoDeclineCoroutine = StartCoroutine(AutoDeclineAfterDelay());
        }
        
        private IEnumerator ShowOverlayAnimated()
        {
            invitationOverlay.SetActive(true);
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = true;
            overlayCanvasGroup.blocksRaycasts = true;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            overlayCanvasGroup.alpha = 1f;
        }
        
        private IEnumerator HideOverlayAnimated()
        {
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
            
            float elapsedTime = 0f;
            float startAlpha = overlayCanvasGroup.alpha;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            overlayCanvasGroup.alpha = 0f;
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
            
            StartCoroutine(HideOverlayAnimated());
            
            // Clear invitation data
            currentLobbyInvite = CSteamID.Nil;
            inviterSteamID = CSteamID.Nil;
        }
        
        // Public method to check if overlay is currently showing
        public bool IsShowingInvitation()
        {
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