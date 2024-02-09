using UnityEngine;

namespace Imui.Rendering
{
    public class MeshDrawer
    {
        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;
        
        public readonly MeshBuffer Buffer;

        // ReSharper disable once InconsistentNaming
        public float UVZ;
        public Color32 Color;
        public Vector4 ScaleOffset;

        public MeshDrawer()
        {
            Buffer = new MeshBuffer(INIT_MESHES_COUNT, INIT_VERTICES_COUNT, INIT_INDICES_COUNT);
        }

        public void Clear()
        {
            Buffer.Clear();
        }

        public void NextMesh()
        {
            Buffer.NextMesh();
        }

        public ref MeshData GetMesh()
        {
            return ref Buffer.Meshes[Buffer.MeshesCount - 1];
        }
        
        public void AddQuad(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3, float z)
        {
            var vc = Buffer.VerticesCount;
            var ic = Buffer.IndicesCount;
            
            Buffer.EnsureVerticesCapacity(vc + 4);
            Buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref Buffer.Vertices[vc + 0];
            v0.Position.x = x0;
            v0.Position.y = y0;
            v0.Position.z = z;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.UV.z = UVZ;

            ref var v1 = ref Buffer.Vertices[vc + 1];
            v1.Position.x = x1;
            v1.Position.y = y1;
            v1.Position.z = z;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.UV.z = UVZ;
            
            ref var v2 = ref Buffer.Vertices[vc + 2];
            v2.Position.x = x2;
            v2.Position.y = y2;
            v2.Position.z = z;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.UV.z = UVZ;

            ref var v3 = ref Buffer.Vertices[vc + 3];
            v3.Position.x = x3;
            v3.Position.y = y3;
            v3.Position.z = z;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.UV.z = UVZ;

            Buffer.Indices[ic + 0] = vc + 0;
            Buffer.Indices[ic + 1] = vc + 1;
            Buffer.Indices[ic + 2] = vc + 2;
            Buffer.Indices[ic + 3] = vc + 2;
            Buffer.Indices[ic + 4] = vc + 3;
            Buffer.Indices[ic + 5] = vc + 0;

            Buffer.AddIndices(6);
            Buffer.AddVertices(4);
        }

        public void AddQuad(float x, float y, float w, float h, float z)
        {
            var vc = Buffer.VerticesCount;
            var ic = Buffer.IndicesCount;
            
            Buffer.EnsureVerticesCapacity(vc + 4);
            Buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref Buffer.Vertices[vc + 0];
            v0.Position.x = x;
            v0.Position.y = y;
            v0.Position.z = z;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.UV.z = UVZ;

            ref var v1 = ref Buffer.Vertices[vc + 1];
            v1.Position.x = x;
            v1.Position.y = y + h;
            v1.Position.z = z;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.UV.z = UVZ;
            
            ref var v2 = ref Buffer.Vertices[vc + 2];
            v2.Position.x = x + w;
            v2.Position.y = y + h;
            v2.Position.z = z;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.UV.z = UVZ;

            ref var v3 = ref Buffer.Vertices[vc + 3];
            v3.Position.x = x + w;
            v3.Position.y = y;
            v3.Position.z = z;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.UV.z = UVZ;

            Buffer.Indices[ic + 0] = vc + 0;
            Buffer.Indices[ic + 1] = vc + 1;
            Buffer.Indices[ic + 2] = vc + 2;
            Buffer.Indices[ic + 3] = vc + 2;
            Buffer.Indices[ic + 4] = vc + 3;
            Buffer.Indices[ic + 5] = vc + 0;

            Buffer.AddIndices(6);
            Buffer.AddVertices(4);
        }
    }
}