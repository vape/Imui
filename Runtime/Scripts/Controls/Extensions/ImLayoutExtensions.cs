using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImLayoutExtensions
    {
        public static Vector2 GetAvailableSize(this ImGui gui)
        {
            return gui.Layout.GetAvailableSize();
        }
        
        public static float GetAvailableWidth(this ImGui gui)
        {
            return gui.Layout.GetAvailableWidth();
        }

        public static float GetAvailableHeight(this ImGui gui)
        {
            return gui.Layout.GetAvailableHeight();
        }
        
        public static void BeginVertical(this ImGui gui, float width = 0.0f, float height = 0.0f)
        {
            gui.Layout.Push(ImAxis.Vertical, width, height);
        }

        public static void BeginVertical(this ImGui gui, Vector2 size)
        {
            gui.Layout.Push(ImAxis.Vertical, size);
        }

        public static void EndVertical(this ImGui gui)
        {
            gui.Layout.Pop();
        }
        
        public static void BeginHorizontal(this ImGui gui, float width = 0.0f, float height = 0.0f)
        {
            gui.Layout.Push(ImAxis.Horizontal, width, height);
        }

        public static void BeginHorizontal(this ImGui gui, Vector2 size)
        {
            gui.Layout.Push(ImAxis.Horizontal, size);
        }

        public static void EndHorizontal(this ImGui gui)
        {
            gui.Layout.Pop();
        }
    }
}