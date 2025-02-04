using Imui.Core;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Imui.Controls
{
    public static class ImLayoutUtility
    {
        public static ImRect AddSingleRowRect(this ImGui gui, ImSize size, float minWidth = 0)
        {
            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(Mathf.Max(minWidth, gui.Layout.GetAvailableWidth()), gui.GetRowHeight())
            };
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeightFromFontSize(gui.Style.Layout.TextSize) + gui.Style.Layout.ExtraRowHeight;
        }
        
        public static float GetTextLineHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeightFromFontSize(gui.Style.Layout.TextSize);
        }

        public static float GetRowsHeightWithSpacing(this ImGui gui, int rows)
        {
            return Mathf.Max(0, gui.GetRowHeight() * rows + gui.Style.Layout.Spacing * (rows - 1));
        }
        
        public static void AddSpacingIfLayoutFrameNotEmpty(this ImGui gui)
        {
            if (!IsLayoutEmpty(gui))
            {
                gui.AddSpacing();
            }
        }

        public static bool IsLayoutEmpty(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            return frame.Size.x == 0 && frame.Size.y == 0;
        }
        
        public static void AddSpacing(this ImGui gui)
        {
            gui.Layout.AddSpace(gui.Style.Layout.Spacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }

        public static void BeginIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(gui.Style.Layout.Indent);
        }

        public static void EndIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(-gui.Style.Layout.Indent);
        }
        
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
        
        public static Vector2 GetLayoutPosition(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            
            return ImLayout.GetNextPosition(in frame, 0.0f);
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