using UnityEngine;

public class PressurePlatformRotator : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("The object to rotate when platform is pressed")]
    public Transform targetObject;
    
    [Tooltip("The rotation offset to apply (e.g., 0, 90, 0 for a 90-degree turn on Y axis)")]
    public Vector3 rotationOffset = new Vector3(0, 90, 0);
    
    [Tooltip("Speed of the rotation (degrees per second)")]
    public float rotationSpeed = 90f;

    [Tooltip("Should the object rotate back to original rotation when the player leaves?")]
    public bool returnWhenPlayerLeaves = true;

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
    
    private Quaternion _startRotation;
    private Quaternion _endRotation;
    private Quaternion _currentTargetRotation;
    private bool _isRotating = false;

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
        
        if (targetObject != null)
        {
            // Calculate start and end rotations
            _startRotation = targetObject.rotation;
            _endRotation = _startRotation * Quaternion.Euler(rotationOffset);
            _currentTargetRotation = _startRotation;
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
                
                if (returnWhenPlayerLeaves)
                {
                    DeactivatePlatform();
                }
                
                Debug.Log($"PressurePlatformRotator '{gameObject.name}': Platform deactivated (debounced)");
            }
        }
        
        // Handle smooth rotation
        if (_isRotating && targetObject != null)
        {
            targetObject.rotation = Quaternion.RotateTowards(
                targetObject.rotation, 
                _currentTargetRotation, 
                rotationSpeed * Time.deltaTime
            );
            
            // Check if we reached the target rotation
            if (Quaternion.Angle(targetObject.rotation, _currentTargetRotation) < 0.1f)
            {
                targetObject.rotation = _currentTargetRotation; // Snap to exact
                _isRotating = false;
            }
        }
    }

    void ActivatePlatform()
    {
        if (_isPlatformActivated)
            return;
        
        _isPlatformActivated = true;
        
        if (targetObject != null)
        {
            _currentTargetRotation = _endRotation;
            _isRotating = true;
            Debug.Log($"PressurePlatformRotator '{gameObject.name}': Activated! Rotating object.");
        }
    }

    void DeactivatePlatform()
    {
        if (!_isPlatformActivated)
            return;
        
        _isPlatformActivated = false;
        
        if (returnWhenPlayerLeaves && targetObject != null)
        {
            _currentTargetRotation = _startRotation;
            _isRotating = true;
            Debug.Log($"PressurePlatformRotator '{gameObject.name}': Deactivated! Rotating back.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
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
                
                Debug.Log($"PressurePlatformRotator '{gameObject.name}': Player stepped on platform (Count: {_playersOnPlatform})");
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
            
            Debug.Log($"PressurePlatformRotator '{gameObject.name}': Player leaving platform (Count: {_playersOnPlatform})");
            
            // Only start the exit process if no players remain
            if (_playersOnPlatform == 0)
            {
                _pendingExit = true;
                _exitTimer = 0f;
            }
        }
    }
}
