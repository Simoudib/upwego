using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Displays player names above characters with billboard behavior (always faces camera)
    /// </summary>
    public class PlayerNameDisplay : NetworkBehaviour
    {
        [Header("Name Display Settings")]
        [SerializeField] private Canvas nameCanvas;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private float heightOffset = 2.5f; // How high above the player to show the name
        [SerializeField] private float maxDistance = 50f; // Maximum distance to show names
        [SerializeField] private bool hideOwnName = true; // Hide name for local player
        
        [Header("Visual Settings")]
        [SerializeField] private Color defaultNameColor = Color.white;
        [SerializeField] private Color localPlayerColor = Color.cyan;
        [SerializeField] private Color carriedPlayerColor = Color.yellow;
        [SerializeField] private float fadeDistance = 30f; // Distance at which names start to fade
        [SerializeField] private AnimationCurve distanceFadeCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private float fontSize = 24f; // Font size for the name text
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Font fallbackFont;
        
        // Network synchronized player name
        [SyncVar(hook = nameof(OnPlayerNameChanged))]
        private string playerName = "";
        
        // References
        private Camera playerCamera;
        private Transform playerTransform;
        private EnhancedPlayerMovement playerMovement;
        private CanvasGroup canvasGroup;
        
        // State tracking
        private bool isSetup = false;
        private float lastDistanceCheck = 0f;
        private const float DISTANCE_CHECK_INTERVAL = 0.1f; // Check distance 10 times per second
        
        void Start()
        {
            playerTransform = transform;
            playerMovement = GetComponent<EnhancedPlayerMovement>();
            
            // Setup UI if needed
            if (autoCreateUI && nameCanvas == null)
            {
                SetupNameDisplayUI();
            }
            
            // Find the main camera
            FindPlayerCamera();
            
            // Set default name if none provided
            if (string.IsNullOrEmpty(playerName))
            {
                if (isServer)
                {
                    // Generate a default name based on connection ID or netId
                    string defaultName = $"Player {netId}";
                    if (connectionToClient != null)
                    {
                        defaultName = $"Player {connectionToClient.connectionId}";
                    }
                    playerName = defaultName;
                }
            }
            
            isSetup = true;
            UpdateNameDisplay();
        }
        
        void Update()
        {
            if (!isSetup || nameCanvas == null) return;
            
            // Update billboard rotation to face camera
            UpdateBillboardRotation();
            
            // Update distance-based visibility and fading
            if (Time.time - lastDistanceCheck > DISTANCE_CHECK_INTERVAL)
            {
                UpdateDistanceVisibility();
                lastDistanceCheck = Time.time;
            }
        }
        
        /// <summary>
        /// Sets the player's display name (only works on server)
        /// </summary>
        public void SetPlayerName(string newName)
        {
            if (!isServer) return;
            
            playerName = newName;
        }
        
        /// <summary>
        /// Gets the current player name
        /// </summary>
        public string GetPlayerName()
        {
            return playerName;
        }
        
        /// <summary>
        /// Called when the player name changes (SyncVar hook)
        /// </summary>
        void OnPlayerNameChanged(string oldName, string newName)
        {
            Debug.Log($"🏷️ Player name changed: {oldName} → {newName} (NetId: {netId})");
            UpdateNameDisplay();
        }
        
        /// <summary>
        /// Updates the name display text and color
        /// </summary>
        void UpdateNameDisplay()
        {
            if (nameText == null) return;
            
            nameText.text = playerName;
            
            // Set color based on player state
            Color nameColor = defaultNameColor;
            
            if (isLocalPlayer)
            {
                nameColor = localPlayerColor;
                // Hide own name if setting is enabled
                if (hideOwnName && nameCanvas != null)
                {
                    nameCanvas.gameObject.SetActive(false);
                    return;
                }
            }
            else if (playerMovement != null && playerMovement.IsBeingCarried)
            {
                nameColor = carriedPlayerColor;
            }
            
            nameText.color = nameColor;
            
            // Make sure canvas is active
            if (nameCanvas != null)
            {
                nameCanvas.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Updates the billboard rotation to always face the camera
        /// </summary>
        void UpdateBillboardRotation()
        {
            if (playerCamera == null)
            {
                FindPlayerCamera();
                return;
            }
            
            if (nameCanvas != null)
            {
                // Make the canvas face the camera (billboard effect for all players)
                Vector3 directionToCamera = playerCamera.transform.position - nameCanvas.transform.position;
                directionToCamera.y = 0; // Keep it horizontal
                
                if (directionToCamera != Vector3.zero)
                {
                    nameCanvas.transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
        }
        
        /// <summary>
        /// Updates visibility and fading based on distance from camera
        /// </summary>
        void UpdateDistanceVisibility()
        {
            if (playerCamera == null || nameCanvas == null) return;
            
            float distance = Vector3.Distance(playerCamera.transform.position, playerTransform.position);
            
            // Hide if too far away
            if (distance > maxDistance)
            {
                nameCanvas.gameObject.SetActive(false);
                return;
            }
            
            // Show if within range
            if (!nameCanvas.gameObject.activeSelf && (!isLocalPlayer || !hideOwnName))
            {
                nameCanvas.gameObject.SetActive(true);
            }
            
            // Apply distance-based fading
            if (canvasGroup != null && distance > fadeDistance)
            {
                float fadeRatio = (distance - fadeDistance) / (maxDistance - fadeDistance);
                float alpha = distanceFadeCurve.Evaluate(1f - fadeRatio);
                canvasGroup.alpha = alpha;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        /// <summary>
        /// Finds the main camera or active camera
        /// </summary>
        void FindPlayerCamera()
        {
            // Try to find the main camera
            if (Camera.main != null)
            {
                playerCamera = Camera.main;
                return;
            }
            
            // Fallback to any active camera
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.isActiveAndEnabled)
                {
                    playerCamera = cam;
                    break;
                }
            }
            
            if (playerCamera == null)
            {
                Debug.LogWarning($"⚠️ PlayerNameDisplay: No active camera found for {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Automatically creates the name display UI if not already present
        /// </summary>
        void SetupNameDisplayUI()
        {
            // Create canvas GameObject
            GameObject canvasObj = new GameObject("NameDisplayCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = new Vector3(0, heightOffset, 0);
            
            // Setup Canvas component
            nameCanvas = canvasObj.AddComponent<Canvas>();
            nameCanvas.renderMode = RenderMode.WorldSpace;
            nameCanvas.worldCamera = playerCamera;
            
            // Setup Canvas Scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 0.01f; // Adjust size
            
            // Add CanvasGroup for fading
            canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            
            // Setup RectTransform
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            
            // Create text GameObject
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(canvasObj.transform);
            
            // Setup TextMeshPro component
            nameText = textObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Player Name";
            nameText.fontSize = fontSize; // Use configurable font size
            nameText.color = defaultNameColor;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            // Add outline for better readability
            nameText.fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
            if (nameText.fontMaterial == null)
            {
                // Fallback: create a simple outline effect
                nameText.enableVertexGradient = false;
                nameText.outlineWidth = 0.2f;
                nameText.outlineColor = Color.black;
            }
            
            // Setup text RectTransform
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log($"✅ Created name display UI for {gameObject.name}");
        }
        
        /// <summary>
        /// Updates the height offset of the name display
        /// </summary>
        public void SetHeightOffset(float newOffset)
        {
            heightOffset = newOffset;
            if (nameCanvas != null)
            {
                nameCanvas.transform.localPosition = new Vector3(0, heightOffset, 0);
            }
        }
        
        /// <summary>
        /// Sets the maximum display distance
        /// </summary>
        public void SetMaxDistance(float distance)
        {
            maxDistance = distance;
        }
        
        /// <summary>
        /// Sets the font size for the name text
        /// </summary>
        public void SetFontSize(float size)
        {
            fontSize = size;
            if (nameText != null)
            {
                nameText.fontSize = fontSize;
            }
        }
        
        /// <summary>
        /// Temporarily highlights the name (useful for interactions)
        /// </summary>
        public void HighlightName(Color highlightColor, float duration = 2f)
        {
            if (nameText != null)
            {
                StartCoroutine(HighlightCoroutine(highlightColor, duration));
            }
        }
        
        private System.Collections.IEnumerator HighlightCoroutine(Color highlightColor, float duration)
        {
            Color originalColor = nameText.color;
            nameText.color = highlightColor;
            
            yield return new WaitForSeconds(duration);
            
            // Restore original color based on player state
            UpdateNameDisplay();
        }
        
        void OnDestroy()
        {
            // Cleanup
            if (nameCanvas != null && nameCanvas.gameObject != null)
            {
                Destroy(nameCanvas.gameObject);
            }
        }
        
        // Editor helper to test name changes
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void OnValidate()
        {
            if (Application.isPlaying && isSetup)
            {
                UpdateNameDisplay();
            }
        }
    }
}
