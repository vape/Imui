using UnityEngine;

namespace Imui.Rendering
{
    public class MeshDrawer
    {
        private const float PI = Mathf.PI;
        private const float HALF_PI = PI / 2;

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
        
        public int GetSegmentsCount(float radius, float maxError = 2)
        {
            const int MIN_SEGMENTS = 1;
            const int MAX_SEGMENTS = 16;
            
            if (radius <= maxError)
            {
                return MIN_SEGMENTS;
            }

            var segments = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI / Mathf.Acos(1 - Mathf.Min(maxError, radius) / radius)), MIN_SEGMENTS, MAX_SEGMENTS);
            return ((segments + 1) / 2) * 2;
        }
        
        // TODO (artem-s): calculate proper UV values
        public void AddTriangleFan(Vector2 center, float depth, float from, float to, float radius, int segments)
        {
            ImuiAssert.True(segments > 0, "segments > 0");
            ImuiAssert.True(to > from, "to > from");
            
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 2 + segments);
            buffer.EnsureIndicesCapacity(ic + 3 * segments);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = center.x;
            v0.Position.y = center.y;
            v0.Position.z = depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.UV.z = UVZ;
            
            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = center.x + Mathf.Cos(from) * radius;
            v1.Position.y = center.y + Mathf.Sin(from) * radius;
            v1.Position.z = depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = ScaleOffset.w;
            v1.UV.z = UVZ;

            var angleStep = (1f / segments) * (to - from);
            for (int i = 0; i < segments; ++i)
            {
                var a = from + angleStep * (i + 1);
                var idx = vc + i + 2;
                
                ref var v2 = ref buffer.Vertices[idx];
                v2.Position.x = center.x + Mathf.Cos(a) * radius;
                v2.Position.y = center.y + Mathf.Sin(a) * radius;
                v2.Position.z = depth;
                v2.Color = Color;
                v2.UV.x = ScaleOffset.z;
                v2.UV.y = ScaleOffset.w;
                v2.UV.z = UVZ;

                buffer.Indices[ic + (i * 3) + 0] = vc + 0;
                buffer.Indices[ic + (i * 3) + 1] = idx;
                buffer.Indices[ic + (i * 3) + 2] = idx - 1;
            }
            
            buffer.AddVertices(2 + segments);
            buffer.AddIndices(3 * segments);
        }
        
        // TODO (artem-s): implement texturing with proper UV values
        public void AddRoundCornersRect(Vector4 rect, float depth, float tlr, float trr, float brr, float blr, int segments)
        {
            var p0 = new Vector2(rect.x + blr, rect.y + blr);
            var p1 = new Vector2(rect.x + tlr, rect.y + rect.w - tlr);
            var p2 = new Vector2(rect.x + rect.z - trr, rect.y + rect.w - trr);
            var p3 = new Vector2(rect.x + rect.z - brr, rect.y + brr);

            var v0 = buffer.VerticesCount;
            AddTriangleFan(p0, depth, PI, PI + HALF_PI, blr, segments);
            var v1 = buffer.VerticesCount;
            AddTriangleFan(p1, depth, HALF_PI, PI, tlr, segments);
            var v2 = buffer.VerticesCount;
            AddTriangleFan(p2, depth, 0, HALF_PI, trr, segments);
            var v3 = buffer.VerticesCount;
            AddTriangleFan(p3, depth, PI + HALF_PI, PI * 2, brr, segments);

            var ic = buffer.IndicesCount;
            buffer.EnsureIndicesCapacity(ic + 6 * 5);
            buffer.AddIndices(6 * 5);
            
            SetQuad(ic +  0, v0, v0 + 1, v1 + segments + 1, v1);
            SetQuad(ic +  6, v1, v1 + 1, v2 + segments + 1, v2);
            SetQuad(ic + 12, v2, v2 + 1, v3 + segments + 1, v3);
            SetQuad(ic + 18, v3, v3 + 1, v0 + segments + 1, v0);
            SetQuad(ic + 24, v0, v1, v2, v3);
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
        
        private void SetQuad(int index, int i0, int i1, int i2, int i3)
        {
            buffer.Indices[index + 0] = i0;
            buffer.Indices[index + 1] = i1;
            buffer.Indices[index + 2] = i2;
            buffer.Indices[index + 3] = i2;
            buffer.Indices[index + 4] = i3;
            buffer.Indices[index + 5] = i0;
        }
    }
}