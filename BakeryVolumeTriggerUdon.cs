#if VRC_SDK_VRCSDK3
#if BAKERY_INCLUDED
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Configuration;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using VRC.SDKBase.Editor.BuildPipeline;
#endif


[UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(BoxCollider))]
public class BakeryVolumeTriggerUdon : UdonSharpBehaviour
{
    MaterialPropertyBlock mb;
    MaterialPropertyBlock mbEmpty;
    
    public Texture3D bakedTexture0, bakedTexture1, bakedTexture2, bakedTexture3, bakedMask;
    public Vector3 volumeMin, volumeInvSize;
    
    void Start()
    {
        mbEmpty = new MaterialPropertyBlock();
        mb = new MaterialPropertyBlock();
        
        if (bakedTexture0 != null)
        {
            mb.SetTexture("_Volume0", bakedTexture0);
            mb.SetTexture("_Volume1", bakedTexture1);
            mb.SetTexture("_Volume2", bakedTexture2);
            if (bakedTexture3 != null) mb.SetTexture("_Volume3", bakedTexture3);
        }
        
        if (bakedMask != null) mb.SetTexture("_VolumeMask", bakedMask);
        mb.SetVector("_VolumeMin", volumeMin);
        mb.SetVector("_VolumeInvSize", volumeInvSize);

    }

    private void OnTriggerEnter(Collider c)
    {
        if (c == null) return;
        var bvr = c.GetComponent<BakeryVolumeReceiverUdon>();
        if (bvr == null) return;
        
        Debug.Log(c.name + " entered " + this.name);
        bvr.SetPropertyBlock(bakedTexture0, bakedTexture1, bakedTexture2, bakedTexture3, bakedMask, volumeMin, volumeInvSize);
    }

    // private void OnTriggerStay(Collider c)
    // {
    //     if (c == null) return;
    //     var bvr = c.GetComponent<BakeryVolumeReceiverUdon>();
    //     if (bvr == null) return;
    //     
    //     Debug.Log(c.name + " entered " + this.name);
    //     bvr.SetPropertyBlock(mb);
    // }

    // private void OnTriggerExit(Collider c)
    // {
    //     if (c == null) return;
    //     var bvr = c.GetComponent<BakeryVolumeReceiverUdon>();
    //     if (bvr == null) return;
    //     
    //     Debug.Log(c.name + " left " + this.name);
    //     bvr.enterCounter--;
    //     if (bvr.enterCounter == 0) bvr.SetPropertyBlock(mbEmpty);
    // }

    
    
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    private BakeryVolume bw;

    private void Reset()
    {
        UpdateVolumeProperties();
    }
    
    private void OnValidate()
    {
        UpdateVolumeProperties();
    }
    
    
    
    public void UpdateVolumeProperties()
    {
        Debug.Log("Updating properties" +  gameObject.name);

        BakeryVolumeTriggerUdon bva = this;
        if (bva == null) return;

        bw = gameObject.GetComponent<BakeryVolume>();
        var box = gameObject.GetComponent<BoxCollider>();
        if (bw == null) return;

        box.isTrigger = true;



        bva.UpdateProxy();
    
        bva.bakedTexture0 = bw.bakedTexture0;
        bva.bakedTexture1 = bw.bakedTexture1;
        bva.bakedTexture2 = bw.bakedTexture2;
        bva.bakedTexture3 = bw.bakedTexture3;
    
        bva.bakedMask = bw.bakedMask;
        bva.volumeMin = bw.GetMin();
        bva.volumeInvSize = bw.GetInvSize();
        return;
        bva.ApplyProxyModifications();
    }
#endif
    
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(BakeryVolumeTriggerUdon))]
    public class BakeryVolumeTriggerUdonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            BakeryVolumeTriggerUdon bvt = (BakeryVolumeTriggerUdon) target;
            if (GUILayout.Button("Copy properties"))
            {
                bvt.UpdateVolumeProperties();
            }
            base.OnInspectorGUI();
        }

    }   
#endif
}
#endif
#endif