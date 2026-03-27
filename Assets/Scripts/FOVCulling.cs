using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Field of View (FOV) Culling System
/// Attach this script to the player character to optimize performance by only rendering
/// objects within a configurable radius sphere around the player.
/// 
/// Objects on the specified layer will have their renderers enabled/disabled based on distance.
/// No additional components needed on cullable objects - just assign them to the correct layer!
/// </summary>
public class FOVCulling : MonoBehaviour
{
    [Header("Culling Settings")]
    [Tooltip("Radius of the sphere around the player. Objects outside this radius will be culled.")]
    [SerializeField] private float cullingRadius = 50f;
    
    [Tooltip("How often to check for objects to cull (in seconds). Lower = more responsive, higher = better performance.")]
    [SerializeField] private float updateInterval = 0.5f;
    
    [Header("Layer/Tag Filtering")]
    [Tooltip("Layer mask for objects that should be culled. Set to 'Default' to affect all default objects.")]
    [SerializeField] private LayerMask cullableLayerMask = 1; // Default layer
    
    [Tooltip("Use tags instead of layers for detection (optional).")]
    [SerializeField] private bool useTagInstead = false;
    
    [Tooltip("Tag to identify cullable objects (if using tags instead of layers).")]
    [SerializeField] private string cullableTag = "Cullable";
    
    [Header("Debug")]
    [Tooltip("Show a wire sphere in the Scene view to visualize the culling radius.")]
    [SerializeField] private bool showDebugSphere = true;
    
    [Tooltip("Enable debug logging for culling operations.")]
    [SerializeField] private bool debugLogging = false;
    
    // Internal tracking
    private float _updateTimer = 0f;
    private Dictionary<GameObject, List<Renderer>> _activeObjects = new Dictionary<GameObject, List<Renderer>>();
    private Dictionary<GameObject, List<Renderer>> _culledObjects = new Dictionary<GameObject, List<Renderer>>();
    
    // Terrain tracking (terrains are handled differently than regular renderers)
    private Dictionary<Terrain, bool> _activeTerrains = new Dictionary<Terrain, bool>();
    private Dictionary<Terrain, bool> _culledTerrains = new Dictionary<Terrain, bool>();
    
    void Start()
    {
        if (debugLogging)
        {
            Debug.Log($"FOVCulling initialized with radius: {cullingRadius}, update interval: {updateInterval}s");
            Debug.Log($"Using {(useTagInstead ? "Tag: " + cullableTag : "Layer Mask: " + cullableLayerMask.value)}");
        }
        
        // Find all cullable objects in the scene at start
        InitializeAllCullableObjects();
        
        // Find all terrains in the scene
        InitializeTerrains();
        
        // Perform initial culling check
        UpdateCulling();
    }
    
    void Update()
    {
        _updateTimer += Time.deltaTime;
        
        if (_updateTimer >= updateInterval)
        {
            _updateTimer = 0f;
            UpdateCulling();
        }
    }
    
    /// <summary>
    /// Finds all cullable objects in the scene and adds them to tracking
    /// </summary>
    private void InitializeAllCullableObjects()
    {
        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        int objectCount = 0;
        foreach (Renderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            
            // Check if object is on the correct layer
            bool isOnCorrectLayer = ((1 << obj.layer) & cullableLayerMask) != 0;
            
            // Check tag if using tag mode
            bool hasCorrectTag = !useTagInstead || obj.CompareTag(cullableTag);
            
            if (isOnCorrectLayer && hasCorrectTag)
            {
                // Skip if already tracked
                if (_activeObjects.ContainsKey(obj) || _culledObjects.ContainsKey(obj))
                    continue;
                
                // Get all renderers for this object
                List<Renderer> renderers = GetAllRenderers(obj);
                if (renderers.Count > 0)
                {
                    // Start with all objects as active (visible)
                    _activeObjects[obj] = renderers;
                    objectCount++;
                }
            }
        }
        
        if (debugLogging)
        {
            Debug.Log($"FOVCulling: Found {objectCount} cullable objects in scene");
        }
    }
    
