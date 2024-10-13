using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImWindowFlag : ulong
    {
        None = 0,
        NoResize = 1 << 0,
        NoDrag = 1 << 1,
        NoTitleBar = 1 << 2,
        NoCloseButton = 1 << 3
    }

    public class ImWindowManager
    {
        private const int DRAWING_STACK_CAPACITY = 8;
        private const int WINDOWS_CAPACITY = 32;

        private ImDynamicArray<uint> drawingStack = new(DRAWING_STACK_CAPACITY);
        private ImDynamicArray<ImWindowState> windows = new(WINDOWS_CAPACITY);
        private Vector2 screenSize;

        public ref ImWindowState BeginWindow(uint id, string title, float width, float height, ImWindowFlag flags)
        {
            drawingStack.Push(id);

            var index = TryFindWindow(id);
            if (index >= 0)
            {
                ref var window = ref windows.Array[index];
                window.Flags = flags;
                return ref window;
            }

            var rect = GetNewWindowRect(width, height);

            windows.Add(new ImWindowState
            {
                Id = id,
                Order = windows.Count,
                Title = title,
                Rect = rect,
                NextRect = rect,
                Flags = flags
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

        public ImRect GetCurrentWindowRect()
        {
            if (!IsDrawingWindow())
            {
                return default;
            }

            ref var state = ref GetWindowState(drawingStack.Peek());
            return state.Rect;
        }

        public void SetCurrentWindowRect(ImRect rect)
        {
            if (!IsDrawingWindow())
            {
                return;
            }

            ref var state = ref GetWindowState(drawingStack.Peek());
            state.Rect = rect;
        }

        public bool Raycast(float x, float y)
        {
            for (int i = 0; i < windows.Count; ++i)
            {
                if (windows.Array[i].Rect.Contains(x, y))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetScreenSize(Vector2 screenSize)
        {
            this.screenSize = screenSize;
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

        private ImRect GetNewWindowRect(float width, float height)
        {
            var size = new Vector2(width, height);
            var position = new Vector2((screenSize.x - width) / 2f, (screenSize.y - height) / 2f);

            return new ImRect(position, size);
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
    }
}