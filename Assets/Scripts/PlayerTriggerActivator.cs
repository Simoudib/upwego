using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class PlayerTriggerActivator : MonoBehaviour
    {
        [Header("Activation Target")]
        [Tooltip("The GameObject (UI, Platform, etc.) to enable when a player enters the trigger.")]
        public GameObject elementToEnable;
        
        [Header("Settings")]
        [Tooltip("If true, only the player playing on the local computer can trigger this. Useful for local UI elements.")]
        public bool localPlayerOnly = false;
        
        [Tooltip("If true, the element will disable itself again once the player leaves the area.")]
        public bool disableOnExit = false;

        [Tooltip("Should this trigger only be activated once, and then permanently stay on?")]
        public bool triggerOnce = false;

        private bool hasBeenTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // If it's a one-time trigger and we already triggered it, stop here
            if (triggerOnce && hasBeenTriggered) return;

            // Check if the object colliding is a player (using your custom movement script)
            EnhancedPlayerMovement player = other.GetComponent<EnhancedPlayerMovement>();
            if (player == null) player = other.GetComponentInParent<EnhancedPlayerMovement>();
            
            if (player != null)
            {
                // If localPlayerOnly is true, ignore any remote network players that trigger this
                if (localPlayerOnly && !player.isLocalPlayer) return;

                if (elementToEnable != null)
                {
                    elementToEnable.SetActive(true);
                    hasBeenTriggered = true;
                    Debug.Log($"✨ [Activator] Player {player.gameObject.name} activated {elementToEnable.name}");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (disableOnExit && elementToEnable != null)
            {
                // Check if it's a player leaving
                EnhancedPlayerMovement player = other.GetComponent<EnhancedPlayerMovement>();
                if (player == null) player = other.GetComponentInParent<EnhancedPlayerMovement>();
                
                if (player != null)
                {
                    if (localPlayerOnly && !player.isLocalPlayer) return;

                    elementToEnable.SetActive(false);
                    Debug.Log($"💤 [Activator] Player {player.gameObject.name} deactivated {elementToEnable.name}");
                }
            }
        }
    }
}
