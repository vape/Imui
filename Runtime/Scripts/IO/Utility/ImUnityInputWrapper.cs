using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.Controls;
#if UNITY_EDITOR
using EnhancedTouchSupport = UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport;
using TouchSimulation = UnityEngine.InputSystem.EnhancedTouch.TouchSimulation;
#endif
#endif

namespace Imui.IO.Utility
{
    public static class ImUnityInputWrapper
    {
#if ENABLE_INPUT_SYSTEM
        public static Vector2 MousePosition => TryGetPointerPosition(out Vector2 position) ? position : default;
        public static bool TouchScreenSupported => Touchscreen.current?.enabled ?? false;
        public static bool IsControlPressed => Keyboard.current?.ctrlKey.isPressed ?? false;
#if UNITY_EDITOR
        private static int editorTouchSimulationUsers;
#endif

        public static bool PointerPressed
        {
            get
            {
                if (TryGetEditorEnhancedTouch(out _))
                    return true;

                if (TryGetPressedTouch(out _))
                    return true;

                Mouse mouse = Mouse.current;
                return mouse != null && mouse.leftButton.isPressed;
            }
        }

        public static bool PointerPressedThisFrame
        {
            get
            {
                if (IsTouchBeganThisFrame())
                    return true;

                Mouse mouse = Mouse.current;
                return mouse != null && mouse.leftButton.wasPressedThisFrame;
            }
        }
        
        public static bool IsTouchBeganThisFrame()
        {
            if (EditorEnhancedTouchWasPressedThisFrame())
                return true;

            Touchscreen touchscreen = Touchscreen.current;
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

        public static bool TryGetPointerPosition(out Vector2 position)
        {
            if (TryGetEditorEnhancedTouch(out var editorTouch))
            {
                position = editorTouch.screenPosition;
                if (IsFinite(position))
                    return true;
            }

            if (TryGetActiveTouch(out TouchControl touch))
            {
                position = touch.position.ReadValue();
                if (IsFinite(position))
                    return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                position = mouse.position.ReadValue();
                if (IsFinite(position))
                    return true;
            }

            position = default;
            return false;
        }

        static bool TryGetPressedTouch(out TouchControl touch)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                if (IsPressedTouch(touchscreen.primaryTouch))
                {
                    touch = touchscreen.primaryTouch;
                    return true;
                }

                var touches = touchscreen.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    if (!IsPressedTouch(touches[i]))
                        continue;

                    touch = touches[i];
                    return true;
                }
            }

            touch = null;
            return false;
        }

        static bool TryGetActiveTouch(out TouchControl touch)
        {
            Touchscreen touchscreen = Touchscreen.current;
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
                        continue;

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

        static bool IsPressedTouch(TouchControl touch)
        {
            return touch != null && touch.press.isPressed;
        }

        static bool IsActiveTouch(TouchControl touch)
        {
            if (touch == null)
                return false;

            if (!IsFinite(touch.position.ReadValue()))
                return false;

            UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();
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

        static bool EditorEnhancedTouchWasPressedThisFrame()
        {
#if UNITY_EDITOR
            if (!EditorTouchSimulationEnabled)
                return false;

            var touches = EnhancedTouch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began && IsFinite(touch.screenPosition))
                    return true;
            }
#endif

            return false;
        }

        static bool TryGetEditorEnhancedTouch(out EnhancedTouch touch)
        {
#if UNITY_EDITOR
            if (!EditorTouchSimulationEnabled)
            {
                touch = default;
                return false;
            }

            var touches = EnhancedTouch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                touch = touches[i];
                UnityEngine.InputSystem.TouchPhase phase = touch.phase;
                if (IsFinite(touch.screenPosition)
                    && (phase == UnityEngine.InputSystem.TouchPhase.Began
                        || phase == UnityEngine.InputSystem.TouchPhase.Moved
                        || phase == UnityEngine.InputSystem.TouchPhase.Stationary))
                    return true;
            }
#endif

            touch = default;
            return false;
        }

#if UNITY_EDITOR
        static bool EditorTouchSimulationEnabled => editorTouchSimulationUsers > 0;

        public static void EnableEditorTouchSimulation()
        {
            editorTouchSimulationUsers++;
            if (editorTouchSimulationUsers > 1)
                return;

            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();
        }

        public static void DisableEditorTouchSimulation()
        {
            if (editorTouchSimulationUsers == 0)
                return;

            editorTouchSimulationUsers--;
            if (editorTouchSimulationUsers > 0)
                return;

            TouchSimulation.Disable();
            EnhancedTouchSupport.Disable();
        }
#endif
#else
        public static Vector2 MousePosition => Input.mousePosition;
        public static bool TouchScreenSupported => Input.touchSupported;
        public static bool IsControlPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool PointerPressed => Input.GetMouseButton(0);
        public static bool PointerPressedThisFrame => Input.GetMouseButtonDown(0);
        public static bool TryGetPointerPosition(out Vector2 position)
        {
            position = Input.mousePosition;
            return true;
        }
        
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
