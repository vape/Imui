using UnityEngine;

namespace Imui.Rendering
{
    public struct MeshClipRect
    {
        public bool Enabled;
        public Rect Rect;
    }
    
    public struct MeshMaskRect
    {
        public bool Enabled;
        public Rect Rect;
        public float Radius;
    }
    
    public struct MeshData
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
        public MeshClipRect ClipRect;
        public MeshMaskRect MaskRect;
        
        public void ClearOptions()
        {
            MainTex = null;
            FontTex = null;
            Material = null;
            Topology = default;
            Order = 0;
            ClipRect = default;
            MaskRect = default;
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