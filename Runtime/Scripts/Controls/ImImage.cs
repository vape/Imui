using Imui.Core;
using Imui.Style;
using UnityEngine;

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
            if (gui.Canvas.Cull(rect))
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
    }
}