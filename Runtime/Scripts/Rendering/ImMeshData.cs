using UnityEngine;

namespace Imui.Rendering
{
    public struct ImMeshClipRect
    {
        public bool Enabled;
        public Rect Rect;
    }
    
    public struct ImMeshMaskRect
    {
        public bool Enabled;
        public Rect Rect;
        public float Radius;
    }
    
    public struct ImMeshData
    {
        public Texture MainTex;
        public Texture FontTex;
        public Material Material;
        public int IndicesOffset;
        public int VerticesOffset;
        public int VerticesCount;
        public int IndicesCount;
        public MeshTopology Topology;
        public int Order;
        public ImMeshClipRect ClipRect;
        public ImMeshMaskRect MaskRect;
        public float Contrast;
        
        public void ClearOptions()
        {
            MainTex = null;
            FontTex = null;
            Material = null;
            Topology = default;
            Order = 0;
            ClipRect = default;
            MaskRect = default;
            Contrast = default;
        }
        
        public void Clear()
        {
            IndicesOffset = 0;
            VerticesOffset = 0;
            VerticesCount = 0;
            IndicesCount = 0;
            
            ClearOptions();
        }
    }
}