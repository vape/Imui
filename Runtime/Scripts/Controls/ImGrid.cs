using Imui.Controls.Styling;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public struct ImGridState
    {
        public Vector2 Origin;
        public Vector2 CellSize;
        public Vector2 Spacing;
        public int Columns;
        public int X;
        public int Y;
    }
    
    public static class ImGrid
    {
        public static ImGridState BeginGrid(this ImGui gui, int columns, float cellHeight = 0)
        {
            var width = gui.GetLayoutWidth();
            var spacing = GetDefaultSpacing();
            var cellWidth = (width + spacing.x) / columns - spacing.x;
            cellHeight = cellHeight <= 0 ? cellWidth : cellHeight;

            return BeginGrid(gui, new Vector2(cellWidth, cellHeight));
        }
        
        public static ImGridState BeginGrid(this ImGui gui, Vector2 cellSize)
        {
            return BeginGrid(gui, cellSize, new Vector2(ImTheme.Active.Controls.InnerSpacing, ImTheme.Active.Controls.InnerSpacing));
        }
        
        public static ImGridState BeginGrid(this ImGui gui, Vector2 cellSize, Vector2 spacing)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            
            var width = gui.Layout.GetAvailableWidth();
            var columns = Mathf.Max(1, (width + spacing.x) / (cellSize.x + spacing.x));
            var state = new ImGridState()
            {
                Origin = ImLayout.GetNextPosition(in frame, 0),
                CellSize = cellSize,
                Columns = (int)columns,
                Spacing = spacing
            };

            return state;
        }

        public static void EndGrid(this ImGui gui, in ImGridState state)
        {
            var width = state.Columns * state.CellSize.x + (state.Columns - 1) * state.Spacing.x;
            var height = (state.Y + 1) * state.CellSize.y + state.Y * state.Spacing.y;
            gui.Layout.AddRect(width, height);
        }

        public static ImRect GridNextCell(this ImGui gui, ref ImGridState state)
        {
            if (state.X >= state.Columns)
            {
                state.X = 0;
                state.Y++;
            }

            var x = state.Origin.x + state.X * (state.CellSize.x + state.Spacing.x);
            var y = state.Origin.y - state.Y * (state.CellSize.y + state.Spacing.y);
            
            state.X++;

            return new ImRect(x, y - state.CellSize.y, state.CellSize.x, state.CellSize.y);
        }

        public static Vector2 GetDefaultSpacing()
        {
            return new Vector2(ImTheme.Active.Controls.InnerSpacing, ImTheme.Active.Controls.InnerSpacing);
        }
    }
}