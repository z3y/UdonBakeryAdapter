
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Configuration;
using UdonSharpEditor;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif


public class UdonBakeryAdapter : UdonSharpBehaviour
{
    public MeshRenderer[] renderers;
    public int[] bakeryLightmapMode;
    public Texture[][] rnmTexture;
    void Start()
    {
        #if UNITY_EDITOR
        return;
        #endif
        for (int i = 0; i < renderers.Length; i++)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetFloat("bakeryLightmapMode", bakeryLightmapMode[i]);
            propertyBlock.SetTexture("_RNM0", rnmTexture[i][0]);
            propertyBlock.SetTexture("_RNM1", rnmTexture[i][1]);
            propertyBlock.SetTexture("_RNM2", rnmTexture[i][2]);
            renderers[i].SetPropertyBlock(propertyBlock);
        }

        gameObject.SetActive(false);
    }

}
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public class UdonBakeryAdapterEditor : Editor
{

    //[MenuItem("Tools/Set UdonBakeryAdaper Properties")]
    public static void SetProperties()
    {
        GameObject obj = GameObject.Find("UdonBakeryAdapter");
        if (obj == null) return;
        if(PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab) PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        UdonBakeryAdapter uba = obj.GetUdonSharpComponent<UdonBakeryAdapter>();


        MeshRenderer[] renderersEditor = UnityEngine.Object.FindObjectsOfType<MeshRenderer>();

        List<MeshRenderer> renderersEditorClean = new List<MeshRenderer>();

        List<RNMTextures> rnmTexturesList = new List<RNMTextures>();

        List<int> bakeryLightmapModeEditor = new List<int>();


        for (int i = 0; i < renderersEditor.Length; i++)
        {
            MaterialPropertyBlock b = new MaterialPropertyBlock();
            renderersEditor[i].GetPropertyBlock(b);

            Texture RNM0 = b.GetTexture("_RNM0");
            Texture RNM1 = b.GetTexture("_RNM1");
            Texture RNM2 = b.GetTexture("_RNM2");
            int propertyLightmapMode = (int) b.GetFloat("bakeryLightmapMode");

            if (RNM0 && RNM1 && RNM2 && propertyLightmapMode != 0)
            {
                for (int j = 0; j < renderersEditor[i].sharedMaterials.Length; j++)
                {
                    Material mat = renderersEditor[i].sharedMaterials[j];
                    if (mat == null) continue;
                    Shader shader = mat.shader;

                    int propertyCount = ShaderUtil.GetPropertyCount(shader);

                    for (int k = propertyCount - 1; k >= 0; k--)
                    {
                        string propertyName = ShaderUtil.GetPropertyName(shader, k);
                        if (propertyName == "_BAKERY_SH" || propertyName == "_BAKERY_RNM")
                        {
                            if (mat.GetFloat("_BAKERY_SH") == 1 || mat.GetFloat("_BAKERY_RNM") == 1)
                            {
                                goto AddProperty;
                            }
                        }
                    }
                }

                continue;

                AddProperty:
                RNMTextures textures = new RNMTextures
                {
                    RNM0 = RNM0,
                    RNM1 = RNM1,
                    RNM2 = RNM2
                };
                rnmTexturesList.Add(textures);
                renderersEditorClean.Add(renderersEditor[i]);
                bakeryLightmapModeEditor.Add(propertyLightmapMode);
            }

        }

        MeshRenderer[] r = renderersEditorClean.ToArray();

        Texture[][] bakeryTextures = new Texture[r.Length][];

        for (int i = 0; i < r.Length; i++)
        {

            RNMTextures rnmTextures = rnmTexturesList[i];
            Texture[] textures = new Texture[3];
            textures[0] = rnmTextures.RNM0;
            textures[1] = rnmTextures.RNM1;
            textures[2] = rnmTextures.RNM2;
            bakeryTextures[i] = textures;
        }

        uba.UpdateProxy();
        uba.renderers = r;
        uba.rnmTexture = bakeryTextures;
        uba.bakeryLightmapMode = bakeryLightmapModeEditor.ToArray();
        uba.ApplyProxyModifications();
    }

    private struct RNMTextures
    {
        public Texture RNM0;
        public Texture RNM1;
        public Texture RNM2;
    }

}

public class SetUdonBakeryAdapterProperties : IVRCSDKBuildRequestedCallback
{
    public int callbackOrder => -2;

    bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        UdonBakeryAdapterEditor.SetProperties();
        return true;
    }
}
#endif
