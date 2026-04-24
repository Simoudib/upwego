using UnityEngine;

public class PressurePlatformActivator : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("The disabled object to activate when platform is pressed")]
    public GameObject targetObject;
    
    [Tooltip("Should the object be disabled again when the player leaves?")]
    public bool disableWhenPlayerLeaves = false;

    [Header("Detection Settings")]
    [Tooltip("Tag to detect (usually 'Player')")]
    public string targetTag = "Player";
    
    [Tooltip("Delay before platform resets after player leaves (prevents jitter)")]
    public float exitDebounceTime = 0.5f;

    [Header("Audio Settings")]
    [Tooltip("Audio source for playing sound effects (will auto-add if not assigned)")]
    public AudioSource audioSource;
    
    [Tooltip("Sound effect to play when player steps on the platform")]
    public AudioClip collisionSound;

    private bool _isPlatformActivated = false;
    private bool _isPlayerOnPlatform = false;
    
    // Debounce variables to prevent jittery trigger exits
    private int _playersOnPlatform = 0;
    private float _exitTimer = 0f;
    private bool _pendingExit = false;

    void Start()
    {
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && collisionSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void Update()
    {
        // Handle debounce timer for platform exit
        if (_pendingExit)
        {
            _exitTimer += Time.deltaTime;
            if (_exitTimer >= exitDebounceTime && _playersOnPlatform == 0)
            {
                _isPlayerOnPlatform = false;
                _pendingExit = false;
                
                DeactivatePlatform();
                
                Debug.Log($"PressurePlatformActivator '{gameObject.name}': Platform deactivated (debounced)");
            }
        }
    }

    void ActivatePlatform()
    {
        if (_isPlatformActivated)
            return;
        
        _isPlatformActivated = true;
        
        // Activate the target object
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"PressurePlatformActivator '{gameObject.name}': Activated! Enabled object '{targetObject.name}'");
        }
    }

    void DeactivatePlatform()
    {
        if (!_isPlatformActivated)
            return;
        
        _isPlatformActivated = false;
        
        // Optionally deactivate the target object when leaving
        if (disableWhenPlayerLeaves && targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"PressurePlatformActivator '{gameObject.name}': Deactivated! Disabled object '{targetObject.name}'");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Activator] Trigger entered by: {other.name} (Tag: {other.tag})");
        
        // Check if the colliding object has the correct tag
        if (other.CompareTag(targetTag))
        {
            _playersOnPlatform++;
            _isPlayerOnPlatform = true;
            _pendingExit = false;  // Cancel any pending exit
            _exitTimer = 0f;
            
            // Only activate once
            if (_playersOnPlatform == 1)
            {
                ActivatePlatform();
                
                // Play collision sound
                if (audioSource != null && collisionSound != null)
                {
                    audioSource.PlayOneShot(collisionSound);
                }
                
                Debug.Log($"PressurePlatformActivator '{gameObject.name}': Player stepped on platform (Count: {_playersOnPlatform})");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Activator] Collision entered by: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        if (collision.gameObject.CompareTag(targetTag))
        {
            _playersOnPlatform++;
            _isPlayerOnPlatform = true;
            _pendingExit = false;
            _exitTimer = 0f;
            
            if (_playersOnPlatform == 1)
            {
                ActivatePlatform();
                if (audioSource != null && collisionSound != null)
                {
                    audioSource.PlayOneShot(collisionSound);
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Continuously ensure the player is registered while on platform
        if (other.CompareTag(targetTag))
        {
            _isPlayerOnPlatform = true;
            _pendingExit = false;  // Reset pending exit while player is still here
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player left the platform
        if (other.CompareTag(targetTag))
        {
            _playersOnPlatform--;
            
            // Ensure we don't go negative
            if (_playersOnPlatform < 0)
            {
                _playersOnPlatform = 0;
            }
            
            Debug.Log($"PressurePlatformActivator '{gameObject.name}': Player leaving platform (Count: {_playersOnPlatform})");
            
            // Only start the exit process if no players remain
            if (_playersOnPlatform == 0)
            {
                _pendingExit = true;
                _exitTimer = 0f;
            }
        }
    }
}