    /// <summary>
    /// Finds all terrains in the scene and adds them to tracking
    /// </summary>
    private void InitializeTerrains()
    {
        Terrain[] allTerrains = FindObjectsOfType<Terrain>();
        
        foreach (Terrain terrain in allTerrains)
        {
            if (!_activeTerrains.ContainsKey(terrain) && !_culledTerrains.ContainsKey(terrain))
            {
                _activeTerrains[terrain] = true;
            }
        }
        
        if (debugLogging && allTerrains.Length > 0)
        {
            Debug.Log($"FOVCulling: Found {allTerrains.Length} terrain(s) in scene");
        }
    }
    
    /// <summary>
    /// Main culling update - checks all tracked objects and culls based on distance
    /// </summary>
    private void UpdateCulling()
    {
        Vector3 playerPosition = transform.position;
        
        // Check all active objects - cull those outside radius
        List<GameObject> objectsToCull = new List<GameObject>();
        foreach (var kvp in _activeObjects)
        {
            GameObject obj = kvp.Key;
            if (obj == null) continue;
            
            float distance = Vector3.Distance(playerPosition, obj.transform.position);
            if (distance > cullingRadius)
            {
                objectsToCull.Add(obj);
            }
        }
        
        foreach (GameObject obj in objectsToCull)
        {
            DisableObject(obj);
        }
        
        // Check all culled objects - restore those within radius
        List<GameObject> objectsToRestore = new List<GameObject>();
        foreach (var kvp in _culledObjects)
        {
            GameObject obj = kvp.Key;
            if (obj == null) continue;
            
            float distance = Vector3.Distance(playerPosition, obj.transform.position);
            if (distance <= cullingRadius)
            {
                objectsToRestore.Add(obj);
            }
        }
        
        foreach (GameObject obj in objectsToRestore)
        {
            EnableObject(obj);
        }
        
        // Check terrains - cull those outside radius
        List<Terrain> terrainsToCull = new List<Terrain>();
        foreach (var kvp in _activeTerrains)
        {
            Terrain terrain = kvp.Key;
            if (terrain == null) continue;
            
            // Calculate distance to closest point on terrain (not just the pivot)
            float distance = GetTerrainDistance(terrain, playerPosition);
            if (distance > cullingRadius)
            {
                terrainsToCull.Add(terrain);
            }
        }
        
        foreach (Terrain terrain in terrainsToCull)
        {
            DisableTerrain(terrain);
        }
        
        // Check culled terrains - restore those within radius
        List<Terrain> terrainsToRestore = new List<Terrain>();
        foreach (var kvp in _culledTerrains)
        {
        Terrain terrain = kvp.Key;
            if (terrain == null) continue;
            
            // Calculate distance to closest point on terrain (not just the pivot)
            float distance = GetTerrainDistance(terrain, playerPosition);
            if (distance <= cullingRadius)
            {
                terrainsToRestore.Add(terrain);
            }
        }
        
        foreach (Terrain terrain in terrainsToRestore)
        {
            EnableTerrain(terrain);
        }
    }
    
