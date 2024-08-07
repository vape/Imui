using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace Imui.Rendering
{
    // TODO (artem-s); try burst for optimizations
    public class ImMeshDrawer
    {
        public const float MAIN_TEX_ID = 0.0f;
        public const float FONT_TEX_ID = 1.0f;
        
        private const float PI = Mathf.PI;
        private const float HALF_PI = PI / 2;
        
        public float Depth;
        public float Atlas;
        public Color32 Color;
        public Vector4 ScaleOffset;

        internal readonly ImMeshBuffer buffer;
        
        public ImMeshDrawer(ImMeshBuffer buffer)
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

        public ref ImMeshData GetMesh()
        {
            return ref buffer.Meshes[buffer.MeshesCount - 1];
        }

        public void AddLine(ReadOnlySpan<Vector2> path, bool closed, float thickness, float outerScale, float innerScale)
        {
            Profiler.BeginSample("MeshDrawer.AddLine");
            
            var outerThickness = thickness * outerScale;
            var innerThickness = thickness * innerScale;
            
            var ic = buffer.IndicesCount;
            var vc = buffer.VerticesCount;

            var generatedIndices = (path.Length - (closed ? 0 : 1)) * 6;
            var generatedVertices = ((path.Length - (closed ? 0 : 1)) * 4);
            
            buffer.EnsureIndicesCapacity(ic + generatedIndices);
            buffer.EnsureVerticesCapacity(vc + generatedVertices);

            float normalX;
            float normalY;
            
            for (int i = 0; i < path.Length - 1; ++i)
            {
                var a = path[i];
                var b = path[i + 1];
                
                var abx = b.x - a.x;
                var aby = b.y - a.y;
                var abm = Mathf.Sqrt(abx * abx + aby * aby);
                if (abm != 0)
                {
                    normalX = -aby / abm;
                    normalY = abx / abm;
                }
                else
                {
                    normalX = 0;
                    normalY = 0;
                }
                
                ref var v0 = ref buffer.Vertices[vc + 0];
                v0.Position.x = a.x + normalX * -1 * outerThickness;
                v0.Position.y = a.y + normalY * -1 * outerThickness;
                v0.Position.z = Depth;
                v0.Color = Color;
                v0.UV.x = ScaleOffset.z;
                v0.UV.y = ScaleOffset.w;
                v0.Atlas = Atlas;

                ref var v1 = ref buffer.Vertices[vc + 1];
                v1.Position.x = a.x + normalX * innerThickness;
                v1.Position.y = a.y + normalY * innerThickness;
                v1.Position.z = Depth;
                v1.Color = Color;
                v1.UV = default;
                v1.Atlas = Atlas;
                
                ref var v2 = ref buffer.Vertices[vc + 2];
                v2.Position.x = b.x + normalX * -1 * outerThickness;
                v2.Position.y = b.y + normalY * -1 * outerThickness;
                v2.Position.z = Depth;
                v2.Color = Color;
                v2.UV.x = ScaleOffset.z;
                v2.UV.y = ScaleOffset.w;
                v2.Atlas = Atlas;

                ref var v3 = ref buffer.Vertices[vc + 3];
                v3.Position.x = b.x + normalX * innerThickness;
                v3.Position.y = b.y + normalY * innerThickness;
                v3.Position.z = Depth;
                v3.Color = Color;
                v3.UV.x = ScaleOffset.z;
                v3.UV.y = ScaleOffset.w;
                v3.Atlas = Atlas;

                buffer.Indices[ic + 0] = vc + 0;
                buffer.Indices[ic + 1] = vc + 1;
                buffer.Indices[ic + 2] = vc + 3;
                buffer.Indices[ic + 3] = vc + 3;
                buffer.Indices[ic + 4] = vc + 2;
                buffer.Indices[ic + 5] = vc + 0;

                ic += 6;
                vc += 4;
            }

            if (closed)
            {
                var a = path[^1];
                var b = path[0];
                
                var abx = b.x - a.x;
                var aby = b.y - a.y;
                var abm = Mathf.Sqrt(abx * abx + aby * aby);
                if (abm != 0)
                {
                    normalX = -aby / abm;
                    normalY = abx / abm;
                }
                else
                {
                    normalX = 0;
                    normalY = 0;
                }
                
                ref var v0 = ref buffer.Vertices[vc + 0];
                v0.Position.x = a.x + normalX * -1 * outerThickness;
                v0.Position.y = a.y + normalY * -1 * outerThickness;
                v0.Position.z = Depth;
                v0.Color = Color;
                v0.UV.x = ScaleOffset.z;
                v0.UV.y = ScaleOffset.w;
                v0.Atlas = Atlas;

                ref var v1 = ref buffer.Vertices[vc + 1];
                v1.Position.x = a.x + normalX * innerThickness;
                v1.Position.y = a.y + normalY * innerThickness;
                v1.Position.z = Depth;
                v1.Color = Color;
                v1.UV = default;
                v1.Atlas = Atlas;
                
                ref var v2 = ref buffer.Vertices[vc + 2];
                v2.Position.x = b.x + normalX * -1 * outerThickness;
                v2.Position.y = b.y + normalY * -1 * outerThickness;
                v2.Position.z = Depth;
                v2.Color = Color;
                v2.UV.x = ScaleOffset.z;
                v2.UV.y = ScaleOffset.w;
                v2.Atlas = Atlas;

                ref var v3 = ref buffer.Vertices[vc + 3];
                v3.Position.x = b.x + normalX * innerThickness;
                v3.Position.y = b.y + normalY * innerThickness;
                v3.Position.z = Depth;
                v3.Color = Color;
                v3.UV.x = ScaleOffset.z;
                v3.UV.y = ScaleOffset.w;
                v3.Atlas = Atlas;

                buffer.Indices[ic + 0] = vc + 0;
                buffer.Indices[ic + 1] = vc + 1;
                buffer.Indices[ic + 2] = vc + 3;
                buffer.Indices[ic + 3] = vc + 3;
                buffer.Indices[ic + 4] = vc + 2;
                buffer.Indices[ic + 5] = vc + 0;
            }

            buffer.AddIndices(generatedIndices);
            buffer.AddVertices(generatedVertices);
            
            Profiler.EndSample();
        }
        
        // TODO (artem-s): add proper texturing
        public void AddLineMiter(ReadOnlySpan<Vector2> path, bool closed, float thickness, float outerScale, float innerScale)
        {
            Profiler.BeginSample("MeshDrawer.AddLineMiter");
            
            if (path.Length < 2)
            {
                return;
            }

            thickness = Mathf.Max(1.0f, thickness);

            var outerThickness = thickness * outerScale;
            var innerThickness = thickness * innerScale;
            var pointsCount = closed ? path.Length : path.Length - 1;
            
            float prevNormX;
            float prevNormY;
            
            if (closed)
            {
                var a = path[^1];
                var b = path[0];
                var c = path[1];
                
                var abx = b.x - a.x;
                var aby = b.y - a.y;
                var abm = Mathf.Sqrt(abx * abx + aby * aby);
                if (abm > 0)
                {
                    abx /= abm;
                    aby /= abm;
                }
                
                var bcx = c.x - b.x;
                var bcy = c.y - b.y;
                var bcm = Mathf.Sqrt(bcx * bcx + bcy * bcy);
                if (bcm > 0)
                {
                    bcx /= bcm;
                    bcy /= bcm;
                }

                var tx = abx + bcx;
                var ty = aby + bcy;
                var tm = Mathf.Sqrt(tx * tx + ty * ty);
                if (tm > 0)
                {
                    tx /= tm;
                    ty /= tm;
                }


                prevNormX = -ty;
                prevNormY = tx;
            }
            else
            {
                var a = path[0];
                var b = path[1];
                
                var abx = b.x - a.x;
                var aby = b.y - a.y;
                var abm = Mathf.Sqrt(abx * abx + aby * aby);
                abx /= abm;
                aby /= abm;

                prevNormX = -aby;
                prevNormY = abx;
            }
            
            var ic = buffer.IndicesCount;
            var vc = buffer.VerticesCount;

            var generatedIndices = pointsCount * 6;
            var generatedVertices = (pointsCount * 2) + 2;
            
            buffer.EnsureIndicesCapacity(ic + generatedIndices);
            buffer.EnsureVerticesCapacity(vc + generatedVertices);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = path[0].x + prevNormX * -1 * outerThickness;
            v0.Position.y = path[0].y + prevNormY * -1 * outerThickness;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.Atlas = Atlas;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = path[0].x + prevNormX * innerThickness;
            v1.Position.y = path[0].y + prevNormY * innerThickness;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV = default;
            v1.Atlas = Atlas;

            for (int i = 0; i < pointsCount; ++i)
            {
                var a = path[i];
                var b = path[(i + 1) % path.Length];

                float normalX;
                float normalY;
                float scale;

                if (i <= path.Length - 3 || closed)
                {
                    var c = path[(i + 2) % path.Length];
                    
                    var abx = b.x - a.x;
                    var aby = b.y - a.y;
                    var abm = Mathf.Sqrt(abx * abx + aby * aby);
                    if (abm > 0)
                    {
                        abx /= abm;
                        aby /= abm;
                    }
                    
                    var bcx = c.x - b.x;
                    var bcy = c.y - b.y;
                    var bcm = Mathf.Sqrt(bcx * bcx + bcy * bcy);
                    if (bcm > 0)
                    {
                        bcx /= bcm;
                        bcy /= bcm;
                    }

                    var tx = abx + bcx;
                    var ty = aby + bcy;
                    var tm = Mathf.Sqrt(tx * tx + ty * ty);
                    if (tm > 0)
                    {
                        tx /= tm;
                        ty /= tm;
                    }

                    normalX = -ty;
                    normalY = tx;
                    scale = normalX * -aby + normalY * abx;

                    if (Mathf.Abs(scale) < 0.01f)
                    {
                        normalX = -bcy;
                        normalY = bcx;
                        scale = 1.0f;
                    }
                }
                else
                {
                    var abx = b.x - a.x;
                    var aby = b.y - a.y;
                    var abm = Mathf.Sqrt(abx * abx + aby * aby);
                    abx /= abm;
                    aby /= abm;

                    normalX = -aby;
                    normalY = abx;
                    scale = 1.0f;
                }
                
                ref var v2 = ref buffer.Vertices[vc + 2];
                v2.Position.x = b.x + normalX * -1 * outerThickness / scale;
                v2.Position.y = b.y + normalY * -1 * outerThickness / scale;
                v2.Position.z = Depth;
                v2.Color = Color;
                v2.UV.x = ScaleOffset.z;
                v2.UV.y = ScaleOffset.w;
                v2.Atlas = Atlas;

                ref var v3 = ref buffer.Vertices[vc + 3];
                v3.Position.x = b.x + normalX * innerThickness / scale;
                v3.Position.y = b.y + normalY * innerThickness / scale;
                v3.Position.z = Depth;
                v3.Color = Color;
                v3.UV.x = ScaleOffset.z;
                v3.UV.y = ScaleOffset.w;
                v3.Atlas = Atlas;

                buffer.Indices[ic + 0] = vc + 0;
                buffer.Indices[ic + 1] = vc + 1;
                buffer.Indices[ic + 2] = vc + 3;
                buffer.Indices[ic + 3] = vc + 3;
                buffer.Indices[ic + 4] = vc + 2;
                buffer.Indices[ic + 5] = vc + 0;

                ic += 6;
                vc += 2;
            }

            buffer.AddIndices(generatedIndices);
            buffer.AddVertices(generatedVertices);
            
            Profiler.EndSample();
        }
        
        // TODO (artem-s): calculate proper UV values
        public void AddTriangleFan(Vector2 center, float from, float to, float radius, int segments)
        {
            ImAssert.True(segments > 0, "segments > 0");
            ImAssert.True(to > from, "to > from");
            
            Profiler.BeginSample("MeshDrawer.AddTriangleFan");
            
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 2 + segments);
            buffer.EnsureIndicesCapacity(ic + 3 * segments);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = center.x;
            v0.Position.y = center.y;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.Atlas = Atlas;
            
            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = center.x + Mathf.Cos(from) * radius;
            v1.Position.y = center.y + Mathf.Sin(from) * radius;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = ScaleOffset.w;
            v1.Atlas = Atlas;

            var angleStep = (1f / segments) * (to - from);
            for (int i = 0; i < segments; ++i)
            {
                var a = from + angleStep * (i + 1);
                var idx = vc + i + 2;
                
                ref var v2 = ref buffer.Vertices[idx];
                v2.Position.x = center.x + Mathf.Cos(a) * radius;
                v2.Position.y = center.y + Mathf.Sin(a) * radius;
                v2.Position.z = Depth;
                v2.Color = Color;
                v2.UV.x = ScaleOffset.z;
                v2.UV.y = ScaleOffset.w;
                v2.Atlas = Atlas;

                buffer.Indices[ic + (i * 3) + 0] = vc + 0;
                buffer.Indices[ic + (i * 3) + 1] = idx;
                buffer.Indices[ic + (i * 3) + 2] = idx - 1;
            }
            
            buffer.AddVertices(2 + segments);
            buffer.AddIndices(3 * segments);
            
            Profiler.EndSample();
        }
        
        // TODO (artem-s): implement texturing with proper UV values
        public void AddRoundCornersRect(Vector4 rect, float tlr, float trr, float brr, float blr, int segments)
        {
            Profiler.BeginSample("MeshDrawer.AddRoundCornersRect");
            
            var p0 = new Vector2(rect.x + blr, rect.y + blr);
            var p1 = new Vector2(rect.x + tlr, rect.y + rect.w - tlr);
            var p2 = new Vector2(rect.x + rect.z - trr, rect.y + rect.w - trr);
            var p3 = new Vector2(rect.x + rect.z - brr, rect.y + brr);

            var v0 = buffer.VerticesCount;
            AddTriangleFan(p0, PI, PI + HALF_PI, blr, segments);
            var v1 = buffer.VerticesCount;
            AddTriangleFan(p1, HALF_PI, PI, tlr, segments);
            var v2 = buffer.VerticesCount;
            AddTriangleFan(p2, 0, HALF_PI, trr, segments);
            var v3 = buffer.VerticesCount;
            AddTriangleFan(p3, PI + HALF_PI, PI * 2, brr, segments);

            var ic = buffer.IndicesCount;
            buffer.EnsureIndicesCapacity(ic + 6 * 5);
            buffer.AddIndices(6 * 5);
            
            SetQuad(ic +  0, v0, v0 + 1, v1 + segments + 1, v1);
            SetQuad(ic +  6, v1, v1 + 1, v2 + segments + 1, v2);
            SetQuad(ic + 12, v2, v2 + 1, v3 + segments + 1, v3);
            SetQuad(ic + 18, v3, v3 + 1, v0 + segments + 1, v0);
            SetQuad(ic + 24, v0, v1, v2, v3);
            
            Profiler.EndSample();
        }
        
        public void AddQuad(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Profiler.BeginSample("MeshDrawer.AddQuad");
            
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 4);
            buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = x0;
            v0.Position.y = y0;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.Atlas = Atlas;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = x1;
            v1.Position.y = y1;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.Atlas = Atlas;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = x2;
            v2.Position.y = y2;
            v2.Position.z = Depth;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.Atlas = Atlas;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = x3;
            v3.Position.y = y3;
            v3.Position.z = Depth;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.Atlas = Atlas;

            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddIndices(6);
            buffer.AddVertices(4);
            
            Profiler.EndSample();
        }

        public void AddQuad(float x, float y, float w, float h)
        {
            Profiler.BeginSample("MeshDrawer.AddQuad");
            
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + 4);
            buffer.EnsureIndicesCapacity(ic + 6);
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = x;
            v0.Position.y = y;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.Atlas = Atlas;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = x;
            v1.Position.y = y + h;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v1.Atlas = Atlas;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = x + w;
            v2.Position.y = y + h;
            v2.Position.z = Depth;
            v2.Color = Color;
            v2.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v2.UV.y = (ScaleOffset.w + ScaleOffset.y);
            v2.Atlas = Atlas;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = x + w;
            v3.Position.y = y;
            v3.Position.z = Depth;
            v3.Color = Color;
            v3.UV.x = (ScaleOffset.z + ScaleOffset.x);
            v3.UV.y = (ScaleOffset.w);
            v3.Atlas = Atlas;

            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddIndices(6);
            buffer.AddVertices(4);
            
            Profiler.EndSample();
        }

        public void AddFilledConvexMesh(ReadOnlySpan<Vector2> points)
        {
            ImAssert.True(points.Length > 2, "points.Length > 2");
            
            Profiler.BeginSample("MeshDrawer.AddFilledConvexMesh");
            
            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            buffer.EnsureVerticesCapacity(vc + points.Length * 4);
            buffer.EnsureIndicesCapacity(ic + (points.Length - 2) * 3);

            ref readonly var p0 = ref points[0];
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = p0.x;
            v0.Position.y = p0.y;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = ScaleOffset.z;
            v0.UV.y = ScaleOffset.w;
            v0.Atlas = Atlas;
            
            ref readonly var p1 = ref points[1];
            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = p1.x;
            v1.Position.y = p1.y;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = ScaleOffset.z;
            v1.UV.y = ScaleOffset.w;
            v1.Atlas = Atlas;
            
            for (int i = 2; i < points.Length; ++i)
            {
                ref readonly var p = ref points[i];
                ref var v = ref buffer.Vertices[vc + i];
                v.Position.x = p.x;
                v.Position.y = p.y;
                v.Position.z = Depth;
                v.Color = Color;
                v.UV.x = ScaleOffset.z;
                v.UV.y = ScaleOffset.w;
                v.Atlas = Atlas;

                buffer.Indices[ic + 0] = vc + i;
                buffer.Indices[ic + 1] = vc + i - 1;
                buffer.Indices[ic + 2] = vc;
                
                ic += 3;
            }

            buffer.AddVertices(points.Length * 4);
            buffer.AddIndices((points.Length - 2) * 3);
            
            Profiler.EndSample();
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