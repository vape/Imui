using UnityEngine;

namespace Imui.Core.Input
{
    public interface IImInput
    {
        ref readonly Vector2 MousePosition { get; }
        ref readonly ImInputMouseEvent MouseEvent { get; }
        ref readonly ImInputKeyboardEvent KeyboardEvent { get; }

        void UseKeyboard();
        void UseMouse();

        void Pull();
        void SetScale(float value);
    }
}