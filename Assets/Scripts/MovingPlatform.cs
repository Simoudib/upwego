using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Configuration")]
    public Transform[] waypoints;
    public float speed = 3f;
    public float waitTime = 1f;

    private int _currentIndex = 0;
    private bool _waiting;
    private float _waitTimer;
    private int _direction = 1;

    /// <summary>
    /// The delta this platform moved this frame. Other scripts (like player movement)
    /// can read this to move along with the platform.
    /// </summary>
    public Vector3 MoveDelta { get; private set; }

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("MovingPlatform: Need at least 2 waypoints");
            enabled = false;
            return;
        }
        
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (_waiting)
        {
            MoveDelta = Vector3.zero;
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitTime)
            {
                _waiting = false;
                _waitTimer = 0f;
            }
            return;
        }

        MovePlatform();
    }

    void MovePlatform()
    {
        Vector3 targetPos = waypoints[_currentIndex].position;
        Vector3 previousPos = transform.position;
        Vector3 newPos = Vector3.MoveTowards(previousPos, targetPos, speed * Time.deltaTime);
        
        // Store the delta so passengers can read it
        MoveDelta = newPos - previousPos;
        
        // Move the platform
        transform.position = newPos;

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            _waiting = true;
            
            // Ping-pong between waypoints
            _currentIndex += _direction;
            if (_currentIndex >= waypoints.Length || _currentIndex < 0)
            {
                _direction *= -1;
                _currentIndex += _direction;
            }
        }
    }
}
