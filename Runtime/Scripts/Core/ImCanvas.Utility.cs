using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public partial class ImCanvas
    {
        public int GetCurrentOrder() => GetActiveMeshSettings().Order;
        
        public void PopTexture() => PopMeshSettings();
        public void PushTexture(Texture texture)
        {
            var prop = GetActiveMeshSettings();
            prop.MainTex = texture;
            PushMeshSettings(in prop);
        }
        
        public void PopOrder() => PopMeshSettings();
        public void PushOrder(int order)
        {
            var prop = GetActiveMeshSettings();
            prop.Order = order;
            PushMeshSettings(in prop);
        }

        public void PopMaterial() => PopMeshSettings();
        public void PushMaterial(Material mat)
        {
            var prop = GetActiveMeshSettings();
            prop.Material = mat;
            PushMeshSettings(in prop);
        }

        public void PopClipRect() => PopMeshSettings();
        public void PushClipRect(ImRect rect)
        {
            var prop = GetActiveMeshSettings();
            var clipRect = (Rect)rect;
            if (prop.ClipRect.Enabled)
            {
                clipRect = prop.ClipRect.Rect.Intersection(clipRect);
            }
            prop.ClipRect.Enabled = true;
            prop.ClipRect.Rect = clipRect;
            PushMeshSettings(in prop);
        }
        public void PushNoClipRect()
        {
            var prop = GetActiveMeshSettings();
            prop.ClipRect.Enabled = false;
            PushMeshSettings(in prop);
        }

        public void PopRectMask() => PopMeshSettings();
        public void PushRectMask(ImRect rect, float radius)
        {
            var prop = GetActiveMeshSettings();
            prop.MaskRect.Enabled = true;
            prop.MaskRect.Rect = (Rect)rect;
            prop.MaskRect.Radius = radius;
            PushMeshSettings(in prop);
        }

        public void PushNoRectMask()
        {
            var prop = GetActiveMeshSettings();
            prop.MaskRect.Enabled = false;
            PushMeshSettings(prop);
        }
    }
}