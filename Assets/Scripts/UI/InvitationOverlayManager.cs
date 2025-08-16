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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            overlayCanvasGroup = invitationOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = invitationOverlay.AddComponent<CanvasGroup>();
            }
            
            // Setup button listeners
            acceptButton.onClick.AddListener(AcceptInvitation);
            declineButton.onClick.AddListener(DeclineInvitation);
            
            // Hide overlay initially
            HideOverlay();
        }
        
        public void ShowInvitationOverlay(CSteamID lobbyID, CSteamID inviterID)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            currentLobbyInvite = lobbyID;
            inviterSteamID = inviterID;
            
            // Get inviter's name
            string inviterName = SteamFriends.GetFriendPersonaName(inviterID);
            
            // Update UI text
            inviterNameText.text = inviterName;
            invitationMessageText.text = $"wants you to join their lobby";
            
            // Show overlay with animation
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
        
        void OnDestroy()
        {
            if (autoDeclineCoroutine != null)
            {
                StopCoroutine(autoDeclineCoroutine);
            }
        }
    }
}