#if UNITY_EDITOR && BAKERY_INCLUDED
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class BakeryModeUtility : EditorWindow
{
    [MenuItem("Window/Bakery Mode Utility")]
    private static void Init()
    {
        var window = (BakeryModeUtility)GetWindow(typeof(BakeryModeUtility));
        window.Show();
    }
    
    private static MethodInfo _getShaderGlobalKeywords = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
    private static MethodInfo _getShaderLocalKeywords = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
    
    private void OnGUI()
    {

        if (GUILayout.Button(new GUIContent("Mono SH", "Enable BAKERY_MONOSH on all materials if the shader supports it")))
        {
            var bakeryMaterials = FindSupportedObjects();
            foreach (var material in bakeryMaterials.Keys)
            {
                material.DisableKeyword("BAKERY_SH");
                material.DisableKeyword("BAKERY_RNM");
                material.EnableKeyword("BAKERY_MONOSH");
                ToggleProperties(material, false, false, true);
            }
        }

        if (GUILayout.Button(new GUIContent("SH", "Enable BAKERY_SH on all materials if the shader supports it")))
        {
            var bakeryMaterials = FindSupportedObjects();
            foreach (var material in bakeryMaterials.Keys)
            {
                material.EnableKeyword("BAKERY_SH");
                material.DisableKeyword("BAKERY_RNM");
                material.DisableKeyword("BAKERY_MONOSH");
                ToggleProperties(material, true, false, false);
            }
        }
        
        if (GUILayout.Button(new GUIContent("RNM", "Enable BAKERY_RNM on all materials if the shader supports it")))
        {
            var bakeryMaterials = FindSupportedObjects();
            foreach (var material in bakeryMaterials.Keys)
            {
                material.EnableKeyword("BAKERY_RNM");
                material.DisableKeyword("BAKERY_SH");
                material.DisableKeyword("BAKERY_MONOSH");
                ToggleProperties(material, false, true, false);
            }
        }
        
        if (GUILayout.Button(new GUIContent("Disable", "Disable BAKERY_SH, BAKERY_RNM and BAKERY_MONOSH on all materials")))
        {
            DisableAll();
        }

        EditorGUILayout.HelpBox(new GUIContent("Only supports shaders with BAKERY_SH, BAKERY_RNM or BAKERY_MONOSH keyword. Toggles on some materials will not be updated visually, only keywords get applied"));
    }

    private static void DisableAll()
    {
        var renderers = FindObjectsOfType<Renderer>().ToList();
        var materials = renderers.SelectMany(x => x.sharedMaterials).Distinct().ToList();

        foreach (var material in materials)
        {
            if (material is null) continue;
            material.DisableKeyword("BAKERY_SH");
            material.DisableKeyword("BAKERY_RNM");
            material.DisableKeyword("BAKERY_MONOSH");
            ToggleProperties(material, false, false, false);
        }
    }

    private static Dictionary<Material, int> FindSupportedObjects()
    {
        var renderers = FindObjectsOfType<Renderer>().ToList();
        
        
        var materials = renderers.SelectMany(x => x.sharedMaterials).Distinct().ToList();
        var supportedMaterials = new List<Material>();

        foreach (var material in materials)
        {
            if (material is null || material.shader is null) continue;

            var globalKeywords = (string[])_getShaderGlobalKeywords.Invoke(null, new object[] { material.shader });
            var localKeywords = (string[])_getShaderLocalKeywords.Invoke(null, new object[] { material.shader });

            
            if (!globalKeywords.Contains("BAKERY_SH") && !globalKeywords.Contains("BAKERY_RNM") &&
                !localKeywords.Contains("BAKERY_SH") && !localKeywords.Contains("BAKERY_RNM"))
            {
                continue;
            }
            
            supportedMaterials.Add(material);
        }

        return supportedMaterials.ToDictionary(m => m, m => 0);
    }


    // tries to set the float on common property names
    private static void ToggleProperties(Material material, bool SH, bool RNM, bool MonoSH)
    {
        material.SetFloat("_BAKERY_SH", SH ? 1 : 0);
        material.SetFloat("_BAKERY_RNM", RNM ? 1 : 0);
        material.SetFloat("_BAKERY_MONOSH", MonoSH ? 1 : 0);

        if (!SH && !RNM && !MonoSH)
        {
            material.SetFloat("Bakery", 0);
            material.SetFloat("_BakeryMode", 0);
            material.SetFloat("_Bakery", 0);
        }
        else if (SH)
        {
            material.SetFloat("Bakery", 1);
            material.SetFloat("_BakeryMode", 1);
            material.SetFloat("_Bakery", 1);
        }
        else if (RNM)
        {
            material.SetFloat("Bakery", 2);
            material.SetFloat("_BakeryMode", 2);
            material.SetFloat("_Bakery", 2);
        }
        else if (MonoSH)
        {
            material.SetFloat("Bakery", 3);
            material.SetFloat("_BakeryMode", 3);
            material.SetFloat("_Bakery", 3);
        }

    }
}
#endif