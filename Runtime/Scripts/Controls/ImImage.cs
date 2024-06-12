using Imui.Core;
using Imui.Controls.Styling;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImImage
    {
        private static Vector4 ScaleOffset = new Vector4(1, 1, 0, 0);
        
        public static void Image(this ImGui gui, ImRect rect, Texture texture, bool preserveAspect = false)
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
            gui.Canvas.Rect(rect, ImColors.White, ScaleOffset);
            gui.Canvas.PopTexture();
        }
    }
}