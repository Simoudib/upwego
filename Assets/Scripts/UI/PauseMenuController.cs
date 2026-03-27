using UnityEngine;
using UnityEngine.SceneManagement;

namespace UpWeGo
{
    /// <summary>
    /// Controls the pause menu in the game scene (accessed via ESC key).
    /// Provides Resume, Respawn, Settings, and Exit functionality.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The main pause menu panel")]
        public GameObject pauseMenuPanel;
        
        [Tooltip("The settings panel")]
        public GameObject settingsPanel;

        [Header("Settings")]
        [Tooltip("Name of the main menu scene to return to")]
        public string mainMenuSceneName = "MainMenu";

        private bool isPaused = false;
        private bool wasLocked = false;

        void Start()
        {
            // Hide pause menu at start
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        void Update()
        {
            // Check for ESC key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If settings is open, close it first
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    // Toggle pause menu
                    if (isPaused)
                    {
                        Resume();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }
        }

        /// <summary>
        /// Pauses the game and shows the pause menu.
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            
            // Remember cursor state
            wasLocked = Cursor.lockState == CursorLockMode.Locked;
            
            // Show pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }

            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause game time
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Resumes the game and hides the pause menu.
        /// Called by Resume button.
        /// </summary>
        public void Resume()
        {
            isPaused = false;

            // Hide pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }

            // Hide settings if it's open
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Restore cursor state
            if (wasLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Resume game time
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Opens the settings panel.
        /// Called by Settings button.
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                // Hide pause menu
                if (pauseMenuPanel != null)
                {
                    pauseMenuPanel.SetActive(false);
                }

                // Show settings
                settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Closes the settings panel and returns to pause menu.
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Show pause menu again
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Exits to main menu.
        /// Called by Exit button.
        /// </summary>
        public void ExitToMainMenu()
        {
            // Resume time before loading new scene
            Time.timeScale = 1f;
            
            // Restore cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Load main menu
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Respawns the local player at a spawn point, simulating a fresh entry to the game scene.
        /// Called by Respawn button.
        /// </summary>
        public void Respawn()
        {
            Debug.Log("🔄 Respawn button clicked!");
            
            // Find the local player
            GameObject localPlayer = FindLocalPlayer();
            
            if (localPlayer == null)
            {
                Debug.LogWarning("❌ Cannot respawn: Local player not found!");
                return;
            }

            Debug.Log($"✅ Found local player: {localPlayer.name}");

            // Get a spawn position
            Transform spawnPoint = GetSpawnPoint();
            
            if (spawnPoint != null)
            {
                Debug.Log($"📍 Spawn point found: {spawnPoint.name} at {spawnPoint.position}");
                
                // Teleport player to spawn point
                localPlayer.transform.position = spawnPoint.position;
                localPlayer.transform.rotation = spawnPoint.rotation;
                
                Debug.Log($"✅ Player respawned at: {spawnPoint.position}");
            }
            else
            {
                // Fallback: spawn at origin with slight random offset
                Vector3 respawnPosition = new Vector3(
                    UnityEngine.Random.Range(-2f, 2f),
                    1f,
                    UnityEngine.Random.Range(-2f, 2f)
                );
                
                localPlayer.transform.position = respawnPosition;
                localPlayer.transform.rotation = Quaternion.identity;
                
                Debug.LogWarning($"⚠️ No spawn points found, respawned at default position: {respawnPosition}");
            }

            // Reset velocity if the player has a Rigidbody
            Rigidbody rb = localPlayer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Debug.Log("🔧 Reset Rigidbody velocity");
            }

            // Reset CharacterController velocity if present
            CharacterController cc = localPlayer.GetComponent<CharacterController>();
            if (cc != null)
            {
                // CharacterController doesn't have a direct velocity reset, 
                // but moving it resets its internal state
                cc.enabled = false;
                cc.enabled = true;
                Debug.Log("🔧 Reset CharacterController");
            }

            Debug.Log("✅ Respawn complete, resuming game...");
            
            // Resume game after respawn
            Resume();
        }

        /// <summary>
        /// Finds the local player GameObject.
        /// </summary>
        private GameObject FindLocalPlayer()
        {
            // Try to find player with NetworkBehaviour
            Mirror.NetworkBehaviour[] networkBehaviours = FindObjectsOfType<Mirror.NetworkBehaviour>();
            foreach (var nb in networkBehaviours)
            {
                if (nb.isLocalPlayer)
                {
                    return nb.gameObject;
                }
            }

            // Fallback: try to find by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.GetComponent<Mirror.NetworkBehaviour>() != null)
            {
                return player;
            }

            return null;
        }

        /// <summary>
        /// Gets a spawn point using Mirror's NetworkManager system.
        /// </summary>
        private Transform GetSpawnPoint()
        {
            // Use Mirror's spawn point system if available
            if (Mirror.NetworkManager.singleton != null)
            {
                Transform spawn = Mirror.NetworkManager.singleton.GetStartPosition();
                if (spawn != null)
                {
                    return spawn;
                }
            }

            // Fallback: find all NetworkStartPosition components
            Mirror.NetworkStartPosition[] spawnPoints = FindObjectsOfType<Mirror.NetworkStartPosition>();
            if (spawnPoints.Length > 0)
            {
                // Return a random spawn point
                int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                return spawnPoints[randomIndex].transform;
            }

            return null;
        }

        void OnDestroy()
        {
            // Ensure time scale is reset
            Time.timeScale = 1f;
        }
    }
}
