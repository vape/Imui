using System;
using System.Runtime.CompilerServices;
using Imui.Core;
using Imui.IO.Events;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    [Flags]
    public enum ImTableColumnFlag: byte
    {
        None = 0,
        SizeIsAbsolute = 1 << 0,
        SizeSetByHost = 1 << 1,
        Resized = 1 << 2
    }

    [Flags]
    public enum ImTableFlag: byte
    {
        None = 0,
        ResizableColumns = 1 << 0,
        DisableClipping = 1 << 1
    }

    [Flags]
    public enum ImTableStateFlags: byte
    {
        None = 0,
        LayoutBuilt = 1 << 0,
        Enclosed = 1 << 1
    }

    public readonly struct ImTableRowsRange
    {
        public readonly int Min;
        public readonly int Max;

        public ImTableRowsRange(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public readonly struct ImTableColumnsRange
    {
        public readonly int Min;
        public readonly int Max;

        public ImTableColumnsRange(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public struct ImTableColumnState
    {
        public float Offset;
        public float RequiredWidth;
        public ImTableColumnFlag Flags;
    }

    public unsafe struct ImTableState
    {
        public ImTableFlag Flags;
        public ImTableStateFlags StateFlags;
        public ImTableColumnState* Columns;
        public int ColumnsCount;
        public Vector2 Position;
        public float Width;
        public float Height;
        public int CurrentRow;
        public float CurrentRowHeight;
        public int CurrentColumn;
        public float FixedRowHeight;
        public float FixedCellHeight;
        public int FixedRowsCount;
        public int SelectedColumn;
    }

    public static unsafe class ImTable
    {
        private const string COL_CONTROL_ID = "col";

        private const int DEFAULT_COL = -1;
        private const int DEFAULT_ROW = -1;

        private const float COL_MIN_WIDTH = 10;
        private const float ROW_MIN_HEIGHT = 15;
        private const float RESIZE_HANDLE_WIDTH = COL_MIN_WIDTH * 2;

        public static ref ImTableState PrepareState(this ImGui gui, int columns, ImSize size = default, ImTableFlag flags = ImTableFlag.None)
        {
            var id = gui.GetNextControlId();

            return ref PrepareState(gui, id, columns, size, flags);
        }

        public static ref ImTableState PrepareState(this ImGui gui, uint id, int columns, ImSize size = default, ImTableFlag flags = ImTableFlag.None)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            if (size.Mode == ImSizeMode.Fixed)
            {
                return ref PrepareState(gui, id, columns, gui.Layout.AddRect(size.Width, size.Height), flags);
            }

            var width = gui.GetLayoutWidth();
            var position = gui.Layout.GetNextPosition();
            var state = gui.BeginScopeUnsafe<ImTableState>(id);

            PrepareState(gui, state, id, columns, position, width, flags, ImTableStateFlags.None);

            return ref *state;
        }

        public static ref ImTableState PrepareState(this ImGui gui, uint id, int columns, ImRect rect, ImTableFlag flags = ImTableFlag.None)
        {
            gui.Layout.Push(ImAxis.Vertical, rect);
            gui.BeginScrollable();

            ref readonly var scrollableFrame = ref gui.Layout.GetFrame();
            if ((flags & ImTableFlag.DisableClipping) == 0)
            {
                gui.Canvas.PushClipRect(scrollableFrame.Bounds);
            }

            var width = gui.GetLayoutWidth();
            var position = gui.Layout.GetNextPosition();
            var state = gui.BeginScopeUnsafe<ImTableState>(id);

            PrepareState(gui, state, id, columns, position, width, flags, ImTableStateFlags.Enclosed);

            return ref *state;
        }

        public static void PrepareState(ImGui gui,
                                        ImTableState* state,
                                        uint id,
                                        int columns,
                                        Vector2 position,
                                        float width,
                                        ImTableFlag flags,
                                        ImTableStateFlags stateFlags)
        {
            columns = columns <= 0 ? 1 : columns;

            var columnsId = gui.GetControlId(COL_CONTROL_ID, id);
            state->Flags = flags;
            state->StateFlags = stateFlags;
            state->Columns = gui.Storage.GetArrayUnsafe<ImTableColumnState>(columnsId, columns);
            state->ColumnsCount = columns;
            state->Width = width;
            state->Position = position;
            state->Height = 0.0f;
            state->CurrentRowHeight = 0.0f;
            state->CurrentRow = DEFAULT_ROW;
            state->CurrentColumn = DEFAULT_COL;
            state->FixedRowHeight = 0.0f;
            state->FixedRowsCount = 0;
            state->FixedCellHeight = 0.0f;
        }

        public static void EndTable(this ImGui gui)
        {
            TableNextRow(gui);

            var state = gui.EndScopeUnsafe<ImTableState>(out var id);

            var x = state->Position.x;
            var y = state->Position.y;
            var w = state->Columns[state->ColumnsCount - 1].Offset * state->Width;
            var h = Mathf.Max(state->Height, state->FixedRowHeight * state->FixedRowsCount);

            var rect = new ImRect(x, y, w, h);

            gui.Layout.AddRect(rect.Size);

            DrawColumnSeparators(gui, state);
            DrawTableOutlineBorder(gui, state);

            if ((state->Flags & ImTableFlag.ResizableColumns) != 0)
            {
                HandleResizableColumns(gui, id, state);
            }
            else
            {
                state->SelectedColumn = -1;
            }

            if ((state->StateFlags & ImTableStateFlags.Enclosed) != 0)
            {
                if ((state->Flags & ImTableFlag.DisableClipping) == 0)
                {
                    gui.Canvas.PopClipRect();
                }

                gui.EndScrollable();
                gui.Layout.Pop();
            }
        }

        public static void TableSetColumnWidth(this ImGui gui, int column, float width)
        {
            const ImTableColumnFlag PREDEFINED_SIZE_CHANGED =
                ImTableColumnFlag.SizeSetByHost |
                ImTableColumnFlag.SizeIsAbsolute |
                ImTableColumnFlag.Resized;

            var state = gui.GetCurrentScopeUnsafe<ImTableState>();
            var col = &state->Columns[column];

            // if size is set by host and column has been resized (given its allowed), keep resized size
            if ((col->Flags & PREDEFINED_SIZE_CHANGED) == PREDEFINED_SIZE_CHANGED)
            {
                return;
            }

            col->Flags |= ImTableColumnFlag.SizeIsAbsolute | ImTableColumnFlag.SizeSetByHost;
            col->RequiredWidth = width;
        }

        public static void TableSetRowsHeight(this ImGui gui, float height)
        {
            var state = gui.GetCurrentScopeUnsafe<ImTableState>();

            state->FixedRowHeight = height;
            state->FixedCellHeight = height - gui.Style.Table.CellPadding.Vertical;
        }

        public static ImTableRowsRange TableGetVisibleRows(this ImGui gui, int rowsCount)
        {
            var state = gui.GetCurrentScopeUnsafe<ImTableState>();
            var viewport = gui.Layout.GetBoundsRect();

            if (state->FixedRowHeight == 0.0f)
            {
                return new ImTableRowsRange(0, rowsCount);
            }

            state->FixedRowsCount = rowsCount;

            var from = (int)(Mathf.Max(0.0f, state->Position.y - viewport.Top) / state->FixedRowHeight);
            var count = Mathf.CeilToInt(viewport.H / state->FixedRowHeight);

            return new ImTableRowsRange(from, Mathf.Min(rowsCount, from + count + 1));
        }

        public static ImTableColumnsRange TableGetVisibleColumns(this ImGui gui)
        {
            var state = gui.GetCurrentScopeUnsafe<ImTableState>();
            var viewport = gui.Layout.GetBoundsRect();

            viewport.Position -= state->Position;

            var min = 0;
            while (min < state->ColumnsCount - 1 && (state->Columns[min].Offset * state->Width) < viewport.X)
            {
                min++;
            }

            var count = 0;
            while ((min + count) < state->ColumnsCount - 1 && (state->Columns[min + count].Offset * state->Width) < (viewport.X + viewport.W))
            {
                count++;
            }

            return new ImTableColumnsRange(min, min + count + 1);
        }

        public static void TableNextRow(this ImGui gui)
        {
            ImProfiler.BeginSample("ImTable.TableNextRow");

            var state = gui.GetCurrentScopeUnsafe<ImTableState>();
            TableSetRow(gui, state->CurrentRow + 1, state);

            ImProfiler.EndSample();
        }

        public static void TableSetRow(this ImGui gui, int row, ref ImTableState state)
        {
            fixed (ImTableState* p = &state)
            {
                TableSetRow(gui, row, p);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TableSetRow(this ImGui gui, int row, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.TableSetRow");

            if (state->CurrentColumn >= 0)
            {
                EndColumn(gui, state);
            }

            if (state->CurrentRow != DEFAULT_ROW)
            {
                DrawRowSeparator(gui, state);
            }

            if ((state->StateFlags & ImTableStateFlags.LayoutBuilt) == 0)
            {
                CalcLayout(state);
            }

            var delta = row - (state->CurrentRow == DEFAULT_ROW ? 0 : state->CurrentRow);
            if (delta <= 0 && state->CurrentRow >= 0)
            {
                return;
            }

            // if anything has been drawn to previous row
            if (state->CurrentRowHeight != 0 && state->FixedRowHeight == 0.0f)
            {
                state->Height += state->CurrentRowHeight;
                state->CurrentRowHeight = 0.0f;

                delta -= 1;
            }

            // if we're jumping over more than one row
            if (delta > 0)
            {
                state->Height += delta * (state->FixedRowHeight == 0.0f ? ROW_MIN_HEIGHT : state->FixedRowHeight);

                delta = 0;
            }

            state->CurrentRow = row;
            state->CurrentColumn = DEFAULT_COL;

            ImProfiler.EndSample();
        }

        public static void TableNextColumn(this ImGui gui)
        {
            ImProfiler.BeginSample("ImTable.TableNextColumn");

            var state = gui.GetCurrentScopeUnsafe<ImTableState>();
            TableSetColumn(gui, state->CurrentColumn + 1, state);

            ImProfiler.EndSample();
        }

        public static void TableSetColumn(this ImGui gui, int column, ref ImTableState state)
        {
            fixed (ImTableState* s = &state)
            {
                TableSetColumn(gui, column, s);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TableSetColumn(this ImGui gui, int column, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.TableSetColumn");

            if (state->CurrentColumn >= 0)
            {
                EndColumn(gui, state);
            }

            BeginColumn(gui, column, state);

            ImProfiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BeginColumn(ImGui gui, int column, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.BeginColumn");

            if ((state->StateFlags & ImTableStateFlags.LayoutBuilt) == 0)
            {
                CalcLayout(state);
            }

            state->CurrentColumn = column;

            var p = gui.Style.Table.CellPadding;
            var x0 = state->Position.x + (state->CurrentColumn > 0 ? state->Columns[state->CurrentColumn - 1].Offset * state->Width : 0.0f);
            var x1 = state->Position.x + (state->Columns[state->CurrentColumn].Offset * state->Width);
            var y = state->Position.y - p.Top - state->Height - state->FixedCellHeight;
            var c = new ImRect(x0 + p.Left, y, x1 - x0 - p.Left - p.Right, state->FixedCellHeight);

            gui.Layout.Push(ImAxis.Vertical, c);

            ImProfiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EndColumn(ImGui gui, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.EndColumn");

            gui.Layout.Pop(out var frame);

            var height = frame.Size.y;
            height += gui.Style.Table.CellPadding.Vertical;
            state->CurrentRowHeight = state->CurrentRowHeight > height ? state->CurrentRowHeight : height;

            ImProfiler.EndSample();
        }

        private static void DrawRowSeparator(ImGui gui, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.DrawRowSeparator");

            ImRect rect;

            if ((state->StateFlags & ImTableStateFlags.Enclosed) != 0)
            {
                ref readonly var frame = ref gui.Layout.GetFrame();
                rect = frame.Bounds;
            }
            else
            {
                rect = new ImRect(
                    state->Position.x,
                    state->Position.y - state->Height,
                    state->Width * Mathf.Max(1.0f, state->Columns[state->ColumnsCount - 1].Offset),
                    state->Height);
            }

            var y = state->Position.y - state->Height;
            var x0 = rect.Left;
            var x1 = rect.Right;

            var p0 = new Vector2(x0, y);
            var p1 = new Vector2(x1, y);

            gui.Canvas.Line(p0, p1, gui.Style.Separator.Color, gui.Style.Separator.Thickness);

            ImProfiler.EndSample();
        }

        private static void DrawColumnSeparators(ImGui gui, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.DrawColumnSeparators");

            ref readonly var frame = ref gui.Layout.GetFrame();
            ref readonly var cullingBounds = ref gui.Canvas.GetCullingBounds();
            
            var count = (state->Flags & ImTableFlag.ResizableColumns) != 0 ? state->ColumnsCount : state->ColumnsCount - 1;
            var defaultThickness = gui.Canvas.GetScaledLineThickness(gui.Style.Table.BorderThickness);
            var selectedThickness = gui.Canvas.GetScaledLineThickness(gui.Style.Table.SelectedColumnThickness);
            var maxThickness = Mathf.Max(defaultThickness, selectedThickness);

            var y0 = state->Position.y - ((state->StateFlags & ImTableStateFlags.Enclosed) != 0 ? frame.Bounds.H + frame.Offset.y : state->Height);
            var y1 = state->Position.y - ((state->StateFlags & ImTableStateFlags.Enclosed) != 0 ? frame.Offset.y : 0.0f);

            for (int i = 0; i < count; ++i)
            {
                var x = state->Position.x + state->Columns[i].Offset * state->Width;
                if (x < cullingBounds.Left)
                {
                    continue;
                }

                if (x > cullingBounds.Right)
                {
                    break;
                }

                var p0 = new Vector2(x, y0);
                var p1 = new Vector2(x, y1);

                var color = state->SelectedColumn == i ? gui.Style.Table.SelectedColumnColor : gui.Style.Table.BorderColor;
                var thickness = state->SelectedColumn == i ? selectedThickness : defaultThickness;

                gui.Canvas.Line(p0, p1, color, thickness);
            }

            ImProfiler.EndSample();
        }

        private static void DrawTableOutlineBorder(ImGui gui, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.DrawTableOutlineBorder");

            ImRect rect;

            if ((state->StateFlags & ImTableStateFlags.Enclosed) != 0)
            {
                ref readonly var frame = ref gui.Layout.GetFrame();
                rect = frame.Bounds;
            }
            else
            {
                rect = new ImRect(
                    state->Position.x,
                    state->Position.y - state->Height,
                    state->Width * Mathf.Max(1.0f, state->Columns[state->ColumnsCount - 1].Offset),
                    state->Height);
            }

            var thickness = gui.Canvas.GetScaledLineThickness(gui.Style.Table.BorderThickness);
            var color = gui.Style.Table.BorderColor;

            gui.Canvas.RectOutline(rect, color, thickness);

            ImProfiler.EndSample();
        }

        private static void HandleResizableColumns(ImGui gui, uint id, ImTableState* state)
        {
            ImProfiler.BeginSample("ImTable.HandleResizableColumns");

            ref readonly var frame = ref gui.Layout.GetFrame();

            var active = gui.IsControlActive(id);
            var hovered = gui.IsControlHovered(id);
            var mousePos = gui.Input.MousePosition;

            if (!active)
            {
                state->SelectedColumn = -1;

                for (int i = 0; i < state->ColumnsCount; ++i)
                {
                    var columnPosition = state->Position.x + state->Columns[i].Offset * state->Width;
                    if (Mathf.Abs(mousePos.x - columnPosition) < RESIZE_HANDLE_WIDTH / 2.0f)
                    {
                        state->SelectedColumn = i;
                        break;
                    }
                }

                if (state->SelectedColumn < 0)
                {
                    ImProfiler.EndSample();
                    return;
                }
            }

            var x0 = state->Position.x + state->Columns[state->SelectedColumn].Offset * state->Width - RESIZE_HANDLE_WIDTH / 2.0f;
            var x1 = x0 + RESIZE_HANDLE_WIDTH;
            var y0 = state->Position.y - ((state->StateFlags & ImTableStateFlags.Enclosed) != 0 ? frame.Bounds.H + frame.Offset.y : state->Height);
            var y1 = state->Position.y - ((state->StateFlags & ImTableStateFlags.Enclosed) != 0 ? frame.Offset.y : 0.0f);
            var hr = new ImRect(x0, y0, x1 - x0, y1 - y0);

            gui.RegisterControl(id, hr);

            ref readonly var evt = ref gui.Input.MouseEvent;
            switch (evt.Type)
            {
                case ImMouseEventType.Down when evt.LeftButton && hovered:
                    gui.SetActiveControl(id, ImControlFlag.Draggable);
                    gui.Input.UseMouseEvent();
                    break;

                case ImMouseEventType.Up when active:
                    state->SelectedColumn = default;
                    gui.ResetActiveControl();
                    break;

                case ImMouseEventType.Drag when active:
                    var dt = evt.Delta.x;
                    var col = &state->Columns[state->SelectedColumn];

                    if ((col->Flags & ImTableColumnFlag.SizeIsAbsolute) != 0)
                    {
                        col->RequiredWidth = Mathf.Max(COL_MIN_WIDTH, col->RequiredWidth + dt);
                    }
                    else
                    {
                        col->RequiredWidth = Mathf.Max(COL_MIN_WIDTH / state->Width, col->RequiredWidth + dt / state->Width);
                    }

                    col->Flags |= ImTableColumnFlag.Resized;

                    gui.Input.UseMouseEvent();
                    break;
            }

            ImProfiler.EndSample();
        }

        private static void CalcLayout(ImTableState* state)
        {
            const ImTableColumnFlag CUSTOM_LAYOUT = ImTableColumnFlag.SizeSetByHost | ImTableColumnFlag.Resized;

            ImProfiler.BeginSample("ImTable.CalcLayout");

            var minWidth = COL_MIN_WIDTH / state->Width;
            var defWidth = Mathf.Max(minWidth, 1.0f / state->ColumnsCount);
            var prev = 0.0f;

            for (int i = 0; i < state->ColumnsCount; ++i)
            {
                var column = &state->Columns[i];
                var requested = defWidth;

                if ((state->Flags & ImTableFlag.ResizableColumns) == 0)
                {
                    column->Flags &= ~ImTableColumnFlag.Resized;
                }

                if ((column->Flags & ImTableColumnFlag.SizeIsAbsolute) != 0)
                {
                    requested = column->RequiredWidth / state->Width;
                }
                else if ((column->Flags & CUSTOM_LAYOUT) == 0)
                {
                    column->RequiredWidth = defWidth;
                }
                else
                {
                    requested = column->RequiredWidth;
                }

                column->Offset = prev + Mathf.Max(requested, minWidth);
                prev = column->Offset;
            }

            state->StateFlags |= ImTableStateFlags.LayoutBuilt;

            ImProfiler.EndSample();
        }
    }
}