using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Imui.IO.Utility
{
    public static class ImUnityInputWrapper
    {
#if ENABLE_INPUT_SYSTEM
        public static Vector2 MousePosition => TryGetActiveTouch(out TouchControl touch)
            ? touch.position.ReadValue()
            : Mouse.current?.position?.value ?? default;
        public static bool TouchScreenSupported => Touchscreen.current?.enabled ?? false;
        public static bool IsControlPressed => Keyboard.current?.ctrlKey.isPressed ?? false;
        
        public static bool IsTouchBeganThisFrame()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; ++i)
            {
                if (WasTouchPressedThisFrame(touches[i]))
                {
                    return true;
                }
            }

            return WasTouchPressedThisFrame(touchscreen.primaryTouch);
        }

        static bool TryGetActiveTouch(out TouchControl touch)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                if (IsActiveTouch(touchscreen.primaryTouch))
                {
                    touch = touchscreen.primaryTouch;
                    return true;
                }

                var touches = touchscreen.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    if (!IsActiveTouch(touches[i]))
                    {
                        continue;
                    }

                    touch = touches[i];
                    return true;
                }
            }

            touch = null;
            return false;
        }

        static bool WasTouchPressedThisFrame(TouchControl touch)
        {
            return touch != null
                && (touch.press.wasPressedThisFrame
                    || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began);
        }

        static bool IsActiveTouch(TouchControl touch)
        {
            if (touch == null)
            {
                return false;
            }

            var phase = touch.phase.ReadValue();
            return touch.press.isPressed
                || touch.press.wasPressedThisFrame
                || touch.press.wasReleasedThisFrame
                || phase == UnityEngine.InputSystem.TouchPhase.Began
                || phase == UnityEngine.InputSystem.TouchPhase.Moved
                || phase == UnityEngine.InputSystem.TouchPhase.Stationary;
        }
#else
        public static Vector2 MousePosition => Input.mousePosition;
        public static bool TouchScreenSupported => Input.touchSupported;
        public static bool IsControlPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        public static bool IsTouchBeganThisFrame()
        {
            var count = Input.touchCount;

            for (int i = 0; i < count; ++i)
            {
                if (Input.GetTouch(i).phase == UnityEngine.TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }
#endif
    }
}
