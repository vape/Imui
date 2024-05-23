using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImWindowFlag : ulong
    {
        None            = 0,
        DisableResize   = 1 << 0,
        DisableMoving   = 1 << 1,
        DisableTitleBar = 1 << 2
    }
    
    public class ImWindowManager
    {
        private const int WINDOWS_CAPACITY = 32;

        public const int DEFAULT_WIDTH = 500;
        public const int DEFAULT_HEIGHT = 500;

        private DynamicArray<ImWindowState> windows = new(WINDOWS_CAPACITY);
        private Rect screenRect;
        
        public ref ImWindowState RegisterWindow(uint id, string title, float width = DEFAULT_WIDTH, float height = DEFAULT_HEIGHT, ImWindowFlag flags = ImWindowFlag.None)
        {
            var index = TryFindWindow(id);
            if (index >= 0)
            {
                ref var window = ref windows.Array[index];
                window.Flags = flags;
                return ref window;
            }

            windows.Add(new ImWindowState
            {
                Id = id,
                Order = windows.Count,
                Title = title,
                Rect = GetWindowRect(width, height),
                Flags = flags
            });
            
            return ref windows.Array[windows.Count - 1];
        }

        public void SetScreenRect(Rect screenRect)
        {
            this.screenRect = screenRect;
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

        private ImRect GetWindowRect(float width, float height)
        {
            var size = new Vector2(width, height);
            var position = new Vector2((screenRect.width - width) / 2f, (screenRect.height - height) / 2f);

            return new ImRect(position, size);
        }
    }

    public struct ImWindowState
    {
        public uint Id;
        public int Order;
        public string Title;
        public ImRect Rect;
        public ImWindowFlag Flags;
    }
}