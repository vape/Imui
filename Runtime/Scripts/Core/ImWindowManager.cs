using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public class ImWindowManager
    {
        private const int WINDOWS_CAPACITY = 32;

        private const int DEFAULT_WIDTH = 500;
        private const int DEFAULT_HEIGHT = 500;
        private const int DEFAULT_X_OFFSET = 30;
        private const int DEFAULT_Y_OFFSET = 30;

        private DynamicArray<ImWindowState> windows = new(WINDOWS_CAPACITY);
        
        public ref ImWindowState RegisterWindow(uint id, string title)
        {
            var index = TryFindWindow(id);
            if (index >= 0)
            {
                return ref windows.Array[index];
            }

            windows.Add(new ImWindowState
            {
                Id = id,
                Order = windows.Count,
                Title = title,
                Rect = GetWindowRect()
            });
            
            return ref windows.Array[windows.Count - 1];
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

        private ImRect GetWindowRect()
        {
            var size = new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT);

            var position = default(Vector2);
            if (windows.Count > 0)
            {
                ref var last = ref windows.Array[windows.Count - 1];
                position = new Vector2(last.Rect.X + DEFAULT_X_OFFSET, last.Rect.Y + DEFAULT_Y_OFFSET);
            }

            return new ImRect(position, size);
        }
    }

    public struct ImWindowState
    {
        public uint Id;
        public int Order;
        public string Title;
        public ImRect Rect;
    }
}