using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Simple UI for players to set their display name
    /// </summary>
    public class PlayerNameInputUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button randomNameButton;
        [SerializeField] private TextMeshProUGUI instructionText;
        
        [Header("Settings")]
        [SerializeField] private int maxNameLength = 20;
        [SerializeField] private int minNameLength = 2;
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.N; // Press N to open name input
        
        [Header("Random Names")]
        [SerializeField] private string[] randomFirstNames = {
            "Alex", "Jordan", "Taylor", "Casey", "Morgan", "Riley", "Avery", "Quinn",
            "Sage", "River", "Phoenix", "Rowan", "Blake", "Cameron", "Dakota", "Emery"
        };
        [SerializeField] private string[] randomLastNames = {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas"
        };
        
        private EnhancedPlayerMovement localPlayer;
        private bool isNameSet = false;
        
        void Start()
        {
            // Setup UI if needed
            if (nameInputPanel == null)
            {
                CreateNameInputUI();
            }
            
            // Setup button events
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmName);
            }
            
            if (randomNameButton != null)
            {
                randomNameButton.onClick.AddListener(OnRandomName);
            }
            
            if (nameInputField != null)
            {
                nameInputField.onValueChanged.AddListener(OnNameInputChanged);
                nameInputField.characterLimit = maxNameLength;
            }
            
            // Find local player
            FindLocalPlayer();
            
            // Show name input if enabled
            if (showOnStart)
            {
                ShowNameInput();
            }
            else
            {
                HideNameInput();
            }
        }
        
        void Update()
        {
            // Toggle name input with key
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleNameInput();
            }
            
            // Find local player if we don't have one
            if (localPlayer == null)
            {
                FindLocalPlayer();
            }
        }
        
        /// <summary>
        /// Finds the local player's EnhancedPlayerMovement component
        /// </summary>
        void FindLocalPlayer()
        {
            // Find all players and check which one is local
            EnhancedPlayerMovement[] players = FindObjectsOfType<EnhancedPlayerMovement>();
            foreach (var player in players)
            {
                if (player.isLocalPlayer)
                {
                    localPlayer = player;
                    Debug.Log($"🎯 Found local player: {player.gameObject.name}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Shows the name input UI
        /// </summary>
        public void ShowNameInput()
        {
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(true);
                
                // Focus the input field
                if (nameInputField != null)
                {
                    nameInputField.Select();
                    nameInputField.ActivateInputField();
                    
                    // Set current name if available
                    if (localPlayer != null && !string.IsNullOrEmpty(localPlayer.PlayerDisplayName))
                    {
                        nameInputField.text = localPlayer.PlayerDisplayName;
                    }
                }
                
                // Update instruction text
                UpdateInstructionText();
                
                // Unlock cursor for UI interaction
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        /// <summary>
        /// Hides the name input UI
        /// </summary>
        public void HideNameInput()
        {
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
                
                // Lock cursor back for gameplay
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        /// <summary>
        /// Toggles the name input UI
        /// </summary>
        public void ToggleNameInput()
        {
            if (nameInputPanel != null)
            {
                if (nameInputPanel.activeSelf)
                {
                    HideNameInput();
                }
                else
                {
                    ShowNameInput();
                }
            }
        }
        
        /// <summary>
        /// Called when the confirm button is clicked
        /// </summary>
        void OnConfirmName()
        {
            if (nameInputField == null || localPlayer == null) return;
            
            string newName = nameInputField.text.Trim();
            
            // Validate name
            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogWarning("⚠️ Name cannot be empty!");
                return;
            }
            
            if (newName.Length < minNameLength)
            {
                Debug.LogWarning($"⚠️ Name must be at least {minNameLength} characters long!");
                return;
            }
            
            if (newName.Length > maxNameLength)
            {
                Debug.LogWarning($"⚠️ Name must be no more than {maxNameLength} characters long!");
                return;
            }
            
            // Send name to server via command
            localPlayer.CmdSetPlayerName(newName);
            
            isNameSet = true;
            HideNameInput();
            
            Debug.Log($"✅ Name set to: {newName}");
        }
        
        /// <summary>
        /// Called when the random name button is clicked
        /// </summary>
        void OnRandomName()
        {
            if (nameInputField == null) return;
            
            string randomName = GenerateRandomName();
            nameInputField.text = randomName;
            
            Debug.Log($"🎲 Generated random name: {randomName}");
        }
        
        /// <summary>
        /// Called when the name input field value changes
        /// </summary>
        void OnNameInputChanged(string value)
        {
            UpdateInstructionText();
            
            // Enable/disable confirm button based on validity
            if (confirmButton != null)
            {
                bool isValid = !string.IsNullOrEmpty(value.Trim()) && 
                              value.Trim().Length >= minNameLength && 
                              value.Trim().Length <= maxNameLength;
                confirmButton.interactable = isValid;
            }
        }
        
        /// <summary>
        /// Updates the instruction text based on current input
        /// </summary>
        void UpdateInstructionText()
        {
            if (instructionText == null || nameInputField == null) return;
            
            string currentName = nameInputField.text.Trim();
            int length = currentName.Length;
            
            if (length == 0)
            {
                instructionText.text = $"Enter your name ({minNameLength}-{maxNameLength} characters)";
                instructionText.color = Color.gray;
            }
            else if (length < minNameLength)
            {
                instructionText.text = $"Too short! Need at least {minNameLength} characters ({length}/{maxNameLength})";
                instructionText.color = Color.red;
            }
            else if (length > maxNameLength)
            {
                instructionText.text = $"Too long! Maximum {maxNameLength} characters ({length}/{maxNameLength})";
                instructionText.color = Color.red;
            }
            else
            {
                instructionText.text = $"Good! Press Confirm to set name ({length}/{maxNameLength})";
                instructionText.color = Color.green;
            }
        }
        
        /// <summary>
        /// Generates a random name from the predefined lists
        /// </summary>
        string GenerateRandomName()
        {
            if (randomFirstNames.Length == 0 || randomLastNames.Length == 0)
            {
                return $"Player{Random.Range(1000, 9999)}";
            }
            
            string firstName = randomFirstNames[Random.Range(0, randomFirstNames.Length)];
            string lastName = randomLastNames[Random.Range(0, randomLastNames.Length)];
            
            return $"{firstName} {lastName}";
        }
        
        /// <summary>
        /// Creates a basic name input UI if none is assigned
        /// </summary>
        void CreateNameInputUI()
        {
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("NameInputCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create main panel
            GameObject panel = new GameObject("NameInputPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(400, 200);
            
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Create input field
            GameObject inputFieldObj = new GameObject("NameInputField");
            inputFieldObj.transform.SetParent(panel.transform, false);
            
            RectTransform inputRect = inputFieldObj.AddComponent<RectTransform>();
            inputRect.anchoredPosition = new Vector2(0, 20);
            inputRect.sizeDelta = new Vector2(350, 40);
            
            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = Color.white;
            
            nameInputField = inputFieldObj.AddComponent<TMP_InputField>();
            nameInputField.textComponent = inputFieldObj.AddComponent<TextMeshProUGUI>();
            nameInputField.placeholder = inputFieldObj.AddComponent<TextMeshProUGUI>();
            ((TextMeshProUGUI)nameInputField.placeholder).text = "Enter your name...";
            ((TextMeshProUGUI)nameInputField.placeholder).color = Color.gray;
            
            // Create confirm button
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(panel.transform, false);
            
            RectTransform confirmRect = confirmObj.AddComponent<RectTransform>();
            confirmRect.anchoredPosition = new Vector2(-80, -40);
            confirmRect.sizeDelta = new Vector2(100, 40);
            
            Image confirmBg = confirmObj.AddComponent<Image>();
            confirmBg.color = Color.green;
            
            confirmButton = confirmObj.AddComponent<Button>();
            
            GameObject confirmText = new GameObject("Text");
            confirmText.transform.SetParent(confirmObj.transform, false);
            TextMeshProUGUI confirmTextComp = confirmText.AddComponent<TextMeshProUGUI>();
            confirmTextComp.text = "Confirm";
            confirmTextComp.alignment = TextAlignmentOptions.Center;
            confirmTextComp.color = Color.white;
            
            // Create random button
            GameObject randomObj = new GameObject("RandomButton");
            randomObj.transform.SetParent(panel.transform, false);
            
            RectTransform randomRect = randomObj.AddComponent<RectTransform>();
            randomRect.anchoredPosition = new Vector2(80, -40);
            randomRect.sizeDelta = new Vector2(100, 40);
            
            Image randomBg = randomObj.AddComponent<Image>();
            randomBg.color = Color.blue;
            
            randomNameButton = randomObj.AddComponent<Button>();
            
            GameObject randomText = new GameObject("Text");
            randomText.transform.SetParent(randomObj.transform, false);
            TextMeshProUGUI randomTextComp = randomText.AddComponent<TextMeshProUGUI>();
            randomTextComp.text = "Random";
            randomTextComp.alignment = TextAlignmentOptions.Center;
            randomTextComp.color = Color.white;
            
            // Create instruction text
            GameObject instructionObj = new GameObject("InstructionText");
            instructionObj.transform.SetParent(panel.transform, false);
            
            RectTransform instructionRect = instructionObj.AddComponent<RectTransform>();
            instructionRect.anchoredPosition = new Vector2(0, 70);
            instructionRect.sizeDelta = new Vector2(350, 30);
            
            instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = $"Enter your name ({minNameLength}-{maxNameLength} characters)";
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.fontSize = 14;
            instructionText.color = Color.gray;
            
            nameInputPanel = panel;
            
            Debug.Log("✅ Created name input UI automatically");
        }
        
        /// <summary>
        /// Public method to set name programmatically
        /// </summary>
        public void SetName(string name)
        {
            if (nameInputField != null)
            {
                nameInputField.text = name;
                OnConfirmName();
            }
        }
        
        void OnDestroy()
        {
            // Cleanup button listeners
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
            }
            
            if (randomNameButton != null)
            {
                randomNameButton.onClick.RemoveAllListeners();
            }
            
            if (nameInputField != null)
            {
                nameInputField.onValueChanged.RemoveAllListeners();
            }
        }
    }
}

