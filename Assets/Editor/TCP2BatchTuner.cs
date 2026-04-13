using UnityEngine;
using UnityEditor;

public class TCP2BatchTuner : EditorWindow
{
    // ⚠️ Paste your generated TCP2 shader name here
    static string tcp2ShaderName = "Toony Colors Pro 2/Desktop/Hybrid Shader 2";

    [MenuItem("Tools/TCP2 Convert and Tune All")]
    static void ConvertAndTuneAll()
    {
        Shader tcp2Shader = Shader.Find(tcp2ShaderName);

        if (tcp2Shader == null)
        {
            Debug.LogError("❌ Shader not found: " + tcp2ShaderName);
            Debug.LogError("Check the exact name in your Shader Generator output.");
            return;
        }

        Renderer[] renderers = FindObjectsOfType<Renderer>();
        int converted = 0;
        int tuned = 0;

        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                // Convert URP/Lit → TCP2
                if (mat.shader.name.Contains("Universal Render Pipeline/Lit") ||
                    mat.shader.name.Contains("Standard"))
                {
                    mat.shader = tcp2Shader;
                    converted++;
                }

                // Tune TCP2 materials
                if (mat.shader.name.Contains("Toony"))
                {
                    if (mat.HasProperty("_RampThreshold"))
                        mat.SetFloat("_RampThreshold", 0.55f);
                    if (mat.HasProperty("_RampSmoothing"))
                        mat.SetFloat("_RampSmoothing", 0.9f);
                    if (mat.HasProperty("_ShadowStrength"))
                        mat.SetFloat("_ShadowStrength", 0.15f);
                    if (mat.HasProperty("_SpecularColor"))
                        mat.SetColor("_SpecularColor", Color.black);
                    if (mat.HasProperty("_RimStrength"))
                        mat.SetFloat("_RimStrength", 0f);

                    EditorUtility.SetDirty(mat);
                    tuned++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Converted: " + converted + " materials | Tuned: " + tuned + " materials.");
    }
}