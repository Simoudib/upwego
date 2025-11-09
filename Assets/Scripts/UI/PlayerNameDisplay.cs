using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using Steamworks;

public class PlayerNameDisplay : NetworkBehaviour
{
    [Header("Name Display Settings")]
    [SerializeField] private bool createDisplayProgrammatically = true;
    [SerializeField] private GameObject nameDisplayPrefab; // Optional: use if you want custom prefab
    [SerializeField] private float displayDistance = 10f;
    [SerializeField] private Vector3 nameOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float textSize = 36f;
    [SerializeField] private Color textColor = Color.white;
    
    [SyncVar(hook = nameof(OnSteamNameChanged))]
    private string steamName = "";
    
    private GameObject nameDisplayInstance;
    private TextMeshProUGUI nameText;
    private Canvas nameCanvas;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (isLocalPlayer)
        {
            // Get Steam name and send to server
            if (SteamManager.Initialized)
            {
                string localSteamName = SteamFriends.GetPersonaName();
                CmdSetSteamName(localSteamName);
            }
        }
        else
        {
            // Create name display for other players
            CreateNameDisplay();
        }
    }
    
    [Command]
    void CmdSetSteamName(string name)
    {
        steamName = name;
    }
    
    void OnSteamNameChanged(string oldName, string newName)
    {
        if (nameText != null)
        {
            nameText.text = newName;
        }
    }
    
    void CreateNameDisplay()
    {
        if (nameDisplayPrefab != null && !createDisplayProgrammatically)
        {
            // Use assigned prefab
            nameDisplayInstance = Instantiate(nameDisplayPrefab, transform);
            nameDisplayInstance.transform.localPosition = nameOffset;
            
            nameText = nameDisplayInstance.GetComponentInChildren<TextMeshProUGUI>();
            nameCanvas = nameDisplayInstance.GetComponent<Canvas>();
        }
        else
        {
            // Create programmatically
            nameDisplayInstance = new GameObject("NameDisplay");
            nameDisplayInstance.transform.SetParent(transform);
            nameDisplayInstance.transform.localPosition = nameOffset;
            
            // Create canvas
            nameCanvas = nameDisplayInstance.AddComponent<Canvas>();
            nameCanvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform canvasRect = nameCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            // Add CanvasScaler for better scaling
            CanvasScaler scaler = nameDisplayInstance.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            
            // Create text object
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(nameDisplayInstance.transform, false);
            
            nameText = textObj.AddComponent<TextMeshProUGUI>();
            nameText.text = steamName;
            nameText.fontSize = textSize;
            nameText.color = textColor;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableWordWrapping = false;
            
            // Add outline for better visibility
            nameText.outlineWidth = 0.2f;
            nameText.outlineColor = new Color(0, 0, 0, 0.8f);
            
            RectTransform textRect = nameText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
        
        if (nameCanvas != null)
        {
            nameCanvas.worldCamera = mainCamera;
        }
        
        if (nameText != null)
        {
            nameText.text = steamName;
        }
        
        nameDisplayInstance.SetActive(false);
    }
    
    void Update()
    {
        if (isLocalPlayer || nameDisplayInstance == null) return;
        
        // Check distance to local player
        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, localPlayer.transform.position);
            
            bool shouldShow = distance <= displayDistance;
            if (nameDisplayInstance.activeSelf != shouldShow)
            {
                nameDisplayInstance.SetActive(shouldShow);
            }
            
            // Make name face camera
            if (shouldShow && mainCamera != null)
            {
                nameDisplayInstance.transform.LookAt(mainCamera.transform);
                nameDisplayInstance.transform.Rotate(0, 180, 0);
            }
        }
    }
    
    GameObject GetLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<PlayerNameDisplay>())
        {
            if (player.isLocalPlayer)
            {
                return player.gameObject;
            }
        }
        return null;
    }
    
    void OnDestroy()
    {
        if (nameDisplayInstance != null)
        {
            Destroy(nameDisplayInstance);
        }
    }
}