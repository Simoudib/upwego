using UnityEngine;

public class PressurePlatform : MonoBehaviour
{
    [Header("Platform Movement")]
    [Tooltip("How far down the platform will move when stepped on")]
    public float platformMoveDistance = 0.5f;
    
    [Tooltip("Speed at which the platform moves down and up")]
    public float platformSpeed = 2f;
    
    [Tooltip("Should the platform return to original position when player leaves?")]
    public bool returnWhenPlayerLeaves = true;

    [Header("Triggered Object")]
    [Tooltip("The object that will move from Point A to Point B when platform is activated")]
    public Transform triggeredObject;
    
    [Tooltip("Starting position for the triggered object")]
    public Transform pointA;
    
    [Tooltip("Ending position for the triggered object")]
    public Transform pointB;
    
    [Tooltip("Speed at which the triggered object moves")]
    public float triggeredObjectSpeed = 3f;
    
    [Tooltip("Should the object return to Point A when platform resets?")]
    public bool returnTriggeredObject = false;

    [Header("Detection Settings")]
    [Tooltip("Tag to detect (usually 'Player')")]
    public string targetTag = "Player";
    
    [Tooltip("Delay before platform resets after player leaves (prevents jitter)")]
    public float exitDebounceTime = 0.2f;
    
    private Vector3 _platformStartPosition;
    private Vector3 _platformTargetPosition;
    private bool _isPlatformActivated = false;
    private bool _isPlayerOnPlatform = false;
    
    private bool _isMovingTriggeredObject = false;
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
        
        // Validate triggered object setup
        if (triggeredObject != null)
        {
            if (pointA != null && pointB != null)
            {
                // Optionally set the triggered object to Point A at start
                triggeredObject.position = pointA.position;
                _currentTriggeredObjectTarget = pointA.position;
            }
            else
            {
                Debug.LogWarning($"PressurePlatform '{gameObject.name}': Point A or Point B not assigned!");
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
                
                if (returnWhenPlayerLeaves)
                {
                    DeactivatePlatform();
                }
                
                Debug.Log($"PressurePlatform '{gameObject.name}': Platform deactivated (debounced)");
            }
        }
        
        // Move the platform
        MovePlatform();
        
        // Move the triggered object
        MoveTriggeredObject();
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
        if (triggeredObject == null || !_isMovingTriggeredObject)
            return;
        
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
        
        // Trigger the object to move from Point A to Point B
        if (triggeredObject != null && pointB != null)
        {
            _currentTriggeredObjectTarget = pointB.position;
            _isMovingTriggeredObject = true;
            Debug.Log($"PressurePlatform '{gameObject.name}': Activated! Moving object to Point B");
        }
    }

    void DeactivatePlatform()
    {
        if (!_isPlatformActivated)
            return;
        
        _isPlatformActivated = false;
        
        // Optionally return the triggered object to Point A
        if (returnTriggeredObject && triggeredObject != null && pointA != null)
        {
            _currentTriggeredObjectTarget = pointA.position;
            _isMovingTriggeredObject = true;
            Debug.Log($"PressurePlatform '{gameObject.name}': Deactivated! Returning object to Point A");
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
                Debug.Log($"PressurePlatform '{gameObject.name}': Player stepped on platform (Count: {_playersOnPlatform})");
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
            
            Debug.Log($"PressurePlatform '{gameObject.name}': Player leaving platform (Count: {_playersOnPlatform})");
            
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
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
