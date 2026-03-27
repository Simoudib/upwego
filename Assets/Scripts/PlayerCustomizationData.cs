using UnityEngine;

namespace UpWeGo
{
    /// <summary>
    /// Static class to store and persist player customization data
    /// Uses PlayerPrefs to save colors across game sessions
    /// Each player stores their own customization locally
    /// </summary>
    public static class PlayerCustomizationData
    {
        // PlayerPrefs keys
        private const string BODY_COLOR_R = "PlayerCustomization_BodyColor_R";
        private const string BODY_COLOR_G = "PlayerCustomization_BodyColor_G";
        private const string BODY_COLOR_B = "PlayerCustomization_BodyColor_B";
        
        private const string PANTS_COLOR_R = "PlayerCustomization_PantsColor_R";
        private const string PANTS_COLOR_G = "PlayerCustomization_PantsColor_G";
        private const string PANTS_COLOR_B = "PlayerCustomization_PantsColor_B";
        
        // Default colors
        private static readonly Color DefaultBodyColor = new Color(1f, 0.6f, 0.2f); // Orange (like current)
        private static readonly Color DefaultPantsColor = new Color(0.2f, 0.3f, 0.8f); // Blue
        
        /// <summary>
        /// Save body color to PlayerPrefs
        /// </summary>
        public static void SaveBodyColor(Color color)
        {
            PlayerPrefs.SetFloat(BODY_COLOR_R, color.r);
            PlayerPrefs.SetFloat(BODY_COLOR_G, color.g);
            PlayerPrefs.SetFloat(BODY_COLOR_B, color.b);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Save pants color to PlayerPrefs
        /// </summary>
        public static void SavePantsColor(Color color)
        {
            PlayerPrefs.SetFloat(PANTS_COLOR_R, color.r);
            PlayerPrefs.SetFloat(PANTS_COLOR_G, color.g);
            PlayerPrefs.SetFloat(PANTS_COLOR_B, color.b);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Get saved body color, or default if not set
        /// </summary>
        public static Color GetBodyColor()
        {
            if (!PlayerPrefs.HasKey(BODY_COLOR_R))
                return DefaultBodyColor;
                
            float r = PlayerPrefs.GetFloat(BODY_COLOR_R);
            float g = PlayerPrefs.GetFloat(BODY_COLOR_G);
            float b = PlayerPrefs.GetFloat(BODY_COLOR_B);
            
            return new Color(r, g, b);
        }
        
        /// <summary>
        /// Get saved pants color, or default if not set
        /// </summary>
        public static Color GetPantsColor()
        {
            if (!PlayerPrefs.HasKey(PANTS_COLOR_R))
                return DefaultPantsColor;
                
            float r = PlayerPrefs.GetFloat(PANTS_COLOR_R);
            float g = PlayerPrefs.GetFloat(PANTS_COLOR_G);
            float b = PlayerPrefs.GetFloat(PANTS_COLOR_B);
            
            return new Color(r, g, b);
        }
        
        /// <summary>
        /// Reset to default colors
        /// </summary>
        public static void ResetToDefaults()
        {
            SaveBodyColor(DefaultBodyColor);
            SavePantsColor(DefaultPantsColor);
        }
        
        /// <summary>
        /// Convert hue and brightness (0-1) to full color
        /// Useful for color sliders
        /// </summary>
        public static Color HueAndBrightnessToColor(float hue, float brightness)
        {
            return Color.HSVToRGB(hue, 0.8f, brightness); // 80% saturation, variable brightness
        }
        
        /// <summary>
        /// Convert color to hue and brightness (0-1)
        /// Useful for initializing sliders
        /// </summary>
        public static void ColorToHueAndBrightness(Color color, out float hue, out float brightness)
        {
            Color.RGBToHSV(color, out hue, out float s, out brightness);
        }
    }
}
