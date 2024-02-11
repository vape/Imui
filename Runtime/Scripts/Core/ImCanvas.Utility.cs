using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public partial class ImCanvas
    {
        public void PopOrder() => PopMeshSettings();
        public void PushOrder(int order)
        {
            var prop = GetActiveMeshSettings();
            prop.Order = order;
            PushMeshSettings(ref prop);
        }

        public void PopMaterial() => PopMeshSettings();
        public void PushMaterial(Material mat)
        {
            var prop = GetActiveMeshSettings();
            prop.Material = mat;
            PushMeshSettings(ref prop);
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
            PushMeshSettings(ref prop);
        }
        public void PushNoClipRect()
        {
            var prop = GetActiveMeshSettings();
            prop.ClipRect.Enabled = false;
            PushMeshSettings(ref prop);
        }

        public void PopRectMask() => PopMeshSettings();
        public void PushRectMask(ImRect rect, float radius)
        {
            var prop = GetActiveMeshSettings();
            prop.MaskRect.Enabled = true;
            prop.MaskRect.Rect = (Rect)rect;
            prop.MaskRect.Radius = radius;
            PushMeshSettings(ref prop);
        }
    }
}