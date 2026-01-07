using UnityEngine;
using System.Collections.Generic;

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
    
    private List<Transform> _passengers = new List<Transform>();
    private Vector3 _lastPosition;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("MovingPlatform: Need at least 2 waypoints");
            enabled = false;
            return;
        }
        
        transform.position = waypoints[0].position;
        _lastPosition = transform.position;
    }

    void Update()
    {
        if (_waiting)
        {
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
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        
        // Calculate how much we moved
        Vector3 moveDelta = newPos - transform.position;
        
        // Move the platform
        transform.position = newPos;
        
        // Move all passengers by the same amount (like carry system does)
        foreach (Transform passenger in _passengers)
        {
            if (passenger != null)
            {
                passenger.position += moveDelta;
            }
        }

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

    void OnTriggerEnter(Collider other)
    {
        if (!_passengers.Contains(other.transform))
        {
            _passengers.Add(other.transform);
            Debug.Log($"Platform: Added passenger {other.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_passengers.Contains(other.transform))
        {
            _passengers.Remove(other.transform);
            Debug.Log($"Platform: Removed passenger {other.name}");
        }
    }
}
