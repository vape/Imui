using Imui.Core;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Imui.Controls
{
    public static class ImLayoutExt
    {
        public static Vector2 GetLayoutSize(this ImGui gui)
        {
            return gui.Layout.GetAvailableSize();
        }
        
        public static float GetLayoutWidth(this ImGui gui)
        {
            return gui.Layout.GetAvailableWidth();
        }

        public static float GetLayoutHeight(this ImGui gui)
        {
            return gui.Layout.GetAvailableHeight();
        }

        public static ImRect GetLayoutBounds(this ImGui gui)
        {
            return gui.Layout.GetBoundsRect();
        }

        public static Vector2 GetLayoutPosition(this ImGui gui, float height = 0.0f)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            
            return ImLayout.GetNextPosition(in frame, height);
        }

        public static ImRect AddLayoutRect(this ImGui gui, Vector2 size)
        {
            return gui.Layout.AddRect(size);
        }

        public static ImRect AddLayoutRect(this ImGui gui, float width, float height)
        {
            return gui.Layout.AddRect(width, height);
        }

        public static ImRect AddLayoutRectWithSpacing(this ImGui gui, Vector2 size)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            return gui.Layout.AddRect(size);
        }
        
        public static ImRect AddLayoutRectWithSpacing(this ImGui gui, float width, float height)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();
            
            return gui.Layout.AddRect(width, height);
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