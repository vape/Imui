using System;
using Imui.IO.Events;
using Imui.IO.Touch;
using UnityEngine;

namespace Imui.IO
{
    public interface IImuiInput
    {
        public delegate bool RaycasterDelegate(float x, float y);
        
        string Clipboard
        {
            get { return GUIUtility.systemCopyBuffer ?? string.Empty; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        Vector2 MousePosition { get; }
        
        ref readonly ImMouseEvent MouseEvent { get; }
        void UseMouseEvent();
        
        ref readonly ImTextEvent TextEvent { get; }
        void UseTextEvent();
        
        int KeyboardEventsCount { get; }
        ref readonly ImKeyboardEvent GetKeyboardEvent(int index);
        void UseKeyboardEvent(int index);
        void RequestTouchKeyboard(uint owner, ReadOnlySpan<char> text, ImTouchKeyboardSettings settings);

        void UseRaycaster(RaycasterDelegate raycaster);
        void Pull();
    }
}