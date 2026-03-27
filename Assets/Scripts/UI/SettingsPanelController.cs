using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    /// <summary>
    /// Controls the Settings Panel UI with sliders for mouse sensitivity and music volumes.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Slider for mouse sensitivity")]
        public Slider mouseSensitivitySlider;
        
        [Tooltip("Text display for mouse sensitivity value")]
        public TextMeshProUGUI mouseSensitivityValueText;
        
        [Tooltip("Slider for lobby music volume")]
        public Slider lobbyMusicVolumeSlider;
        
        [Tooltip("Text display for lobby music volume value")]
        public TextMeshProUGUI lobbyMusicVolumeValueText;
        
        [Tooltip("Slider for game music volume")]
        public Slider gameMusicVolumeSlider;
        
        [Tooltip("Text display for game music volume value")]
        public TextMeshProUGUI gameMusicVolumeValueText;

        [Header("Slider Ranges")]
        public float minSensitivity = 0.5f;
        public float maxSensitivity = 5f;

        private bool wasLocked = false;

        void OnEnable()
        {
            // Remember cursor state
            wasLocked = Cursor.lockState == CursorLockMode.Locked;
            
            // Unlock cursor for settings interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Initialize sliders
            InitializeSliders();
            
            // Load current values
            LoadCurrentSettings();
        }

        void OnDisable()
        {
            // Restore cursor state
            if (wasLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void InitializeSliders()
        {
            // Setup mouse sensitivity slider
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = minSensitivity;
                mouseSensitivitySlider.maxValue = maxSensitivity;
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }

            // Setup lobby music volume slider
            if (lobbyMusicVolumeSlider != null)
            {
                lobbyMusicVolumeSlider.minValue = 0f;
                lobbyMusicVolumeSlider.maxValue = 1f;
                lobbyMusicVolumeSlider.onValueChanged.AddListener(OnLobbyMusicVolumeChanged);
            }

            // Setup game music volume slider
            if (gameMusicVolumeSlider != null)
            {
                gameMusicVolumeSlider.minValue = 0f;
                gameMusicVolumeSlider.maxValue = 1f;
                gameMusicVolumeSlider.onValueChanged.AddListener(OnGameMusicVolumeChanged);
            }
        }

        private void LoadCurrentSettings()
        {
            // Load mouse sensitivity
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = SettingsManager.Instance.MouseSensitivity;
                UpdateSensitivityDisplay(SettingsManager.Instance.MouseSensitivity);
            }

            // Load lobby music volume
            if (lobbyMusicVolumeSlider != null)
            {
                lobbyMusicVolumeSlider.value = SettingsManager.Instance.LobbyMusicVolume;
                UpdateLobbyVolumeDisplay(SettingsManager.Instance.LobbyMusicVolume);
            }

            // Load game music volume
            if (gameMusicVolumeSlider != null)
            {
                gameMusicVolumeSlider.value = SettingsManager.Instance.GameMusicVolume;
                UpdateGameVolumeDisplay(SettingsManager.Instance.GameMusicVolume);
            }
        }

        private void OnMouseSensitivityChanged(float value)
        {
            SettingsManager.Instance.MouseSensitivity = value;
            UpdateSensitivityDisplay(value);
        }

        private void OnLobbyMusicVolumeChanged(float value)
        {
            SettingsManager.Instance.LobbyMusicVolume = value;
            UpdateLobbyVolumeDisplay(value);
        }

        private void OnGameMusicVolumeChanged(float value)
        {
            SettingsManager.Instance.GameMusicVolume = value;
            UpdateGameVolumeDisplay(value);
        }

        private void UpdateSensitivityDisplay(float value)
        {
            if (mouseSensitivityValueText != null)
            {
                mouseSensitivityValueText.text = value.ToString("F2");
            }
        }

        private void UpdateLobbyVolumeDisplay(float value)
        {
            if (lobbyMusicVolumeValueText != null)
            {
                lobbyMusicVolumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
            }
        }

        private void UpdateGameVolumeDisplay(float value)
        {
            if (gameMusicVolumeValueText != null)
            {
                gameMusicVolumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
            }
        }

        /// <summary>
        /// Called by Close/Back button to hide the settings panel.
        /// </summary>
        public void CloseSettings()
        {
            gameObject.SetActive(false);
        }
    }
}
