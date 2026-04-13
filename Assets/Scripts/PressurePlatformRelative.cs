using UnityEngine;

public class PressurePlatformRelative : MonoBehaviour
{
    [Header("Platform Movement")]
    [Tooltip("How far down the platform will move when stepped on")]
    public float platformMoveDistance = 2f;
    
    [Tooltip("Speed at which the platform moves down and up")]
    public float platformSpeed = 1f;
    
    [Tooltip("Should the platform return to original position when player leaves?")]
    public bool returnWhenPlayerLeaves = true;

    public enum MoveDirection
    {
        Up, Down, Left, Right, Forward, Back, Custom
    }

    [Header("Triggered Object")]
    [Tooltip("The object that will move when platform is activated")]
    public Transform triggeredObject;
    
    [Tooltip("Direction the triggered object will move")]
    public MoveDirection moveDirection = MoveDirection.Up;
    
    [Tooltip("Custom direction vector (only used if MoveDirection is Custom)")]
    public Vector3 customDirection = Vector3.up;

    [Tooltip("Distance the triggered object will move")]
    public float moveDistance = 5f;
    
    [Tooltip("Speed at which the triggered object moves")]
    public float triggeredObjectSpeed = 3f;
    
    [Tooltip("Should the object return to its original position when platform resets?")]
    public bool returnTriggeredObject = false;

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
    
    private Vector3 _platformStartPosition;
    private Vector3 _platformTargetPosition;
    private bool _isPlatformActivated = false;
    private bool _isPlayerOnPlatform = false;
    
    private bool _isMovingTriggeredObject = false;
    
    private Vector3 _triggeredObjectStartPos;
    private Vector3 _triggeredObjectEndPos;
    private Vector3 _currentTriggeredObjectTarget;
    
    // Debounce variables to prevent jittery trigger exits
    private int _playersOnPlatform = 0;
    private float _exitTimer = 0f;
    private bool _pendingExit = false;

    void Start()
    {
        // Store the platform's starting position
        _platformStartPosition = transform.position;
        _platformTargetPosition = _platformStartPosition - new Vector3(0, platformMoveDistance, 0);
        
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
        
        // Setup triggered object targets
        if (triggeredObject != null)
        {
            _triggeredObjectStartPos = triggeredObject.position;
            
            Vector3 dir = GetDirectionVector();
            _triggeredObjectEndPos = _triggeredObjectStartPos + (dir.normalized * moveDistance);
            
            _currentTriggeredObjectTarget = _triggeredObjectStartPos;
        }
    }

    private Vector3 GetDirectionVector()
    {
        switch (moveDirection)
        {
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
            case MoveDirection.Forward: return Vector3.forward;
            case MoveDirection.Back: return Vector3.back;
            case MoveDirection.Custom: return customDirection;
            default: return Vector3.up;
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
                
                Debug.Log($"PressurePlatformRelative '{gameObject.name}': Platform deactivated (debounced)");
            }
        }
        
        // Move the platform
        MovePlatform();
        
        // Move the triggered object
        if (_isMovingTriggeredObject && triggeredObject != null)
        {
            MoveTriggeredObject();
        }
    }

    void MovePlatform()
    {
        Vector3 targetPosition;
        
        if (_isPlayerOnPlatform)
        {
            // Move platform down
            targetPosition = _platformTargetPosition;
        }
        else if (returnWhenPlayerLeaves)
        {
            // Move platform back up
            targetPosition = _platformStartPosition;
        }
        else
        {
            // Stay at current position if we don't return
            return;
        }
        
        // Smoothly move the platform
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            platformSpeed * Time.deltaTime
        );
    }

    void MoveTriggeredObject()
    {
        // Move the triggered object towards its target
        triggeredObject.position = Vector3.MoveTowards(
            triggeredObject.position,
            _currentTriggeredObjectTarget,
            triggeredObjectSpeed * Time.deltaTime
        );
        
        // Check if we've reached the target
        if (Vector3.Distance(triggeredObject.position, _currentTriggeredObjectTarget) < 0.01f)
        {
            _isMovingTriggeredObject = false;
        }
    }

    void ActivatePlatform()
    {
        if (_isPlatformActivated)
            return;
        
        _isPlatformActivated = true;
        
        // Trigger the object to move to end position
        if (triggeredObject != null)
        {
            _currentTriggeredObjectTarget = _triggeredObjectEndPos;
            _isMovingTriggeredObject = true;
            Debug.Log($"PressurePlatformRelative '{gameObject.name}': Activated! Moving object");
        }
    }

    void DeactivatePlatform()
    {
        if (!_isPlatformActivated)
            return;
        
        _isPlatformActivated = false;
        
        // Optionally return the triggered object to start position
        if (returnTriggeredObject && triggeredObject != null)
        {
            _currentTriggeredObjectTarget = _triggeredObjectStartPos;
            _isMovingTriggeredObject = true;
            Debug.Log($"PressurePlatformRelative '{gameObject.name}': Deactivated! Returning object");
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
                
                Debug.Log($"PressurePlatformRelative '{gameObject.name}': Player stepped on platform (Count: {_playersOnPlatform})");
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
            
            Debug.Log($"PressurePlatformRelative '{gameObject.name}': Player leaving platform (Count: {_playersOnPlatform})");
            
            // Only start the exit process if no players remain
            if (_playersOnPlatform == 0)
            {
                _pendingExit = true;
                _exitTimer = 0f;
            }
        }
    }

    // Visualize the platform movement in the editor
    void OnDrawGizmosSelected()
    {
        // Draw platform start and end positions
        Gizmos.color = Color.green;
        Vector3 startPos = Application.isPlaying ? _platformStartPosition : transform.position;
        Gizmos.DrawWireCube(startPos, transform.localScale);
        
        Gizmos.color = Color.red;
        Vector3 endPos = startPos - new Vector3(0, platformMoveDistance, 0);
        Gizmos.DrawWireCube(endPos, transform.localScale);
        
        // Draw line between them
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startPos, endPos);
        
        // Draw triggered object path
        if (triggeredObject != null)
        {
            Vector3 objStart = Application.isPlaying ? _triggeredObjectStartPos : triggeredObject.position;
            Vector3 dir = GetDirectionVector();
            Vector3 objEnd = objStart + (dir.normalized * moveDistance);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(objStart, 0.2f);
            Gizmos.DrawSphere(objEnd, 0.2f);
            Gizmos.DrawLine(objStart, objEnd);
        }
    }
}
