using UnityEngine;
using Mirror;
using TMPro;
using Steamworks;

public class PlayerNameDisplay : NetworkBehaviour
{
    [Header("Name Display Settings")]
    [SerializeField] private GameObject nameDisplayPrefab;
    [SerializeField] private float displayDistance = 10f;
    [SerializeField] private Vector3 nameOffset = new Vector3(0, 2.5f, 0);
    
    [Header("References")]
    [SerializeField] private Transform nameDisplayParent;
    
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
        if (nameDisplayPrefab != null)
        {
            nameDisplayInstance = Instantiate(nameDisplayPrefab, transform);
            nameDisplayInstance.transform.localPosition = nameOffset;
            
            nameText = nameDisplayInstance.GetComponentInChildren<TextMeshProUGUI>();
            nameCanvas = nameDisplayInstance.GetComponent<Canvas>();
            
            if (nameText != null)
            {
                nameText.text = steamName;
            }
            
            if (nameCanvas != null)
            {
                nameCanvas.worldCamera = mainCamera;
            }
            
            nameDisplayInstance.SetActive(false);
        }
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