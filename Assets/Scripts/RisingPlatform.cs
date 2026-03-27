using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A platform that rises upward when a player is on top of it.
/// Returns to the starting position when the player leaves.
/// </summary>
public class RisingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How high (in units) the platform rises upward when occupied")]
    public float heightOffset = 3f;
    
    [Tooltip("Speed at which the platform moves")]
    public float speed = 2f;

    [Header("Player Detection")]
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    private Vector3 pointA; // Starting position
    private Vector3 pointB; // Calculated target position (upward)
    private Vector3 targetPosition;
    private bool hasPlayer = false;
    private Dictionary<Transform, Transform> _passengerOriginalParents = new Dictionary<Transform, Transform>();

    void Start()
    {
        // Store the starting position as Point A
        pointA = transform.position;
        
        // Calculate Point B as Point A plus the height offset (upward movement)
        pointB = pointA + Vector3.up * heightOffset;
        
        targetPosition = pointA;
    }

    void Update()
    {
        // Determine target based on player presence
        if (hasPlayer)
        {
            targetPosition = pointB;
        }
        else
        {
            targetPosition = pointA;
        }

        // Move platform smoothly towards target
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Store the original parent and parent to this platform
        if (!_passengerOriginalParents.ContainsKey(other.transform))
        {
            _passengerOriginalParents[other.transform] = other.transform.parent;
            other.transform.SetParent(transform);
            Debug.Log($"RisingPlatform: Added passenger {other.name}");
        }

        // Check if the player entered the platform
        if (other.CompareTag(playerTag))
        {
            hasPlayer = true;
            Debug.Log("Player entered platform - rising up");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Restore the original parent
        if (_passengerOriginalParents.ContainsKey(other.transform))
        {
            other.transform.SetParent(_passengerOriginalParents[other.transform]);
            _passengerOriginalParents.Remove(other.transform);
            Debug.Log($"RisingPlatform: Removed passenger {other.name}");
        }

        // Check if the player left the platform
        if (other.CompareTag(playerTag))
        {
            hasPlayer = false;
            Debug.Log("Player left platform - lowering down");
        }
    }

    void OnDrawGizmos()
    {
        // Visualize the platform's movement path in the editor
        Vector3 start = Application.isPlaying ? pointA : transform.position;
        Vector3 end = Application.isPlaying ? pointB : transform.position + Vector3.up * heightOffset;
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start, 0.3f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(end, 0.3f);
    }
}
