using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable RedundantArgumentDefaultValue

namespace Imui.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImVertex
    {
        public static readonly VertexAttributeDescriptor[] VertexAttributes = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1)
        };

        public Vector3 Position;
        public Color32 Color;
        public Vector2 UV;
        public float Atlas;

        public ImVertex(Vector3 position, Color32 color, Vector2 uv, float atlas)
        {
            Position = position;
            Color = color;
            UV = uv;
            Atlas = atlas;
        }
        
        public ImVertex(ImVertex vertex)
        {
            Position = vertex.Position;
            Color = vertex.Color;
            UV = vertex.UV;
            Atlas = vertex.Atlas;
        }
    }
}