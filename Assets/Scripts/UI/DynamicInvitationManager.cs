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

            // Hide any existing invitation overlay
            HideInvitationOverlay();

            // Instantiate the invitation overlay prefab
            currentInvitationOverlay = Instantiate(invitationOverlayPrefab, overlayCanvas.transform);
            Debug.Log("✅ Instantiated invitation overlay prefab");

            // Store invitation data
            currentLobbyInvite = lobbyID;
            inviterSteamID = inviterID;

            // Get inviter's name
            string inviterName = SteamFriends.GetFriendPersonaName(inviterID);
            Debug.Log($"Inviter name: {inviterName}");

            // Setup the overlay with the invitation data
            SetupInvitationOverlay(inviterName);

            // Start auto-decline timer
            StartCoroutine(AutoDeclineAfterDelay(10f));
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
            var fakeLobbyID = new CSteamID(123456789);
            var fakeInviterID = new CSteamID(987654321);
            ShowInvitationOverlay(fakeLobbyID, fakeInviterID);
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
