using System.Runtime.CompilerServices;
using Imui.Core;
using Imui.Style;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Imui.Controls
{
    public static class ImDrawingUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Box(this ImGui gui, ImRect rect, in ImStyleBox style)
        {
            gui.Canvas.RectWithOutline(rect, style.BackColor, style.BorderColor, style.BorderThickness, style.BorderRadius);
        }

        public static Vector2Int AsInt(this Vector2 vec2)
        {
            return new Vector2Int((int)vec2.x, (int)vec2.y);
        }
    }
}