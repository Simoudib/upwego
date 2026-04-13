using UnityEngine;
using Mirror;

namespace UpWeGo
{
    [RequireComponent(typeof(Collider))]
    public class PlayerRespawnZone : MonoBehaviour
    {
        void Start()
        {
            // Make the zone invisible during gameplay if it has a MeshRenderer
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            // Ensure the collider is set up as a trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the object that fell is a player
            EnhancedPlayerMovement player = other.GetComponent<EnhancedPlayerMovement>();
            
            // We ONLY respawn the local player on their own machine to prevent multiplayer glitches
            if (player != null && player.isLocalPlayer)
            {
                RespawnPlayer(player.gameObject);
            }
        }
        
        // Fallback just in case the BoxCollider wasn't set as a Trigger
        private void OnCollisionEnter(Collision collision)
        {
            EnhancedPlayerMovement player = collision.gameObject.GetComponent<EnhancedPlayerMovement>();
            
            if (player != null && player.isLocalPlayer)
            {
                RespawnPlayer(player.gameObject);
            }
        }

        private void RespawnPlayer(GameObject playerObj)
        {
            Debug.Log("🕳️ Player fell out of the map!");
            
            Transform spawnPoint = GetSpawnPoint();
            
            if (spawnPoint != null)
            {
                // Teleport safely to the spawn point
                playerObj.transform.position = spawnPoint.position;
                playerObj.transform.rotation = spawnPoint.rotation;
            }
            else
            {
                // Fallback position if no NetworkStartPosition exists in the scene
                playerObj.transform.position = new Vector3(
                    UnityEngine.Random.Range(-2f, 2f),
                    5f,
                    UnityEngine.Random.Range(-2f, 2f)
                );
            }

            // Reset Rigidbody velocity so they don't carry falling momentum into the respawn
            Rigidbody rb = playerObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Reset CharacterController properly
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                cc.enabled = true;
            }
            
            Debug.Log("✅ Player respawned successfully.");
        }

        private Transform GetSpawnPoint()
        {
            // Ask Mirror Networking for an official spawn point
            if (NetworkManager.singleton != null)
            {
                Transform spawn = NetworkManager.singleton.GetStartPosition();
                if (spawn != null)
                {
                    return spawn;
                }
            }

            // Fallback: manually find all NetworkStartPositions and pick a random one
            NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
            if (spawnPoints.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                return spawnPoints[randomIndex].transform;
            }

            return null;
        }
    }
}
