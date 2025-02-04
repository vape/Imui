using UnityEngine;

namespace Imui.Core
{
    public partial class ImCanvas
    {
        /// <summary>
        /// Removes the most recently pushed texture setting from the stack.
        /// </summary>
        public void PopTexture() => PopSettings();

        /// <summary>
        /// Pushes a texture to the settings stack.
        /// </summary>
        /// <param name="texture">The texture to use.</param>
        public void PushTexture(Texture texture)
        {
            var prop = GetActiveSettingsCopy();
            prop.MainTex = texture;
            PushSettings(in prop);
        }

        /// <summary>
        /// Retrieves the rendering order from the current settings.
        /// </summary>
        /// <returns>The rendering order value.</returns>
        public int GetOrder()
        {
            ref readonly var settings = ref GetActiveSettings();
            return settings.Order;
        }

        /// <summary>
        /// Removes the most recently pushed order setting from the stack.
        /// </summary>
        public void PopOrder() => PopSettings();

        /// <summary>
        /// Pushes a rendering order to the settings stack.
        /// </summary>
        /// <param name="order">The rendering order to apply.</param>
        public void PushOrder(int order)
        {
            var prop = GetActiveSettingsCopy();
            prop.Order = order;
            PushSettings(in prop);
        }

        /// <summary>
        /// Removes the most recently pushed material setting from the stack.
        /// </summary>
        public void PopMaterial() => PopSettings();

        /// <summary>
        /// Pushes a material to the settings stack.
        /// </summary>
        /// <param name="mat">The material to apply.</param>
        public void PushMaterial(Material mat)
        {
            var prop = GetActiveSettingsCopy();
            prop.Material = mat;
            PushSettings(in prop);
        }

        /// <summary>
        /// Removes the most recently pushed inverse color multiplier setting from the stack.
        /// </summary>
        public void PopInvColorMul() => PopSettings();

        /// <summary>
        /// Pushes a default inverse color multiplier value to the settings stack.
        /// </summary>
        public void PushDefaultInvColorMul() => PushInvColorMul(0.0f);

        /// <summary>
        /// Pushes an inverse color multiplier value to the settings stack.
        /// </summary>
        /// <param name="value">The inverse color multiplier value to apply (0..1).</param>
        public void PushInvColorMul(float value)
        {
            var prop = GetActiveSettingsCopy();
            var pref = new SettingsPref(!Mathf.Approximately(prop.InvColorMul, value));
            prop.InvColorMul = Mathf.Clamp01(value);
            PushSettings(in prop, in pref);
        }

        /// <summary>
        /// Removes the most recently pushed clipping rectangle setting from the stack.
        /// </summary>
        public void PopClipRect() => PopSettings();

        /// <summary>
        /// Pushes a cliprect to the settings stack.
        /// </summary>
        /// <param name="rect">The rectangle to apply for clipping.</param>
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

        /// <summary>
        /// Pushes a setting to disable clipping rectangles.
        /// </summary>
        public void PushNoClipRect()
        {
            var prop = GetActiveSettingsCopy();
            prop.ClipRect.Enabled = false;
            PushSettings(in prop);
        }

        /// <summary>
        /// Pushes a clipping rectangle that clips everything (and render nothing).
        /// </summary>
        public void PushClipEverything()
        {
            PushClipRect(new ImRect(0, 0, 1, 1));
        }

        /// <summary>
        /// Removes the most recently pushed rectangular mask setting from the stack.
        /// </summary>
        public void PopRectMask() => PopSettings();

        /// <summary>
        /// Pushes a rectangular mask to the settings stack, with specified corner radii.
        /// </summary>
        /// <param name="rect">The rectangle to use as a mask.</param>
        /// <param name="radius">The corner radius for the mask.</param>
        public void PushRectMask(ImRect rect, ImRectRadius radius) => PushRectMask(rect, radius.RadiusForMask());

        /// <summary>
        /// Pushes a rectangular mask to the settings stack, with a uniform corner radius.
        /// </summary>
        /// <param name="rect">The rectangle to use as a mask.</param>
        /// <param name="radius">The uniform corner radius for the mask.</param>
        public void PushRectMask(ImRect rect, float radius)
        {
            var prop = GetActiveSettingsCopy();
            prop.MaskRect.Enabled = true;
            prop.MaskRect.Rect = (Rect)rect;
            prop.MaskRect.Radius = radius;
            PushSettings(in prop);
        }

        /// <summary>
        /// Pushes a setting to disable rectangular masking.
        /// </summary>
        public void PushNoRectMask()
        {
            var prop = GetActiveSettingsCopy();
            prop.MaskRect.Enabled = false;
            PushSettings(prop);
        }

        /// <summary>
        /// Computes the intersection of two rectangles.
        /// </summary>
        /// <param name="r1">The first rectangle.</param>
        /// <param name="r2">The second rectangle.</param>
        /// <returns>The intersecting rectangle.</returns>
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