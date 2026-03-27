using UnityEngine;
using Mirror;

namespace UpWeGo
{
    /// <summary>
    /// Applies saved customization colors to the player character on spawn and syncs them over the network.
    /// Attach this to the MainPlayer prefab.
    /// </summary>
    public class PlayerMaterialCustomizer : NetworkBehaviour
    {
        [Header("Material Configuration")]
        [Tooltip("Name of the Body material (must match exactly)")]
        [SerializeField] private string bodyMaterialName = "Body";
        
        [Tooltip("Name of the Pants material (must match exactly)")]
        [SerializeField] private string pantsMaterialName = "Pants";
        
        [Tooltip("Shader color property name (usually _BaseColor for TCP2)")]
        [SerializeField] private string colorPropertyName = "_BaseColor";
        
        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;

        [SyncVar(hook = nameof(OnBodyColorSynced))]
        private Color syncedBodyColor = Color.clear;

        [SyncVar(hook = nameof(OnPantsColorSynced))]
        private Color syncedPantsColor = Color.clear;
        
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        
        void Awake()
        {
            // Find ALL SkinnedMeshRenderer components in children
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        void Start()
        {
            
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            {
                Debug.LogError("PlayerMaterialCustomizer: No SkinnedMeshRenderer found on player!");
                return;
            }
            
            if (debugLogging)
            {
                Debug.Log($"Found {skinnedMeshRenderers.Length} SkinnedMeshRenderers");
            }
            
            // If we are NOT spawned on the network (e.g. preview character) or don't have an identity, apply local colors immediately
            NetworkIdentity identity = GetComponent<NetworkIdentity>();
            if (identity == null || (!identity.isServer && !identity.isClient))
            {
                ApplyCustomColors();
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // Read local colors and tell server to update them for everyone
            Color localBody = PlayerCustomizationData.GetBodyColor();
            Color localPants = PlayerCustomizationData.GetPantsColor();
            CmdSetColors(localBody, localPants);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            // For late joiners or when first spawned (but not local player, or before RPC arrives)
            // Apply whatever colors are already synced
            if (syncedBodyColor != Color.clear && syncedPantsColor != Color.clear)
            {
                ApplyColorsDirectly(syncedBodyColor, syncedPantsColor);
            }
        }

        [Command]
        private void CmdSetColors(Color body, Color pants)
        {
            syncedBodyColor = body;
            syncedPantsColor = pants;
            // Note: Server also needs to apply them if running as host, but SyncVar hook handles this automatically on host.
        }

        private void OnBodyColorSynced(Color oldColor, Color newColor)
        {
            Color pants = syncedPantsColor != Color.clear ? syncedPantsColor : PlayerCustomizationData.GetPantsColor();
            ApplyColorsDirectly(newColor, pants);
        }

        private void OnPantsColorSynced(Color oldColor, Color newColor)
        {
            Color body = syncedBodyColor != Color.clear ? syncedBodyColor : PlayerCustomizationData.GetBodyColor();
            ApplyColorsDirectly(body, newColor);
        }
        
        /// <summary>
        /// Apply local saved customization colors (used mainly for preview character)
        /// </summary>
        public void ApplyCustomColors()
        {
            Color bodyColor = PlayerCustomizationData.GetBodyColor();
            Color pantsColor = PlayerCustomizationData.GetPantsColor();
            ApplyColorsDirectly(bodyColor, pantsColor);
        }

        /// <summary>
        /// Apply specific colors to the character materials
        /// </summary>
        private void ApplyColorsDirectly(Color bodyColor, Color pantsColor)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return;
                
            if (debugLogging)
            {
                Debug.Log($"Applying colors - Body: {bodyColor}, Pants: {pantsColor}");
            }
            
            bool bodyFound = false;
            bool pantsFound = false;
            
            // Check all SkinnedMeshRenderers
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
            {
                if (renderer == null) continue;
                
                // Get all materials on this renderer
                Material[] materials = renderer.materials;
                bool materialsChanged = false;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains(bodyMaterialName))
                    {
                        if (!materials[i].name.Contains("Instance"))
                        {
                            materials[i] = new Material(materials[i]);
                            materialsChanged = true;
                        }
                        
                        materials[i].SetColor(colorPropertyName, bodyColor);
                        bodyFound = true;
                        
                        if (debugLogging)
                        {
                            Debug.Log($"Applied body color to: {renderer.gameObject.name} - Material: {materials[i].name}");
                        }
                    }
                    else if (materials[i].name.Contains(pantsMaterialName))
                    {
                        if (!materials[i].name.Contains("Instance"))
                        {
                            materials[i] = new Material(materials[i]);
                            materialsChanged = true;
                        }
                        
                        materials[i].SetColor(colorPropertyName, pantsColor);
                        pantsFound = true;
                        
                        if (debugLogging)
                        {
                            Debug.Log($"Applied pants color to: {renderer.gameObject.name} - Material: {materials[i].name}");
                        }
                    }
                }
                
                // Only apply back if we created new instances
                if (materialsChanged)
                {
                    renderer.materials = materials;
                }
            }
            
            if (!bodyFound && debugLogging)
            {
                Debug.LogWarning($"Body material '{bodyMaterialName}' not found in any renderer!");
            }
            
            if (!pantsFound && debugLogging)
            {
                Debug.LogWarning($"Pants material '{pantsMaterialName}' not found in any renderer!");
            }
        }
    }
}
