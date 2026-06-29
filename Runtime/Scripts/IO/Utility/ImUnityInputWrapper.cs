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
        public static Vector2 MousePosition
        {
            get
            {
#if UNITY_EDITOR
                if (TryGetMousePosition(out Vector2 mousePosition))
                    return mousePosition;
                if (TryGetActiveTouch(out TouchControl touch))
                    return touch.position.ReadValue();
                // Device Simulator disables the mouse and reports a hovering touch with
                // phase Ended (so TryGetActiveTouch rejects it), yet Pointer.current still
                // carries the correct device-space position. Use it so hover doesn't snap to (0,0).
                if (TryGetPointerPosition(out Vector2 pointerPosition))
                    return pointerPosition;
                return default;
#else
                if (TryGetActiveTouch(out TouchControl touch))
                    return touch.position.ReadValue();
                if (TryGetMousePosition(out Vector2 mousePosition))
                    return mousePosition;
                if (TryGetPointerPosition(out Vector2 pointerPosition))
                    return pointerPosition;
                return default;
#endif
            }
        }

        public static bool TouchScreenSupported => Touchscreen.current?.enabled ?? false;
        public static bool IsControlPressed => Keyboard.current?.ctrlKey.isPressed ?? false;

        static bool TryGetMousePosition(out Vector2 position)
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.enabled)
            {
                position = default;
                return false;
            }

            position = mouse.position.ReadValue();
            return IsFinite(position);
        }

        static bool TryGetPointerPosition(out Vector2 position)
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.enabled)
            {
                position = default;
                return false;
            }

            position = pointer.position.ReadValue();
            return IsFinite(position);
        }
        
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

        static bool IsFinite(Vector2 value)
        {
            return !float.IsNaN(value.x)
                && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y)
                && !float.IsInfinity(value.y);
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
