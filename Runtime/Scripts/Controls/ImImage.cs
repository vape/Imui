using System;
using Imui.Core;
using Imui.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace Imui.Controls
{
    public static class ImImage
    {
        private static readonly Color32 White = new Color32(255, 255, 255, 255);

        public static ImRect AddRect(ImGui gui, Texture texture, ImSize size)
        {
            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(texture.width, texture.height)
            };
        }

        public static void Image(this ImGui gui, Texture texture, ImSize size = default, bool preserveAspect = false)
        {
            Image(gui, texture, AddRect(gui, texture, size), preserveAspect);
        }

        public static void Image(this ImGui gui, Texture texture, ImRect rect, bool preserveAspect = false)
        {
            if (gui.Canvas.Cull(rect) || !texture)
            {
                return;
            }

            if (preserveAspect)
            {
                rect = rect.WithAspect(texture.width / (float)texture.height);
            }

            var scaleOffset = gui.Canvas.GetTexScaleOffset();

            gui.Canvas.PushTexture(texture);
            gui.Canvas.SetTexScaleOffset(new Vector4(1, 1, 0, 0));
            gui.Canvas.Rect(rect, White);
            gui.Canvas.SetTexScaleOffset(scaleOffset);
            gui.Canvas.PopTexture();
        }

        public static void Image(this ImGui gui, Sprite sprite, ImRect rect, bool preserveAspect = false)
        {
            if (gui.Canvas.Cull(rect) || !sprite.texture)
            {
                return;
            }
            
            if (preserveAspect)
            {
                rect = rect.WithAspect(sprite.rect.width / sprite.rect.height);
            }
            
            gui.Canvas.PushTexture(sprite.texture);
            
            if (sprite.packed && sprite.packingMode == SpritePackingMode.Tight)
            {
                DrawSpriteMesh(gui, sprite, rect);
            }
            else
            {
                var textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
                var textureRect = sprite.textureRect;
                var textureRectOffset = sprite.textureRectOffset;
                var spriteRect = sprite.rect;
                var scaleOffset = new Vector4(textureRect.width / textureSize.x, textureRect.height / textureSize.y, textureRect.x / textureSize.x, textureRect.y / textureSize.y);
                var scale = new Vector2(1.0f / (spriteRect.width / rect.W), 1.0f / (spriteRect.height / rect.H));
                var posOffset = scale * textureRectOffset;
                var sizeDelta = scale * (spriteRect.size - textureRect.size);
                
                rect.Position += posOffset;
                rect.Size -= sizeDelta;
                
                var prevScaleOffset = gui.Canvas.GetTexScaleOffset();
                gui.Canvas.SetTexScaleOffset(scaleOffset);
                gui.Canvas.Rect(rect, White);
                gui.Canvas.SetTexScaleOffset(prevScaleOffset);
            }
            
            gui.Canvas.PopTexture();
        }

        // TODO (artem-s): at some point, should be moved to ImMeshDrawer
        private static void DrawSpriteMesh(ImGui gui, Sprite sprite, ImRect rect)
        {
            var vertices = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
            var indices = sprite.GetIndices();
            var uvs = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0);

#if UNITY_EDITOR
            if (vertices.Length > 0)
            {
                try
                {
                    _ = vertices[0];
                }
                catch (ObjectDisposedException)
                {
                    // (artem-s): no other way to check whether underlying array is disposed, and that happens sometimes
                    // when drawing in editor mode and using atlases
                    return;
                }
            }
#endif
            
            var buffer = gui.MeshDrawer.buffer;
            var offset = rect.Position + (rect.Size * (sprite.pivot / sprite.rect.size));
            var bounds = sprite.bounds.size;
            
            buffer.EnsureVerticesCapacity(buffer.VerticesCount + vertices.Length);
            buffer.EnsureIndicesCapacity(buffer.IndicesCount + indices.Length);

            for (int i = 0; i < vertices.Length; ++i)
            {
                var vertexPosition = vertices[i];
                
                ref var v = ref buffer.Vertices[buffer.VerticesCount + i];
                v.Position.x = offset.x + (vertexPosition.x / bounds.x) * rect.W;
                v.Position.y = offset.y + (vertexPosition.y / bounds.y) * rect.H;
                v.Position.z = gui.MeshDrawer.Depth;
                v.UV = uvs[i];
                v.Color = White;
                v.Atlas = gui.MeshDrawer.Atlas;
            }

            for (int i = 0; i < indices.Length; ++i)
            {
                buffer.Indices[buffer.IndicesCount + i] = indices[i] + buffer.VerticesCount;
            }
            
            buffer.AddVertices(vertices.Length);
            buffer.AddIndices(indices.Length);
        }
    }
}