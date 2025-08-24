using UnityEngine;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Helper component to manage spawn points in your game scene.
    /// Automatically creates NetworkStartPosition components for Mirror networking.
    /// </summary>
    public class SpawnPointManager : MonoBehaviour
    {
        [Header("Spawn Point Generation")]
        [SerializeField] private bool generateSpawnPoints = true;
        [SerializeField] private int numberOfSpawnPoints = 8;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private Vector3 centerPoint = Vector3.zero;
        [SerializeField] private bool avoidObstacles = true;
        [SerializeField] private LayerMask obstacleLayerMask = 1;

        [Header("Manual Spawn Points")]
        [SerializeField] private Transform[] manualSpawnPoints;
        
        [Header("Visualization")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color spawnPointColor = Color.green;

        private GameObject spawnPointsParent;

        void Awake()
        {
            if (generateSpawnPoints)
            {
                CreateSpawnPoints();
            }
            else if (manualSpawnPoints != null && manualSpawnPoints.Length > 0)
            {
                SetupManualSpawnPoints();
            }
        }

        [ContextMenu("Generate Spawn Points")]
        void CreateSpawnPoints()
        {
            // Create parent object for organization
            spawnPointsParent = new GameObject("NetworkSpawnPoints");
            spawnPointsParent.transform.position = centerPoint;

            // Generate spawn points in a circle
            for (int i = 0; i < numberOfSpawnPoints; i++)
            {
                float angle = (float)i / numberOfSpawnPoints * 360f * Mathf.Deg2Rad;
                Vector3 spawnPosition = centerPoint + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    0f,
                    Mathf.Sin(angle) * spawnRadius
                );

                // Check for obstacles if enabled
                if (avoidObstacles)
                {
                    spawnPosition = FindSafeSpawnPosition(spawnPosition);
                }

                CreateSpawnPoint(spawnPosition, i);
            }

            Debug.Log($"✅ Created {numberOfSpawnPoints} spawn points");
        }

        void SetupManualSpawnPoints()
        {
            Debug.Log("🔧 Setting up manual spawn points...");
            
            for (int i = 0; i < manualSpawnPoints.Length; i++)
            {
                if (manualSpawnPoints[i] != null)
                {
                    // Add NetworkStartPosition if it doesn't exist
                    if (manualSpawnPoints[i].GetComponent<NetworkStartPosition>() == null)
                    {
                        manualSpawnPoints[i].gameObject.AddComponent<NetworkStartPosition>();
                        Debug.Log($"✅ Added NetworkStartPosition to {manualSpawnPoints[i].name}");
                    }
                }
            }
        }

        void CreateSpawnPoint(Vector3 position, int index)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{index:00}");
            spawnPoint.transform.position = position;
            spawnPoint.transform.SetParent(spawnPointsParent.transform);

            // Add NetworkStartPosition component - this is what Mirror uses
            spawnPoint.AddComponent<NetworkStartPosition>();

            // Optional: Add a visual indicator
            CreateSpawnPointVisual(spawnPoint);
        }

        void CreateSpawnPointVisual(GameObject spawnPoint)
        {
            // Create a simple visual indicator (cylinder)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(spawnPoint.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1f, 0.1f, 1f);

            // Make it non-collidable
            Collider col = visual.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Color it
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = spawnPointColor;
                renderer.material = mat;
            }
        }

        Vector3 FindSafeSpawnPosition(Vector3 desiredPosition)
        {
            Vector3 safePosition = desiredPosition;
            
            // Check if position is clear
            if (Physics.CheckSphere(desiredPosition, 1f, obstacleLayerMask))
            {
                // Try to find a nearby clear position
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 3f;
                    randomOffset.y = 0f; // Keep on ground level
                    Vector3 testPosition = desiredPosition + randomOffset;
                    
                    if (!Physics.CheckSphere(testPosition, 1f, obstacleLayerMask))
                    {
                        safePosition = testPosition;
                        break;
                    }
                }
            }

            return safePosition;
        }

        [ContextMenu("Clear Spawn Points")]
        void ClearSpawnPoints()
        {
            if (spawnPointsParent != null)
            {
                DestroyImmediate(spawnPointsParent);
                Debug.Log("🗑️ Cleared all generated spawn points");
            }

            // Also clear any NetworkStartPosition components from manual points
            NetworkStartPosition[] existingSpawns = FindObjectsOfType<NetworkStartPosition>();
            for (int i = 0; i < existingSpawns.Length; i++)
            {
                DestroyImmediate(existingSpawns[i]);
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw center point
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(centerPoint, 1f);

            // Draw spawn radius
            Gizmos.color = Color.yellow;
            DrawWireCircle(centerPoint, spawnRadius);

            // Draw generated spawn points preview
            if (generateSpawnPoints)
            {
                Gizmos.color = spawnPointColor;
                for (int i = 0; i < numberOfSpawnPoints; i++)
                {
                    float angle = (float)i / numberOfSpawnPoints * 360f * Mathf.Deg2Rad;
                    Vector3 spawnPosition = centerPoint + new Vector3(
                        Mathf.Cos(angle) * spawnRadius,
                        0f,
                        Mathf.Sin(angle) * spawnRadius
                    );
                    Gizmos.DrawWireSphere(spawnPosition, 0.5f);
                }
            }

            // Draw manual spawn points
            if (manualSpawnPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform spawn in manualSpawnPoints)
                {
                    if (spawn != null)
                    {
                        Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    }
                }
            }
        }

        // Extension method for drawing circles in gizmos
        void DrawWireCircle(Vector3 center, float radius)
        {
            int segments = 32;
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            }
        }
    }
}
