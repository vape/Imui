using System;
using UnityEngine;

namespace Imui.Core.Input
{
    public interface IImInput
    {
        Vector2 MousePosition { get; }
        
        ref readonly ImInputMouseEvent MouseEvent { get; }
        int KeyboardEventsCount { get; }
        
        ref readonly ImInputKeyboardEvent GetKeyboardEvent(int index);
        
        void UseKeyboard(int index);
        void UseMouse();

        void Pull();
        void SetScale(float value);
    }
}