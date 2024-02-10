using UnityEngine;

namespace Imui.Rendering
{
    public class MeshDrawer
    {
        // ReSharper disable once InconsistentNaming
        public float UVZ;
        public Color32 Color;
        public Vector4 ScaleOffset;

        private readonly MeshBuffer buffer;
        
        public MeshDrawer(MeshBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void Clear()
        {
            buffer.Clear();
        }

        public void NextMesh()
        {
            buffer.NextMesh();
        }

        public ref MeshData GetMesh()
        {
            return ref buffer.Meshes[buffer.MeshesCount - 1];
        }
        
        public void AddQuad(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3, float depth)
        {
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 4);
            buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = x0;
            v0.Position.y = y0;
            v0.Position.z = depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.UV.z = UVZ;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = x1;
            v1.Position.y = y1;
            v1.Position.z = depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.UV.z = UVZ;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = x2;
            v2.Position.y = y2;
            v2.Position.z = depth;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.UV.z = UVZ;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = x3;
            v3.Position.y = y3;
            v3.Position.z = depth;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.UV.z = UVZ;

            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddIndices(6);
            buffer.AddVertices(4);
        }

        public void AddQuad(float x, float y, float w, float h, float depth)
        {
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 4);
            buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = x;
            v0.Position.y = y;
            v0.Position.z = depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.UV.z = UVZ;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = x;
            v1.Position.y = y + h;
            v1.Position.z = depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.UV.z = UVZ;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = x + w;
            v2.Position.y = y + h;
            v2.Position.z = depth;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.UV.z = UVZ;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = x + w;
            v3.Position.y = y;
            v3.Position.z = depth;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.UV.z = UVZ;

            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddIndices(6);
            buffer.AddVertices(4);
        }
    }
}