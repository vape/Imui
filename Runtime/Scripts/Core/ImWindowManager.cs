using System;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Imui.Core
{
    [Flags]
    public enum ImWindowFlag : ulong
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

        private ImDynamicArray<uint> drawingStack = new(DRAWING_STACK_CAPACITY);
        private ImDynamicArray<ImWindowState> windows = new(WINDOWS_CAPACITY);
        private Vector2 screenSize;

        public ref ImWindowState BeginWindow(uint id, string title, ImRect initialRect, ImWindowFlag flags)
        {
            drawingStack.Push(id);

            var index = TryFindWindow(id);
            if (index >= 0)
            {
                ref var window = ref windows.Array[index];

                window.Flags = flags;
                window.Visible = true;
                window.NextVisible = true;

                if ((flags & ImWindowFlag.NoMovingAndResizing) == ImWindowFlag.NoMovingAndResizing)
                {
                    window.Rect = initialRect;
                    window.NextRect = initialRect;
                }

                return ref window;
            }

            windows.Add(new ImWindowState
            {
                Id = id,
                Order = windows.Count,
                Title = title,
                Rect = initialRect,
                NextRect = initialRect,
                Flags = flags,
                Visible = true,
                NextVisible = true
            });

            return ref windows.Array[windows.Count - 1];
        }

        public uint EndWindow()
        {
            var id = drawingStack.Pop();

            ref var state = ref GetWindowState(id);
            state.Rect = state.NextRect;

            return id;
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

        internal void SetScreenSize(Vector2 size)
        {
            screenSize = size;
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

        private int TryFindWindow(uint id)
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
        public ImRect NextRect;
        public ImWindowFlag Flags;
        public bool Visible;
        public bool NextVisible;
    }
}