using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public partial class ImCanvas
    {
        public void PopTexture() => PopSettings();
        public void PushTexture(Texture texture)
        {
            var prop = GetActiveSettingsCopy();
            prop.MainTex = texture;
            PushSettings(in prop);
        }

        public int GetOrder()
        {
            ref readonly var settings = ref GetActiveSettings();
            return settings.Order;
        }
        
        public void PopOrder() => PopSettings();
        public void PushOrder(int order)
        {
            var prop = GetActiveSettingsCopy();
            prop.Order = order;
            PushSettings(in prop);
        }

        public void PopMaterial() => PopSettings();
        public void PushMaterial(Material mat)
        {
            var prop = GetActiveSettingsCopy();
            prop.Material = mat;
            PushSettings(in prop);
        }

        public void PopClipRect() => PopSettings();
        public void PushClipRect(ImRect rect)
        {
            var prop = GetActiveSettingsCopy();
            var clipRect = (Rect)rect;
            if (prop.ClipRect.Enabled)
            {
                clipRect = GetIntersectingRect(prop.ClipRect.Rect, clipRect);
            }
            prop.ClipRect.Enabled = true;
            prop.ClipRect.Rect = clipRect;
            PushSettings(in prop);
        }
        public void PushNoClipRect()
        {
            var prop = GetActiveSettingsCopy();
            prop.ClipRect.Enabled = false;
            PushSettings(in prop);
        }

        public void PopRectMask() => PopSettings();
        // TODO (artem-s): probably should implement masking with different radii, if I'm making this API...
        public void PushRectMask(ImRect rect, ImRectRadius radius) => PushRectMask(rect, radius.RadiusForMask());
        public void PushRectMask(ImRect rect, float radius)
        {
            var prop = GetActiveSettingsCopy();
            prop.MaskRect.Enabled = true;
            prop.MaskRect.Rect = (Rect)rect;
            prop.MaskRect.Radius = radius;
            PushSettings(in prop);
        }

        public void PushNoRectMask()
        {
            var prop = GetActiveSettingsCopy();
            prop.MaskRect.Enabled = false;
            PushSettings(prop);
        }

        private static Rect GetIntersectingRect(in Rect r1, in Rect r2)
        {
            var x1 = Mathf.Max(r1.x, r2.x);
            var y1 = Mathf.Max(r1.y, r2.y);
            var x2 = Mathf.Min(r1.x + r1.width, r2.x + r2.width);
            var y2 = Mathf.Min(r1.y + r1.height, r2.y + r2.height);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}