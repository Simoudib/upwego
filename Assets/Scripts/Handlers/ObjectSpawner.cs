using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("The prefab to spawn (e.g., the ball with physics).")]
    public GameObject prefabToSpawn;
    
    [Tooltip("Time in seconds between each spawn.")]
    public float spawnInterval = 2f;

    private float timer = 0f;

    void Start()
    {
        // Hide the spawner object during gameplay if a MeshRenderer is attached
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        // Disable Collider if present so it doesn't interfere with the spawned prefab
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    private void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            // Spawn the prefab at this object's exact position and rotation
            Instantiate(prefabToSpawn, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"Prefab to spawn is missing on ObjectSpawner: {gameObject.name}");
        }
    }
}
