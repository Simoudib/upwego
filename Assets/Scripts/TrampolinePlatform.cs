using UnityEngine;

namespace UpWeGo
{
    public class TrampolinePlatform : MonoBehaviour
    {
        [Header("Trampoline Settings")]
        public float bounceForce = 15f;
        public bool playSound = true;
        public AudioClip bounceSound;
        
        // Anti-bounce spam
        private float lastBounceTime = 0f;
        private float bounceCooldown = 0.1f;

        // Called by EnhancedPlayerMovement's OnControllerColliderHit
        public void BouncePlayer(EnhancedPlayerMovement player)
        {
            if (player != null && Time.time - lastBounceTime > bounceCooldown)
            {
                lastBounceTime = Time.time;
                player.ApplyJumpForce(bounceForce);
                
                if (playSound && bounceSound != null)
                {
                    AudioSource.PlayClipAtPoint(bounceSound, transform.position);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Fallback for if the user decides to use a Trigger collider setup
            EnhancedPlayerMovement player = other.GetComponent<EnhancedPlayerMovement>();
            if (player == null && other.transform.parent != null)
            {
                player = other.GetComponentInParent<EnhancedPlayerMovement>();
            }

            if (player != null)
            {
                BouncePlayer(player);
            }
        }
    }
}
