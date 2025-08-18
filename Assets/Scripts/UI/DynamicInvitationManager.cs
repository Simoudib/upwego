using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections;
using Mirror;

namespace UpWeGo
{
    public class DynamicInvitationManager : MonoBehaviour
    {
        [Header("Prefab References")]
        public GameObject invitationOverlayPrefab;
        
        [Header("Canvas Reference")]
        public Canvas overlayCanvas;
        
        public static DynamicInvitationManager Instance;
        
        private GameObject currentInvitationOverlay;
        private CSteamID currentLobbyInvite;
        private CSteamID inviterSteamID;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("✅ DynamicInvitationManager.Instance set successfully");
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"Duplicate DynamicInvitationManager found, destroying...");
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // Find overlay canvas if not assigned
            if (overlayCanvas == null)
            {
                overlayCanvas = FindObjectOfType<Canvas>();
                if (overlayCanvas != null)
                {
                    Debug.Log("✅ Auto-found overlay canvas");
                }
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

            if (invitationOverlayPrefab == null)
            {
                Debug.LogError("❌ invitationOverlayPrefab is not assigned!");
                return;
            }

            if (overlayCanvas == null)
            {
                Debug.LogError("❌ overlayCanvas is not assigned or found!");
                return;
            }

            // Debug the inviter ID issue
            if (!inviterID.IsValid() || inviterID.m_SteamID == 0)
            {
                Debug.LogWarning($"⚠️ Invalid inviter ID: {inviterID}. Trying to get lobby owner...");
                inviterID = SteamMatchmaking.GetLobbyOwner(lobbyID);
                Debug.Log($"🔄 Lobby owner ID: {inviterID}");
            }

            // Hide any existing invitation overlay
            HideInvitationOverlay();

            // Instantiate the invitation overlay prefab
            currentInvitationOverlay = Instantiate(invitationOverlayPrefab, overlayCanvas.transform);
            Debug.Log($"✅ Instantiated invitation overlay prefab. Active: {currentInvitationOverlay.activeSelf}");
            Debug.Log($"📍 Overlay position: {currentInvitationOverlay.transform.position}");
            Debug.Log($"📏 Canvas info: {overlayCanvas.name}, SortingOrder: {overlayCanvas.sortingOrder}, RenderMode: {overlayCanvas.renderMode}");

            // Ensure the overlay canvas is properly configured for UI interaction
            EnsureCanvasSetup();

            // Store invitation data
            currentLobbyInvite = lobbyID;
            inviterSteamID = inviterID;

            // Get inviter's name
            string inviterName = "";
            if (inviterID.IsValid() && inviterID.m_SteamID != 0)
            {
                inviterName = SteamFriends.GetFriendPersonaName(inviterID);
                if (string.IsNullOrEmpty(inviterName))
                {
                    inviterName = "Unknown Player";
                    Debug.LogWarning("⚠️ Could not get inviter name from Steam, using fallback");
                }
            }
            else
            {
                inviterName = "Unknown Player";
                Debug.LogWarning("⚠️ Invalid inviter ID, using fallback name");
            }
            
            Debug.Log($"Inviter name: '{inviterName}'");

            // Setup the overlay with the invitation data
            SetupInvitationOverlay(inviterName);

            // Force the overlay to be visible and on top
            currentInvitationOverlay.SetActive(true);
            currentInvitationOverlay.transform.SetAsLastSibling(); // Move to front

            // Ensure the overlay is properly configured for interaction
            EnsureOverlayInteractable();

            Debug.Log($"🎮 Overlay should now be visible! Child count of canvas: {overlayCanvas.transform.childCount}");

            // TODO: Re-enable auto-decline timer once button clickability is fixed
            // StartCoroutine(AutoDeclineAfterDelay(10f));
            Debug.Log("⏰ Auto-decline timer disabled until button clickability is fixed");
        }

        private void SetupInvitationOverlay(string inviterName)
        {
            if (currentInvitationOverlay == null) return;

            // Try to use the prefab component first
            InvitationOverlayPrefab prefabComponent = currentInvitationOverlay.GetComponent<InvitationOverlayPrefab>();
            if (prefabComponent != null)
            {
                // Use the prefab component for cleaner setup
                prefabComponent.SetInviterName(inviterName);
                prefabComponent.SetInvitationMessage("wants you to join their lobby");
                prefabComponent.SetupButtons(AcceptInvitation, DeclineInvitation);
                Debug.Log("✅ Setup invitation overlay using prefab component");
            }
            else
            {
                // Fallback to manual component finding
                Debug.Log("⚠️ No InvitationOverlayPrefab component found, using manual setup");
                SetupInvitationOverlayManual(inviterName);
            }

            Debug.Log("✅ Invitation overlay setup complete");
        }

