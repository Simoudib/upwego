using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DespawnZone : MonoBehaviour
{
    void Start()
    {
        // Hide the plane during gameplay if a MeshRenderer is attached, making it invisible
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Make sure the collider is a Trigger so objects smoothly fall into it and are destroyed
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // Called whenever another collider object enters this object's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            // Ignore the player to ensure we only despawn the ball (or other obstacles)
            if (other.GetComponent<UpWeGo.EnhancedPlayerMovement>() != null) return;

            Debug.Log("DespawnZone: Trigger entered by " + other.gameObject.name);
            Destroy(other.gameObject);
        }
    }

    // Fallback just in case the collider didn't successfully become a trigger
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != null)
        {
            // Ignore the player
            if (collision.gameObject.GetComponent<UpWeGo.EnhancedPlayerMovement>() != null) return;

            Debug.Log("DespawnZone: Collision entered by " + collision.gameObject.name);
            Destroy(collision.gameObject);
        }
    }
}
