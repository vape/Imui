using System;
using Imui.IO.Events;
using UnityEngine;

namespace Imui.IO
{
    public delegate bool ImInputRaycaster(float x, float y);
    
    public interface IImInputBackend
    {
        string Clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer ?? string.Empty;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }
        
        Vector2 MousePosition { get; }
        ref readonly ImMouseEvent MouseEvent { get; }
        ref readonly ImTextEvent TextEvent { get; }
        int KeyboardEventsCount { get; }
        
        ref readonly ImKeyboardEvent GetKeyboardEvent(int index);
        
        void UseKeyboardEvent(int index);
        void UseMouseEvent();
        void UseTextEvent();

        void SetRaycaster(ImInputRaycaster raycaster);
        void SetScale(float scale);
        void Pull();
        void RequestTouchKeyboard(ReadOnlySpan<char> text);
    }
}