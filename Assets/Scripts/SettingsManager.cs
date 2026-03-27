using UnityEngine;

namespace UpWeGo
{
    /// <summary>
    /// Singleton manager for game settings that persists across scenes.
    /// Handles mouse sensitivity and music volume settings with PlayerPrefs persistence.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;
        public static SettingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    instance = go.AddComponent<SettingsManager>();
                }
                return instance;
            }
        }

        // PlayerPrefs keys
        private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
        private const string LOBBY_VOLUME_KEY = "LobbyMusicVolume";
        private const string GAME_VOLUME_KEY = "GameMusicVolume";

        // Default values
        private const float DEFAULT_MOUSE_SENSITIVITY = 2f;
        private const float DEFAULT_MUSIC_VOLUME = 0.7f;

        // Current settings
        private float mouseSensitivity;
        private float lobbyMusicVolume;
        private float gameMusicVolume;

        // Events for real-time updates
        public System.Action<float> OnMouseSensitivityChanged;
        public System.Action<float> OnLobbyMusicVolumeChanged;
        public System.Action<float> OnGameMusicVolumeChanged;

        // Public properties
        public float MouseSensitivity
        {
            get => mouseSensitivity;
            set
            {
                mouseSensitivity = value;
                PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, value);
                PlayerPrefs.Save();
                OnMouseSensitivityChanged?.Invoke(value);
            }
        }

        public float LobbyMusicVolume
        {
            get => lobbyMusicVolume;
            set
            {
                lobbyMusicVolume = value;
                PlayerPrefs.SetFloat(LOBBY_VOLUME_KEY, value);
                PlayerPrefs.Save();
                OnLobbyMusicVolumeChanged?.Invoke(value);
            }
        }

        public float GameMusicVolume
        {
            get => gameMusicVolume;
            set
            {
                gameMusicVolume = value;
                PlayerPrefs.SetFloat(GAME_VOLUME_KEY, value);
                PlayerPrefs.Save();
                OnGameMusicVolumeChanged?.Invoke(value);
            }
        }

        void Awake()
        {
            // Singleton pattern
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Hide from hierarchy to avoid cleanup warnings
                gameObject.hideFlags = HideFlags.DontSave;
                
                LoadSettings();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads settings from PlayerPrefs or sets defaults if not found.
        /// </summary>
        private void LoadSettings()
        {
            mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DEFAULT_MOUSE_SENSITIVITY);
            lobbyMusicVolume = PlayerPrefs.GetFloat(LOBBY_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
            gameMusicVolume = PlayerPrefs.GetFloat(GAME_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
        }

        /// <summary>
        /// Resets all settings to default values.
        /// </summary>
        public void ResetToDefaults()
        {
            MouseSensitivity = DEFAULT_MOUSE_SENSITIVITY;
            LobbyMusicVolume = DEFAULT_MUSIC_VOLUME;
            GameMusicVolume = DEFAULT_MUSIC_VOLUME;
        }
    }
}
