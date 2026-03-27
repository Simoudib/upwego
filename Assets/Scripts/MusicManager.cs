using UnityEngine;
using UnityEngine.SceneManagement;

namespace UpWeGo
{
    /// <summary>
    /// Manages background music playback for specific scenes.
    /// Automatically handles looping and volume control through SettingsManager.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        public enum MusicType
        {
            Lobby,
            Game
        }

        [Header("Music Settings")]
        [Tooltip("Type of music this manager handles")]
        public MusicType musicType = MusicType.Lobby;

        [Tooltip("Audio clip to play (optional for Game music if not yet composed)")]
        public AudioClip musicClip;

        [Tooltip("Scene name where this music should play (e.g., 'MainMenu' or 'GameplayScene')")]
        public string targetSceneName;

        private AudioSource audioSource;
        private bool isInCorrectScene = false;

        void Awake()
        {
            // Create or get AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configure AudioSource
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.clip = musicClip;
        }

        void Start()
        {
            // Subscribe to volume changes
            if (musicType == MusicType.Lobby)
            {
                SettingsManager.Instance.OnLobbyMusicVolumeChanged += UpdateVolume;
                UpdateVolume(SettingsManager.Instance.LobbyMusicVolume);
            }
            else
            {
                SettingsManager.Instance.OnGameMusicVolumeChanged += UpdateVolume;
                UpdateVolume(SettingsManager.Instance.GameMusicVolume);
            }

            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Check if we're in the correct scene
            CheckScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (SettingsManager.Instance != null)
            {
                if (musicType == MusicType.Lobby)
                {
                    SettingsManager.Instance.OnLobbyMusicVolumeChanged -= UpdateVolume;
                }
                else
                {
                    SettingsManager.Instance.OnGameMusicVolumeChanged -= UpdateVolume;
                }
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckScene(scene.name);
        }

        private void CheckScene(string sceneName)
        {
            bool shouldPlay = sceneName == targetSceneName;

            if (shouldPlay && !isInCorrectScene)
            {
                // We just entered the correct scene
                isInCorrectScene = true;
                PlayMusic();
            }
            else if (!shouldPlay && isInCorrectScene)
            {
                // We just left the correct scene
                isInCorrectScene = false;
                StopMusic();
            }
        }

        private void PlayMusic()
        {
            if (musicClip != null && audioSource != null)
            {
                audioSource.Play();
            }
        }

        private void StopMusic()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        private void UpdateVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }
}
