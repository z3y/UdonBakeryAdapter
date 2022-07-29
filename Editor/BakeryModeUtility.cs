#if UNITY_EDITOR && BAKERY_INCLUDED
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

[InitializeOnLoad]
public class BakeryModeUtility : EditorWindow
{
    [MenuItem("Window/Bakery Mode Utility")]
    private static void Init()
    {
        var window = (BakeryModeUtility)GetWindow(typeof(BakeryModeUtility));
        window.Show();
    }
    
    static BakeryModeUtility() => ftRenderLightmap.OnFinishedFullRender += AutoSwitch;
    
    private static MethodInfo _getShaderGlobalKeywords = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
    private static MethodInfo _getShaderLocalKeywords = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
    
    private static readonly int BakeryLightmapMode = Shader.PropertyToID("bakeryLightmapMode");

    private const string AutoSwitchPref = "BakeryModeUtilityAutoSwitch";

    private static bool _autoSwitch = false;

    private bool _firstTime = true;
    private void OnGUI()
    {
        if (_firstTime)
        {
            _autoSwitch = EditorPrefs.GetBool(AutoSwitchPref, false);
            _firstTime = false;
        }
        if (GUILayout.Button(new GUIContent("From Property Blocks", "Toggle BAKERY_SH or BAKERY_RNM depending on bakeryLightmapMode from property blocks on all materials if the shader supports it")))
        {
            SetFromPropertyBlocks();
        }
        
        if (GUILayout.Button(new GUIContent("SH", "Enable BAKERY_SH on all materials if the shader supports it")))
        {
            var bakeryMaterials = FindSupportedObjects();
            foreach (var material in bakeryMaterials.Keys)
            {
                material.EnableKeyword("BAKERY_SH");
                material.DisableKeyword("BAKERY_RNM");
                ToggleProperties(material, true, false);
            }
        }
        
        if (GUILayout.Button(new GUIContent("RNM", "Enable BAKERY_RNM on all materials if the shader supports it")))
        {
            var bakeryMaterials = FindSupportedObjects();
            foreach (var material in bakeryMaterials.Keys)
            {
                material.EnableKeyword("BAKERY_RNM");
                material.DisableKeyword("BAKERY_SH");
                ToggleProperties(material, false, true);
            }
        }
        
        if (GUILayout.Button(new GUIContent("Disable", "Disable BAKERY_SH and BAKERY_RNM on all materials")))
        {
            DisableAll();
        }
        
        EditorGUI.BeginChangeCheck();
        _autoSwitch = EditorGUILayout.ToggleLeft(new GUIContent("Auto Switch", "Toggle BAKERY_SH or BAKERY_RNM automatically after the bake is finished based on property blocks"), _autoSwitch);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetBool(AutoSwitchPref, _autoSwitch);
        }

        EditorGUILayout.HelpBox(new GUIContent("Supports shaders with BAKERY_SH or BAKERY_RNM keyword. Toggles on some materials will not be updated visually, only keywords get applied. Keywords on materials are visible in debug mode"));
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
            ToggleProperties(material, false, false);
        }
    }

    private static Dictionary<Material, int> FindSupportedObjects(bool checkPropertyBlocks = false)
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

        if (!checkPropertyBlocks) return supportedMaterials.ToDictionary(m => m, m => 0);

        
        var storage = ftRenderLightmap.FindRenderSettingsStorage();
        var bakeryMaterials = new Dictionary<Material, int>();

        foreach (var renderer in renderers)
        {
            if (!renderer.HasPropertyBlock()) continue;
            
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            int bakeryMode = storage.renderSettingsRenderDirMode > 2 ? block.GetInt(BakeryLightmapMode) : 0;
            
            foreach (var rendererSharedMaterial in renderer.sharedMaterials)
            {
                if (!supportedMaterials.Contains(rendererSharedMaterial) || bakeryMaterials.ContainsKey(rendererSharedMaterial)) continue;

                bakeryMaterials.Add(rendererSharedMaterial, bakeryMode);
            }
        }

        return bakeryMaterials;
    }


    // tries to set the float on common property names
    private static void ToggleProperties(Material material, bool SH, bool RNM)
    {
        material.SetFloat("_BAKERY_SH", SH ? 1 : 0);
        material.SetFloat("_BAKERY_RNM", RNM ? 1 : 0);

        if (!SH && !RNM)
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

    }

    private static void ToggleKeyword(Material material, string keyword, bool enable)
    {
        if (enable) material.EnableKeyword(keyword);
        else material.DisableKeyword(keyword);
    }

    private static void SetFromPropertyBlocks()
    {
        DisableAll();
        var bakeryMaterials = FindSupportedObjects(true);

        foreach (var material in bakeryMaterials.Keys)
        {
            ToggleKeyword(material, "BAKERY_SH", bakeryMaterials[material] == 3);
            ToggleKeyword(material, "BAKERY_RNM", bakeryMaterials[material] == 2);

            ToggleProperties(material, bakeryMaterials[material] == 3, bakeryMaterials[material] == 2);

            if (bakeryMaterials[material] == 0)
            {
                material.DisableKeyword("BAKERY_SH");
                material.DisableKeyword("BAKERY_RNM");
            }
        }
    }

    private static void AutoSwitch(object sender, EventArgs e)
    {
        if (!EditorPrefs.GetBool(AutoSwitchPref, false)) return;
        
        SetFromPropertyBlocks();
        Debug.Log("<color=fuchsia>[BakeryModeUtility]</color> Bakery Keywords Applied"); 
    }
}
#endif