    /// <summary>
    /// Disables all renderers on the given object
    /// </summary>
    private void DisableObject(GameObject obj)
    {
        if (_activeObjects.TryGetValue(obj, out List<Renderer> renderers))
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
            
            // Move from active to culled tracking
            _culledObjects[obj] = renderers;
            _activeObjects.Remove(obj);
            
            if (debugLogging)
            {
                Debug.Log($"Culled object: {obj.name} (distance: {Vector3.Distance(transform.position, obj.transform.position):F1})");
            }
        }
    }
    
    /// <summary>
    /// Enables all renderers on the given object
    /// </summary>
    private void EnableObject(GameObject obj)
    {
        if (_culledObjects.TryGetValue(obj, out List<Renderer> renderers))
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
            
            // Move from culled to active tracking
            _activeObjects[obj] = renderers;
            _culledObjects.Remove(obj);
            
            if (debugLogging)
            {
                Debug.Log($"Restored object: {obj.name} (distance: {Vector3.Distance(transform.position, obj.transform.position):F1})");
            }
        }
    }
    
    /// <summary>
    /// Gets all renderers on an object and its children
    /// </summary>
    private List<Renderer> GetAllRenderers(GameObject obj)
    {
        List<Renderer> renderers = new List<Renderer>();
        
        // Get renderer on the object itself
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null)
        {
            renderers.Add(objRenderer);
        }
        
        // Get all child renderers
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != null && !renderers.Contains(renderer))
            {
                renderers.Add(renderer);
            }
        }
        
        return renderers;
    }
    
    /// <summary>
    /// Calculate distance from player to the closest point on the terrain
    /// This prevents large terrains from being culled when the player is standing on them
    /// </summary>
    private float GetTerrainDistance(Terrain terrain, Vector3 playerPosition)
    {
        // Get terrain bounds
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            // Use the collider bounds to get the closest point
            Vector3 closestPoint = terrainCollider.bounds.ClosestPoint(playerPosition);
            return Vector3.Distance(playerPosition, closestPoint);
        }
        
        // Fallback: use terrain data to calculate bounds
        TerrainData terrainData = terrain.terrainData;
        if (terrainData != null)
        {
            Vector3 terrainPosition = terrain.transform.position;
            Vector3 terrainSize = terrainData.size;
            
            // Create bounds from terrain position and size
            Bounds bounds = new Bounds(
                terrainPosition + terrainSize * 0.5f,
                terrainSize
            );
            
            Vector3 closestPoint = bounds.ClosestPoint(playerPosition);
            return Vector3.Distance(playerPosition, closestPoint);
        }
        
        // Last resort fallback: use transform position
        return Vector3.Distance(playerPosition, terrain.transform.position);
    }
    
    /// <summary>
    /// Disables a terrain
    /// </summary>
    private void DisableTerrain(Terrain terrain)
    {
        if (_activeTerrains.ContainsKey(terrain))
        {
            terrain.enabled = false;
            
            // Move from active to culled tracking
            _culledTerrains[terrain] = true;
            _activeTerrains.Remove(terrain);
            
            if (debugLogging)
            {
                Debug.Log($"Culled terrain: {terrain.name} (distance: {Vector3.Distance(transform.position, terrain.transform.position):F1})");
            }
        }
    }
    
    /// <summary>
    /// Enables a terrain
    /// </summary>
    private void EnableTerrain(Terrain terrain)
    {
        if (_culledTerrains.ContainsKey(terrain))
        {
            terrain.enabled = true;
            
            // Move from culled to active tracking
            _activeTerrains[terrain] = true;
            _culledTerrains.Remove(terrain);
            
            if (debugLogging)
            {
                Debug.Log($"Restored terrain: {terrain.name} (distance: {Vector3.Distance(transform.position, terrain.transform.position):F1})");
            }
        }
    }
    
    /// <summary>
    /// Draws the culling sphere in the Scene view for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showDebugSphere)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, cullingRadius);
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Gizmos.DrawSphere(transform.position, cullingRadius);
        }
    }
    
    /// <summary>
    /// Restore all culled objects when the script is disabled
    /// </summary>
    private void OnDisable()
    {
        // Restore all culled objects
        List<GameObject> objectsToRestore = new List<GameObject>(_culledObjects.Keys);
        foreach (GameObject obj in objectsToRestore)
        {
            EnableObject(obj);
        }
        
        // Restore all culled terrains
        List<Terrain> terrainsToRestore = new List<Terrain>(_culledTerrains.Keys);
        foreach (Terrain terrain in terrainsToRestore)
        {
            EnableTerrain(terrain);
        }
        
        if (debugLogging)
        {
            Debug.Log("FOVCulling disabled - restored all culled objects and terrains");
        }
    }
}