        private void SetupInvitationOverlayManual(string inviterName)
        {
            // Find UI components in the instantiated prefab
            TextMeshProUGUI[] texts = currentInvitationOverlay.GetComponentsInChildren<TextMeshProUGUI>();
            Button[] buttons = currentInvitationOverlay.GetComponentsInChildren<Button>();

            // Set inviter name (first text component)
            if (texts.Length > 0)
            {
                texts[0].text = inviterName;
                Debug.Log("✅ Set inviter name");
            }

            // Set invitation message (second text component)
            if (texts.Length > 1)
            {
                texts[1].text = "wants you to join their lobby";
                Debug.Log("✅ Set invitation message");
            }

            // Setup Accept button (first button)
            if (buttons.Length > 0)
            {
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(AcceptInvitation);
                Debug.Log("✅ Setup accept button");
            }

            // Setup Decline button (second button)
            if (buttons.Length > 1)
            {
                buttons[1].onClick.RemoveAllListeners();
                buttons[1].onClick.AddListener(DeclineInvitation);
                Debug.Log("✅ Setup decline button");
            }
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
                
                HideInvitationOverlay();
            }
        }

        public void DeclineInvitation()
        {
            Debug.Log("Invitation declined");
            HideInvitationOverlay();
        }

        private void HideInvitationOverlay()
        {
            if (currentInvitationOverlay != null)
            {
                Debug.Log("🗑️ Destroying invitation overlay");
                Destroy(currentInvitationOverlay);
                currentInvitationOverlay = null;
            }

            // Clear invitation data
            currentLobbyInvite = CSteamID.Nil;
            inviterSteamID = CSteamID.Nil;
        }

        private IEnumerator AutoDeclineAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Only auto-decline if the overlay is still showing
            if (currentInvitationOverlay != null)
            {
                Debug.Log("⏰ Auto-declining invitation after timeout");
                DeclineInvitation();
            }
        }

        public bool IsShowingInvitation()
        {
            return currentInvitationOverlay != null;
        }

        [ContextMenu("Test Show Invitation")]
        public void TestShowInvitation()
        {
            Debug.Log("🧪 Testing invitation overlay...");
            
            // Use a fake but more realistic Steam ID
            var fakeLobbyID = new CSteamID(123456789);
            var fakeInviterID = SteamUser.GetSteamID(); // Use our own Steam ID for testing
            
            // Force show the overlay for testing
            TestShowInvitationForced(fakeLobbyID, fakeInviterID);
        }

        [ContextMenu("Manual Close Overlay")]
        public void ManualCloseOverlay()
        {
            Debug.Log("🗑️ MANUAL: Closing invitation overlay...");
            HideInvitationOverlay();
        }

        public void TestShowInvitationForced(CSteamID lobbyID, CSteamID inviterID)
        {
            Debug.Log("🧪 FORCED TEST: Showing invitation overlay...");
            
            // Skip Steam checks for testing
            if (invitationOverlayPrefab == null)
            {
                Debug.LogError("❌ invitationOverlayPrefab is not assigned!");
                return;
            }

            if (overlayCanvas == null)
            {
                Debug.LogError("❌ overlayCanvas is not assigned or found!");
                return;
            }

            // Hide any existing invitation overlay
            HideInvitationOverlay();

            // Instantiate the invitation overlay prefab
            currentInvitationOverlay = Instantiate(invitationOverlayPrefab, overlayCanvas.transform);
            Debug.Log($"✅ TEST: Instantiated invitation overlay prefab. Active: {currentInvitationOverlay.activeSelf}");
            
            // Store invitation data
            currentLobbyInvite = lobbyID;
            inviterSteamID = inviterID;

            // Use test data
            string inviterName = "Test Player";
            Debug.Log($"TEST: Using inviter name: '{inviterName}'");

            // Setup the overlay with test data
            SetupInvitationOverlay(inviterName);

            // Force the overlay to be visible and on top
            currentInvitationOverlay.SetActive(true);
            currentInvitationOverlay.transform.SetAsLastSibling();

            // Make sure all child objects are active
            foreach (Transform child in currentInvitationOverlay.transform)
            {
                child.gameObject.SetActive(true);
            }

            Debug.Log($"🧪 TEST: Overlay should now be visible! Canvas children: {overlayCanvas.transform.childCount}");
            
            // List all canvas children for debugging
            for (int i = 0; i < overlayCanvas.transform.childCount; i++)
            {
                Transform child = overlayCanvas.transform.GetChild(i);
                Debug.Log($"   Canvas child {i}: {child.name} (active: {child.gameObject.activeSelf})");
            }

            // Auto-decline timer disabled for testing button clickability
            // StartCoroutine(AutoDeclineAfterDelay(30f));
            Debug.Log("⏰ TEST: Auto-decline timer disabled for button testing");
        }

        private void EnsureCanvasSetup()
        {
            if (overlayCanvas == null) return;

            // Ensure Canvas is configured properly for UI interaction
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 1000; // Very high value to appear on top
            overlayCanvas.overrideSorting = true;

            // Ensure GraphicRaycaster exists for UI interaction
            GraphicRaycaster raycaster = overlayCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = overlayCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Added GraphicRaycaster to overlay canvas");
            }

            // Make sure it's blocking raycasts
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            raycaster.ignoreReversedGraphics = true;

            Debug.Log($"📋 Canvas setup: SortingOrder={overlayCanvas.sortingOrder}, RenderMode={overlayCanvas.renderMode}");
        }

        private void EnsureOverlayInteractable()
        {
            if (currentInvitationOverlay == null) return;

            // Ensure the overlay has a CanvasGroup for proper interaction
            CanvasGroup canvasGroup = currentInvitationOverlay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentInvitationOverlay.AddComponent<CanvasGroup>();
                Debug.Log("✅ Added CanvasGroup to invitation overlay");
            }

            // Make sure it's interactable
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            // Ensure all buttons are interactable
            Button[] buttons = currentInvitationOverlay.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.interactable = true;
                Debug.Log($"✅ Set button '{button.name}' as interactable");
            }

            // Set the overlay to stretch full screen for proper raycast coverage
            RectTransform rectTransform = currentInvitationOverlay.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                Debug.Log("✅ Set overlay to full screen");
            }

            Debug.Log($"🎛️ Overlay interaction setup complete. Buttons found: {buttons.Length}");
        }

        void OnDestroy()
        {
            if (currentInvitationOverlay != null)
            {
                Destroy(currentInvitationOverlay);
            }

            if (Instance == this)
            {
                Instance = null;
                Debug.Log("DynamicInvitationManager.Instance cleared");
            }
        }
    }
}
