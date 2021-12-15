#if VRC_SDK_VRCSDK3
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class BakeryVolumeReceiverUdon : UdonSharpBehaviour
{
    // public int enterCounter = 0;
    
    Renderer[] _renderers;
    
    
    void Start()
    {
        
        var currentPos = transform.position;
        transform.position = new Vector3(100000, 100000, 100000);
        _renderers = GetComponentsInChildren<Renderer>();
        
        

        transform.position = currentPos;
    }
    
    public void SetPropertyBlock(Texture3D bakedTexture0, Texture3D bakedTexture1, Texture3D bakedTexture2, Texture3D bakedTexture3, Texture3D bakedMask, Vector3 volumeMin, Vector3 volumeInvSize)
    {
        if(_renderers == null) _renderers = GetComponentsInChildren<Renderer>();
        foreach (var t in _renderers)
        {
            MaterialPropertyBlock mb = new MaterialPropertyBlock();
            if (t.HasPropertyBlock())
            {
                t.GetPropertyBlock(mb);
            }

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
            
            t.SetPropertyBlock(mb);
        }

    }

}
#endif