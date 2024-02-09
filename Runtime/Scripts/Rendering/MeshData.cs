using UnityEngine;

namespace Imui.Rendering
{
    public struct MeshData
    {
        public Material Material;
        public int IndicesOffset;
        public int VerticesOffset;
        public int VerticesCount;
        public int IndicesCount;
        public MeshTopology Topology;
        public int Order;
        
        public void ClearOptions()
        {
            Material = null;
            Topology = default;
            Order = 0;
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