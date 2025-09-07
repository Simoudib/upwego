using UnityEngine;
using Mirror;

namespace UpWeGo
{
    public class SpawnPointDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        public KeyCode debugSpawnPointsKey = KeyCode.P;
        public KeyCode testSpawnKey = KeyCode.O;

        void Update()
        {
            if (Input.GetKeyDown(debugSpawnPointsKey))
            {
                DebugSpawnPoints();
            }

            if (Input.GetKeyDown(testSpawnKey))
            {
                TestSpawnPosition();
            }
        }

        [ContextMenu("Debug Spawn Points")]
        public void DebugSpawnPoints()
        {
            Debug.Log("🔍 === SPAWN POINT DEBUG ===");
            
            if (NetworkManager.singleton == null)
            {
                Debug.LogError("❌ NetworkManager.singleton is null!");
                return;
            }

            Debug.Log($"📊 Total registered spawn points: {NetworkManager.startPositions.Count}");
            Debug.Log($"🎲 Player spawn method: {NetworkManager.singleton.playerSpawnMethod}");
            Debug.Log($"📍 Current spawn index: {NetworkManager.startPositionIndex}");

            if (NetworkManager.startPositions.Count == 0)
            {
                Debug.LogWarning("⚠️ No spawn points found!");
                Debug.LogWarning("💡 Solutions:");
                Debug.LogWarning("   1. Use SpawnPointManager to generate spawn points");
                Debug.LogWarning("   2. Manually add NetworkStartPosition components to GameObjects");
                Debug.LogWarning("   3. Check if spawn points were created but destroyed");
                return;
            }

            // List all spawn points
            for (int i = 0; i < NetworkManager.startPositions.Count; i++)
            {
                Transform spawn = NetworkManager.startPositions[i];
                if (spawn != null)
                {
                    Debug.Log($"   Spawn Point {i}: '{spawn.name}' at {spawn.position}");
                }
                else
                {
                    Debug.LogWarning($"   Spawn Point {i}: NULL (destroyed?)");
                }
            }

            // Check for NetworkStartPosition components in scene
            NetworkStartPosition[] sceneSpawns = FindObjectsOfType<NetworkStartPosition>();
            Debug.Log($"📋 NetworkStartPosition components in scene: {sceneSpawns.Length}");
            
            foreach (var spawn in sceneSpawns)
            {
                Debug.Log($"   Found: '{spawn.name}' at {spawn.transform.position}");
            }
        }

        [ContextMenu("Test Spawn Position")]
        public void TestSpawnPosition()
        {
            if (NetworkManager.singleton == null)
            {
                Debug.LogError("❌ NetworkManager.singleton is null!");
                return;
            }

            Debug.Log("🧪 === TESTING SPAWN POSITION ===");
            
            for (int i = 0; i < 5; i++)
            {
                Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
                if (spawnPoint != null)
                {
                    Debug.Log($"Test {i}: Would spawn at '{spawnPoint.name}' position {spawnPoint.position}");
                }
                else
                {
                    Debug.LogWarning($"Test {i}: GetStartPosition() returned null!");
                }
            }
        }

        [ContextMenu("Create Test Spawn Points")]
        public void CreateTestSpawnPoints()
        {
            Debug.Log("🛠️ Creating test spawn points...");

            // Clear existing spawn points first
            NetworkStartPosition[] existing = FindObjectsOfType<NetworkStartPosition>();
            for (int i = 0; i < existing.Length; i++)
            {
                DestroyImmediate(existing[i].gameObject);
            }

            // Create a parent object
            GameObject spawnParent = new GameObject("TestSpawnPoints");

            // Create 4 spawn points in a square
            Vector3[] positions = {
                new Vector3(-5, 0, -5),
                new Vector3(5, 0, -5),
                new Vector3(5, 0, 5),
                new Vector3(-5, 0, 5)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject spawnPoint = new GameObject($"TestSpawn_{i}");
                spawnPoint.transform.position = positions[i];
                spawnPoint.transform.SetParent(spawnParent.transform);
                spawnPoint.AddComponent<NetworkStartPosition>();

                // Add visual indicator
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.transform.SetParent(spawnPoint.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(1f, 0.1f, 1f);
                visual.GetComponent<Collider>().enabled = false;
                visual.GetComponent<Renderer>().material.color = Color.red;

                Debug.Log($"✅ Created test spawn point at {positions[i]}");
            }

            Debug.Log($"✅ Created {positions.Length} test spawn points");
        }

        void OnGUI()
        {
            if (!NetworkServer.active) return;

            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
            GUILayout.Label("=== SPAWN DEBUG ===");
            
            if (NetworkManager.singleton != null)
            {
                GUILayout.Label($"Spawn Points: {NetworkManager.startPositions.Count}");
                GUILayout.Label($"Spawn Method: {NetworkManager.singleton.playerSpawnMethod}");
                GUILayout.Label($"Current Index: {NetworkManager.startPositionIndex}");
            }

            if (GUILayout.Button("Debug Spawn Points"))
            {
                DebugSpawnPoints();
            }

            if (GUILayout.Button("Test Spawn Position"))
            {
                TestSpawnPosition();
            }

            if (GUILayout.Button("Create Test Spawns"))
            {
                CreateTestSpawnPoints();
            }

            GUILayout.EndArea();
        }
    }
}

