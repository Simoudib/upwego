using UnityEngine;
using UpWeGo; // To access EnhancedPlayerMovement

public class BumperBall : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("The horizontal force applied to the player on impact.")]
    public float pushForce = 15f;
    
    [Tooltip("The vertical force applied to lift the player slightly off the ground.")]
    public float upwardForce = 8f;
    
    [Tooltip("How long the player remains in a 'tossed' physics state.")]
    public float tossDuration = 0.8f;

    // Triggered when a physical collision occurs (requires Rigidbody vs non-Trigger Collider)
    private void OnCollisionEnter(Collision collision)
    {
        TryPushPlayer(collision.gameObject);
    }
    
    // Also support trigger collisions in case the obstacle uses triggers
    private void OnTriggerEnter(Collider other)
    {
        TryPushPlayer(other.gameObject);
    }

    private void TryPushPlayer(GameObject targetObj)
    {
        // Try to get the EnhancedPlayerMovement script from the object we hit
        EnhancedPlayerMovement player = targetObj.GetComponent<EnhancedPlayerMovement>();
        
        if (player != null)
        {
            // Calculate the direction away from the ball
            Vector3 pushDir = targetObj.transform.position - transform.position;
            pushDir.y = 0; // Keep the push horizontal
            
            // If perfectly overlapping, push backwards slightly
            if (pushDir == Vector3.zero) 
            {
                pushDir = -transform.forward;
            }
            
            pushDir.Normalize();

            // Calculate the final force vector
            Vector3 finalForce = (pushDir * pushForce) + (Vector3.up * upwardForce);
            
            // Apply the knockback directly to the player (only the local client will respond)
            player.ApplyKnockback(finalForce, tossDuration);
        }
    }
}
