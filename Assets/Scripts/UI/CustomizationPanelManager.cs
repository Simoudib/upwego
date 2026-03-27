using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UpWeGo
{
    /// <summary>
    /// Manages the Character Customization UI Panel
    /// Handles color sliders, preview updates, and saving
    /// </summary>
    public class CustomizationPanelManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider bodyColorSlider;
        [SerializeField] private Slider pantsColorSlider;
        [SerializeField] private Slider bodyBrightnessSlider;
        [SerializeField] private Slider pantsBrightnessSlider;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button backButton;
        
        [Header("Preview")]
        [Tooltip("Optional: Player prefab to show as preview (leave empty to disable preview)")]
        [SerializeField] private GameObject playerPreviewPrefab;
        
        [Tooltip("Where to spawn the preview character in 3D space")]
        [SerializeField] private Vector3 previewPosition = new Vector3(2, 0, 3);
        
        [Tooltip("Which direction the preview character should face")]
        [SerializeField] private Vector3 previewRotation = new Vector3(0, 180, 0);
        
        [Header("Panel Swapper")]
        [SerializeField] private PanelSwapper panelSwapper;
        [SerializeField] private string returnPanelName = "MainPanel";
        
        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;
        
        private GameObject previewCharacter;
        private PlayerMaterialCustomizer previewCustomizer;
        private float currentBodyHue;
        private float currentPantsHue;
        private float currentBodyBrightness;
        private float currentPantsBrightness;
        
        void Awake()
        {
            // Hook up button events
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
                
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
                
            // Hook up slider events
            if (bodyColorSlider != null)
                bodyColorSlider.onValueChanged.AddListener(OnBodySliderChanged);
                
            if (pantsColorSlider != null)
                pantsColorSlider.onValueChanged.AddListener(OnPantsSliderChanged);

            if (bodyBrightnessSlider != null)
                bodyBrightnessSlider.onValueChanged.AddListener(OnBodyBrightnessChanged);
                
            if (pantsBrightnessSlider != null)
                pantsBrightnessSlider.onValueChanged.AddListener(OnPantsBrightnessChanged);
        }
        
        void OnEnable()
        {
            // Initialize when panel becomes active
            InitializePanel();
        }
        
        void OnDisable()
        {
            // Clean up preview when panel is hidden
            if (previewCharacter != null)
            {
                Destroy(previewCharacter);
                previewCharacter = null;
            }
        }
        
        /// <summary>
        /// Initialize the panel with saved colors
        /// </summary>
        private void InitializePanel()
        {
            // Load saved colors
            Color savedBodyColor = PlayerCustomizationData.GetBodyColor();
            Color savedPantsColor = PlayerCustomizationData.GetPantsColor();
            
            // Convert to hue for sliders
            PlayerCustomizationData.ColorToHueAndBrightness(savedBodyColor, out currentBodyHue, out currentBodyBrightness);
            PlayerCustomizationData.ColorToHueAndBrightness(savedPantsColor, out currentPantsHue, out currentPantsBrightness);
            
            // Set slider values without triggering events
            if (bodyColorSlider != null)
            {
                bodyColorSlider.SetValueWithoutNotify(currentBodyHue);
            }
            
            if (pantsColorSlider != null)
            {
                pantsColorSlider.SetValueWithoutNotify(currentPantsHue);
            }

            if (bodyBrightnessSlider != null)
            {
                bodyBrightnessSlider.SetValueWithoutNotify(currentBodyBrightness);
            }
            
            if (pantsBrightnessSlider != null)
            {
                pantsBrightnessSlider.SetValueWithoutNotify(currentPantsBrightness);
            }
            
            // Spawn preview character
            SpawnPreviewCharacter();
            
            if (debugLogging)
            {
                Debug.Log($"CustomizationPanel initialized - Body: H{currentBodyHue}/V{currentBodyBrightness}, Pants: H{currentPantsHue}/V{currentPantsBrightness}");
            }
        }
        
        /// <summary>
        /// Spawn or refresh the preview character
        /// </summary>
        private void SpawnPreviewCharacter()
        {
            // Clean up existing preview
            if (previewCharacter != null)
            {
                Destroy(previewCharacter);
            }
            
            // Only spawn if prefab is assigned
            if (playerPreviewPrefab == null)
            {
                if (debugLogging)
                    Debug.Log("Preview disabled - no prefab assigned");
                return;
            }
            
            // Spawn preview character in world space
            Quaternion rotation = Quaternion.Euler(previewRotation);
            previewCharacter = Instantiate(playerPreviewPrefab, previewPosition, rotation);
            
            // Get or add the customizer component
            previewCustomizer = previewCharacter.GetComponent<PlayerMaterialCustomizer>();
            if (previewCustomizer == null)
            {
                previewCustomizer = previewCharacter.AddComponent<PlayerMaterialCustomizer>();
            }
            
            // Disable any scripts that shouldn't run on preview
            DisablePreviewScripts();
            
            // Apply current colors
            UpdatePreviewColors();
            
            if (debugLogging)
            {
                Debug.Log($"Preview character spawned at {previewPosition}");
            }
        }
        
        /// <summary>
        /// Disable scripts that shouldn't run on the preview character
        /// </summary>
        private void DisablePreviewScripts()
        {
            if (previewCharacter == null) return;
            
            // Disable movement, networking, etc.
            var playerMovement = previewCharacter.GetComponent<EnhancedPlayerMovement>();
            if (playerMovement != null)
                playerMovement.enabled = false;
                
            var networkIdentity = previewCharacter.GetComponent<Mirror.NetworkIdentity>();
            if (networkIdentity != null)
                Destroy(networkIdentity);
                
            // Disable character controller to prevent falling
            var charController = previewCharacter.GetComponent<CharacterController>();
            if (charController != null)
                charController.enabled = false;
                
            // Disable rigidbody to prevent physics
            var rb = previewCharacter.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
                
            // Disable any other NetworkBehaviour components (except customizer so preview works if it's already attached)
            var networkBehaviours = previewCharacter.GetComponents<Mirror.NetworkBehaviour>();
            foreach (var behaviour in networkBehaviours)
            {
                if (behaviour != null && !(behaviour is PlayerMaterialCustomizer))
                    Destroy(behaviour);
            }
        }
        
        /// <summary>
        /// Update preview character colors
        /// </summary>
        private void UpdatePreviewColors()
        {
            if (previewCustomizer == null) return;
            
            // Temporarily save current colors to apply to preview
            Color bodyColor = PlayerCustomizationData.HueAndBrightnessToColor(currentBodyHue, currentBodyBrightness);
            Color pantsColor = PlayerCustomizationData.HueAndBrightnessToColor(currentPantsHue, currentPantsBrightness);
            
            // Save temporarily
            Color originalBody = PlayerCustomizationData.GetBodyColor();
            Color originalPants = PlayerCustomizationData.GetPantsColor();
            
            PlayerCustomizationData.SaveBodyColor(bodyColor);
            PlayerCustomizationData.SavePantsColor(pantsColor);
            
            // Apply to preview
            previewCustomizer.ApplyCustomColors();
            
            // Restore original saved colors (so preview doesn't affect actual saved data)
            PlayerCustomizationData.SaveBodyColor(originalBody);
            PlayerCustomizationData.SavePantsColor(originalPants);
        }
        
        /// <summary>
        /// Called when body color slider changes
        /// </summary>
        private void OnBodySliderChanged(float hue)
        {
            currentBodyHue = hue;
            UpdatePreviewColors();
            
            if (debugLogging)
            {
                Debug.Log($"Body hue changed: {hue} -> Color: {PlayerCustomizationData.HueAndBrightnessToColor(hue, currentBodyBrightness)}");
            }
        }
        
        /// <summary>
        /// Called when pants color slider changes
        /// </summary>
        private void OnPantsSliderChanged(float hue)
        {
            currentPantsHue = hue;
            UpdatePreviewColors();
            
            if (debugLogging)
            {
                Debug.Log($"Pants hue changed: {hue} -> Color: {PlayerCustomizationData.HueAndBrightnessToColor(hue, currentPantsBrightness)}");
            }
        }

        private void OnBodyBrightnessChanged(float brightness)
        {
            currentBodyBrightness = brightness;
            UpdatePreviewColors();
            
            if (debugLogging)
            {
                Debug.Log($"Body brightness changed: {brightness} -> Color: {PlayerCustomizationData.HueAndBrightnessToColor(currentBodyHue, brightness)}");
            }
        }

        private void OnPantsBrightnessChanged(float brightness)
        {
            currentPantsBrightness = brightness;
            UpdatePreviewColors();
            
            if (debugLogging)
            {
                Debug.Log($"Pants brightness changed: {brightness} -> Color: {PlayerCustomizationData.HueAndBrightnessToColor(currentPantsHue, brightness)}");
            }
        }
        
        /// <summary>
        /// Called when Apply button is clicked
        /// </summary>
        private void OnApplyClicked()
        {
            // Save the current colors
            Color bodyColor = PlayerCustomizationData.HueAndBrightnessToColor(currentBodyHue, currentBodyBrightness);
            Color pantsColor = PlayerCustomizationData.HueAndBrightnessToColor(currentPantsHue, currentPantsBrightness);
            
            PlayerCustomizationData.SaveBodyColor(bodyColor);
            PlayerCustomizationData.SavePantsColor(pantsColor);
            
            if (debugLogging)
            {
                Debug.Log($"Colors saved - Body: {bodyColor}, Pants: {pantsColor}");
            }
            
            // Return to main panel
            ReturnToMainPanel();
        }
        
        /// <summary>
        /// Called when Back button is clicked
        /// </summary>
        private void OnBackClicked()
        {
            if (debugLogging)
            {
                Debug.Log("Back button clicked - discarding changes");
            }
            
            // Return without saving
            ReturnToMainPanel();
        }
        
        /// <summary>
        /// Return to the main panel
        /// </summary>
        private void ReturnToMainPanel()
        {
            if (panelSwapper != null)
            {
                panelSwapper.SwapPanel(returnPanelName);
            }
            else
            {
                Debug.LogError("PanelSwapper reference is missing!");
            }
        }
    }
}
