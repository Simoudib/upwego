using UnityEngine;

namespace UpWeGo
{
    /// <summary>
    /// Helper script to verify settings system setup in Unity Editor.
    /// Attach this to any GameObject and check the console for setup validation messages.
    /// </summary>
    public class SettingsSystemValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [Tooltip("Run validation on Start")]
        public bool validateOnStart = true;

        void Start()
        {
            if (validateOnStart)
            {
                ValidateSetup();
            }
        }

        [ContextMenu("Validate Settings System")]
        public void ValidateSetup()
        {
            Debug.Log("=== Settings System Validation ===");
            
            // Check for SettingsManager
            if (SettingsManager.Instance != null)
            {
                Debug.Log("✓ SettingsManager found and initialized");
                Debug.Log($"  - Mouse Sensitivity: {SettingsManager.Instance.MouseSensitivity}");
                Debug.Log($"  - Lobby Music Volume: {SettingsManager.Instance.LobbyMusicVolume * 100}%");
                Debug.Log($"  - Game Music Volume: {SettingsManager.Instance.GameMusicVolume * 100}%");
            }
            else
            {
                Debug.LogWarning("✗ SettingsManager not found! Create a GameObject with SettingsManager component.");
            }

            // Check for MusicManager
            MusicManager[] musicManagers = FindObjectsOfType<MusicManager>();
            if (musicManagers.Length > 0)
            {
                Debug.Log($"✓ Found {musicManagers.Length} MusicManager(s):");
                foreach (var manager in musicManagers)
                {
                    string clipName = manager.musicClip != null ? manager.musicClip.name : "NONE";
                    Debug.Log($"  - {manager.musicType} Music: Clip='{clipName}', Scene='{manager.targetSceneName}'");
                    
                    if (manager.musicClip == null)
                    {
                        Debug.LogWarning($"    ⚠ No music clip assigned to {manager.musicType} MusicManager!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("✗ No MusicManager found! Add MusicManager components for lobby and game music.");
            }

            // Check for SettingsPanelController
            SettingsPanelController[] settingsPanels = FindObjectsOfType<SettingsPanelController>(true);
            if (settingsPanels.Length > 0)
            {
                Debug.Log($"✓ Found {settingsPanels.Length} SettingsPanel(s):");
                foreach (var panel in settingsPanels)
                {
                    bool allAssigned = panel.mouseSensitivitySlider != null &&
                                      panel.mouseSensitivityValueText != null &&
                                      panel.lobbyMusicVolumeSlider != null &&
                                      panel.lobbyMusicVolumeValueText != null &&
                                      panel.gameMusicVolumeSlider != null &&
                                      panel.gameMusicVolumeValueText != null;
                    
                    if (allAssigned)
                    {
                        Debug.Log($"  - {panel.gameObject.name}: All UI references assigned ✓");
                    }
                    else
                    {
                        Debug.LogWarning($"  - {panel.gameObject.name}: Missing UI references!");
                        if (panel.mouseSensitivitySlider == null) Debug.LogWarning("    ✗ Mouse Sensitivity Slider");
                        if (panel.mouseSensitivityValueText == null) Debug.LogWarning("    ✗ Mouse Sensitivity Value Text");
                        if (panel.lobbyMusicVolumeSlider == null) Debug.LogWarning("    ✗ Lobby Music Volume Slider");
                        if (panel.lobbyMusicVolumeValueText == null) Debug.LogWarning("    ✗ Lobby Music Volume Value Text");
                        if (panel.gameMusicVolumeSlider == null) Debug.LogWarning("    ✗ Game Music Volume Slider");
                        if (panel.gameMusicVolumeValueText == null) Debug.LogWarning("    ✗ Game Music Volume Value Text");
                    }
                }
            }
            else
            {
                Debug.LogWarning("✗ No SettingsPanel found! Create settings panels in your scenes.");
            }

            // Check for PauseMenuController (game scene only)
            PauseMenuController pauseMenu = FindObjectOfType<PauseMenuController>();
            if (pauseMenu != null)
            {
                Debug.Log("✓ PauseMenuController found");
                if (pauseMenu.pauseMenuPanel != null)
                {
                    Debug.Log("  - Pause Menu Panel assigned ✓");
                }
                else
                {
                    Debug.LogWarning("  - ✗ Pause Menu Panel not assigned!");
                }
                
                if (pauseMenu.settingsPanel != null)
                {
                    Debug.Log("  - Settings Panel assigned ✓");
                }
                else
                {
                    Debug.LogWarning("  - ✗ Settings Panel not assigned!");
                }
            }
            else
            {
                Debug.Log("ℹ PauseMenuController not found (OK if this is MainMenu scene)");
            }

            // Check for SimpleThirdPersonCamera
            SimpleThirdPersonCamera[] cameras = FindObjectsOfType<SimpleThirdPersonCamera>();
            if (cameras.Length > 0)
            {
                Debug.Log($"✓ Found {cameras.Length} SimpleThirdPersonCamera(s) - will use SettingsManager sensitivity");
            }

            Debug.Log("=== Validation Complete ===");
        }
    }
}
