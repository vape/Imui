using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImImage
    {
        public static ImRect GetRect(ImGui gui, Texture texture, ImSize size)
        {
            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(texture.width, texture.height)
            };
        }
        
        public static void Image(this ImGui gui, Texture texture, ImSize size = default, bool preserveAspect = false)
        {
            Image(gui, texture, GetRect(gui, texture, size), preserveAspect);
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
            
            gui.Canvas.PushTexture(texture);
            gui.Canvas.Rect(rect, ImStyleUtility.White);
            gui.Canvas.PopTexture();
        }
    }
}