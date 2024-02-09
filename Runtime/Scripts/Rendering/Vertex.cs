using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public static readonly VertexAttributeDescriptor[] VertexAttributes = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3)
        };

        public Vector3 Position;
        public Color32 Color;
        public Vector3 UV;

        public Vertex(Vector3 position, Color32 color, Vector3 uv)
        {
            Position = position;
            Color = color;
            UV = uv;
        }
        
        public Vertex(Vertex vertex)
        {
            Position = vertex.Position;
            Color = vertex.Color;
            UV = vertex.UV;
        }
    }
}