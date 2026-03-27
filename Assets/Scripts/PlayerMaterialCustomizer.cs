using UnityEngine;

namespace UpWeGo
{
    /// <summary>
    /// Applies saved customization colors to the player character on spawn
    /// Attach this to the MainPlayer prefab
    /// </summary>
    public class PlayerMaterialCustomizer : MonoBehaviour
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
        
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        
        void Start()
        {
            // Find ALL SkinnedMeshRenderer components in children
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            {
                Debug.LogError("PlayerMaterialCustomizer: No SkinnedMeshRenderer found on player!");
                return;
            }
            
            if (debugLogging)
            {
                Debug.Log($"Found {skinnedMeshRenderers.Length} SkinnedMeshRenderers");
            }
            
            // Apply saved colors
            ApplyCustomColors();
        }
        
        /// <summary>
        /// Apply saved customization colors to the character materials
        /// </summary>
        public void ApplyCustomColors()
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return;
                
            // Get saved colors
            Color bodyColor = PlayerCustomizationData.GetBodyColor();
            Color pantsColor = PlayerCustomizationData.GetPantsColor();
            
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
                        // If it's already an instance, just modify it
                        // Otherwise create a new instance
                        if (!materials[i].name.Contains("Instance"))
                        {
                            materials[i] = new Material(materials[i]);
                            materialsChanged = true;
                        }
                        
                        // Set the color directly on the material
                        materials[i].SetColor(colorPropertyName, bodyColor);
                        bodyFound = true;
                        
                        if (debugLogging)
                        {
                            Debug.Log($"Applied body color to: {renderer.gameObject.name} - Material: {materials[i].name}");
                        }
                    }
                    else if (materials[i].name.Contains(pantsMaterialName))
                    {
                        // If it's already an instance, just modify it
                        // Otherwise create a new instance
                        if (!materials[i].name.Contains("Instance"))
                        {
                            materials[i] = new Material(materials[i]);
                            materialsChanged = true;
                        }
                        
                        // Set the color directly on the material
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
            
            // Warnings if materials not found
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
