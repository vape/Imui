using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImWindowFlag: ulong
    {
        None = 0,
        NoResizing = 1 << 0,
        NoMoving = 1 << 1,
        NoTitleBar = 1 << 2,
        NoCloseButton = 1 << 3,
        HasMenuBar = 1 << 4,
        NoMovingAndResizing = NoMoving | NoResizing
    }

    public class ImWindowManager
    {
        private const int DRAWING_STACK_CAPACITY = 8;
        private const int WINDOWS_CAPACITY = 32;

        internal ImDynamicArray<uint> drawingStack = new(DRAWING_STACK_CAPACITY);
        internal ImDynamicArray<ImWindowState> windows = new(WINDOWS_CAPACITY);

        public ref ImWindowState BeginWindow(uint id, string title, ImRect initialRect, ImWindowFlag flags)
        {
            drawingStack.Push(id);

            var index = TryFindWindow(id);
            if (index >= 0)
            {
                ref var window = ref windows.Array[index];

                var wasVisible = window.Visible;
                
                window.Visible = true;
                window.NextVisible = true;
                
                if (!wasVisible)
                {
                    MoveToTop(index);
                    drawingStack.Pop();
                    return ref BeginWindow(id, title, initialRect, flags);
                }
                
                window.Flags = flags;

                if ((flags & ImWindowFlag.NoMovingAndResizing) == ImWindowFlag.NoMovingAndResizing)
                {
                    window.Rect = initialRect;
                }

                return ref window;
            }

            windows.Add(new ImWindowState
            {
                Id = id,
                Order = windows.Count,
                Title = title,
                Rect = initialRect,
                Flags = flags,
                Visible = true,
                NextVisible = true
            });

            return ref windows.Array[windows.Count - 1];
        }

        public uint EndWindow()
        {
            return drawingStack.Pop();
        }

        public bool IsDrawingWindow()
        {
            return drawingStack.Count > 0;
        }

        public bool TryGetDrawingWindowId(out uint id)
        {
            if (drawingStack.Count == 0)
            {
                id = default;
                return false;
            }

            id = drawingStack.Peek();
            return true;
        }

        public bool Raycast(float x, float y)
        {
            for (int i = 0; i < windows.Count; ++i)
            {
                ref var state = ref windows.Array[i];

                if (state.Visible && state.Rect.Contains(x, y))
                {
                    return true;
                }
            }

            return false;
        }

        public ref ImWindowState GetWindowState(uint id)
        {
            return ref windows.Array[TryFindWindow(id)];
        }

        public void RequestFocus(uint id)
        {
            var index = TryFindWindow(id);
            if (index < 0)
            {
                return;
            }

            MoveToTop(index);
        }
        
        internal void HandleFrameEnded()
        {
            for (int i = 0; i < windows.Count; ++i)
            {
                ref var state = ref windows.Array[i];

                state.Visible = state.NextVisible;
                state.NextVisible = false;
            }
        }

        private void MoveToTop(int index)
        {
            var state = windows.Array[index];
            windows.RemoveAt(index);
            windows.Add(state);

            for (int i = 0; i < windows.Count; ++i)
            {
                windows.Array[i].Order = i;
            }
        }

        internal int TryFindWindow(uint id)
        {
            for (int i = 0; i < windows.Count; ++i)
            {
                if (windows.Array[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public struct ImWindowState
    {
        public uint Id;
        public int Order;
        public string Title;
        public ImRect Rect;
        public ImWindowFlag Flags;
        public bool Visible;
        public bool NextVisible;
    }
}