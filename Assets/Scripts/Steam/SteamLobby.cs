using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
using Steamworks;

namespace UpWeGo
{
    public class SteamLobby : NetworkBehaviour
    {
        public static SteamLobby Instance;
        public GameObject hostButton = null;
        public ulong lobbyID;
        public NetworkManager networkManager;
        public PanelSwapper panelSwapper;
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyInvite_t> lobbyInvite;
        protected Callback<LobbyEnter_t> lobbyEntered;
        protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

        private const string HostAddressKey = "HostAddress";

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
            networkManager = GetComponent<NetworkManager>();
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initalized. Make sure to run this game in the steam environment");
                return;
            }
            panelSwapper.gameObject.SetActive(true);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyInvite = Callback<LobbyInvite_t>.Create(OnLobbyInvite);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
        }

        void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Failed to create lobby: " + callback.m_eResult);
                return;
            }

            Debug.Log("Lobby successfully created. Lobby ID: " + callback.m_ulSteamIDLobby);
            networkManager.StartHost();

            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
            lobbyID = callback.m_ulSteamIDLobby;
        }

        void OnLobbyInvite(LobbyInvite_t callback)
        {
            Debug.Log("🎉 INVITATION RECEIVED! OnLobbyInvite triggered");
            Debug.Log($"   Lobby ID: {callback.m_ulSteamIDLobby}");
            Debug.Log($"   Inviter ID: {callback.m_ulSteamIDUser}");
            Debug.Log($"   Game ID: {callback.m_ulGameID}");

            CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            CSteamID inviterID = new CSteamID(callback.m_ulSteamIDUser);
            
            // Show invitation overlay immediately when invitation is received
            if (DynamicInvitationManager.Instance != null)
            {
                Debug.Log("📨 Showing invitation overlay from LobbyInvite callback");
                DynamicInvitationManager.Instance.ShowInvitationOverlay(lobbyID, inviterID);
            }
            else
            {
                Debug.LogWarning("❌ DynamicInvitationManager.Instance is null in OnLobbyInvite");
            }
        }

        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("🔗 JOIN REQUEST: OnGameLobbyJoinRequested triggered (user clicked Steam notification)");

            CSteamID lobbyID = callback.m_steamIDLobby;
            CSteamID inviterID = callback.m_steamIDFriend;
            
            Debug.Log($"🔍 Join request details:");
            Debug.Log($"   Lobby ID: {lobbyID}");
            Debug.Log($"   Inviter ID (from callback): {inviterID}");
            Debug.Log($"   Lobby Owner: {SteamMatchmaking.GetLobbyOwner(lobbyID)}");
            
            // Check if we already have an invitation overlay showing
            if (DynamicInvitationManager.Instance != null && DynamicInvitationManager.Instance.IsShowingInvitation())
            {
                Debug.Log("📋 Invitation overlay already showing, not creating duplicate");
                return;
            }
            
            // If inviter ID is invalid, fall back to lobby owner
            if (!inviterID.IsValid() || inviterID.m_SteamID == 0)
            {
                Debug.LogWarning("⚠️ Inviter ID from callback is invalid, using lobby owner");
                inviterID = SteamMatchmaking.GetLobbyOwner(lobbyID);
            }
            
            // Show invitation overlay (fallback if LobbyInvite didn't trigger)
            if (DynamicInvitationManager.Instance != null)
            {
                Debug.Log("📨 Showing invitation overlay from GameLobbyJoinRequested callback (fallback)");
                DynamicInvitationManager.Instance.ShowInvitationOverlay(lobbyID, inviterID);
            }
            else
            {
                Debug.LogWarning("❌ DynamicInvitationManager.Instance is null, auto-joining lobby");
                // Fallback to old behavior if overlay manager is not available
                if (NetworkClient.isConnected || NetworkClient.active)
                {
                    Debug.Log("NetworkClient is active or connected. Disconnecting before joining new lobby");
                    NetworkManager.singleton.StopClient();
                    NetworkClient.Shutdown();
                }
                SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
            }
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Already in a lobby as a host. Ignorning join request");
                return;
            }
            lobbyID = callback.m_ulSteamIDLobby;
            string _hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            networkManager.networkAddress = _hostAddress;
            Debug.Log("Entered lobby: " + callback.m_ulSteamIDLobby);
            networkManager.StartClient();
            panelSwapper.SwapPanel("LobbyPanel");
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby != lobbyID) return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            Debug.Log($"LobbyChatUpdate: {stateChange}");

            bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);

            if (shouldUpdate)
            {
                StartCoroutine(DelayedNameUpdate(0.5f));
                LobbyUIManager.Instance?.CheckAllPlayersReady();
            }
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance == null)
            {
                Debug.LogWarning("Lobby UI Manager.Instance is null, skipping name update");
                yield break;
            }
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI();
        }

        public void LeaveLobby()
        {
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(lobbyID);
            List<CSteamID> members = new List<CSteamID>();

            int count = SteamMatchmaking.GetNumLobbyMembers(lobby);

            for (int i = 0; i < count; i++)
            {
                members.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            }

            if (lobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
                lobbyID = 0;
            }

            if (NetworkServer.active && currentOwner == me)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }

            panelSwapper.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            panelSwapper.SwapPanel("MainPanel");
        }
    }
}