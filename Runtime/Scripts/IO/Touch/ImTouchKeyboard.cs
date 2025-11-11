using System;
using Imui.IO.Events;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace Imui.IO.Touch
{
    public enum ImTouchKeyboardType
    {
        Default,
        Numeric
    }

    public struct ImTouchKeyboardSettings
    {
        public bool Muiltiline;
        public ImTouchKeyboardType Type;
        public int CharactersLimit;
        public RangeInt Selection;
    }

    public class ImTouchKeyboard: IDisposable
    {
        private const int TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD = 3;

        public TouchScreenKeyboard TouchKeyboard;

        public bool NativeInputFieldHidden
        {
            get
            {
                var inputFieldHidden = TouchScreenKeyboard.hideInput;

#if UNITY_6000_0_OR_NEWER
                inputFieldHidden |= TouchScreenKeyboard.inputFieldAppearance == TouchScreenKeyboard.InputFieldAppearance.AlwaysHidden;
#endif

                return inputFieldHidden;
            }
        }

        private uint touchKeyboardOwner;
        private int touchKeyboardRequestFrame;
        private bool textInputDirty;

        public ImTouchKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Keyboard.current.onIMECompositionChange += OnTextInput;
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnTextInput(IMECompositionString c)
        {
            textInputDirty = true;
        }
#endif

        public void RequestTouchKeyboard(uint owner, ReadOnlySpan<char> text, ImTouchKeyboardSettings settings)
        {
#if UNITY_WEBGL
            // TODO (artem-s): fix touch keyboard handling for webgl
            return;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            
            if (!TouchScreenKeyboard.isSupported)
            {
                return;
            }

            if (owner != touchKeyboardOwner && TouchKeyboard != null)
            {
                TouchKeyboard.active = false;
                TouchKeyboard = null;
                touchKeyboardOwner = 0;
            }

            if (TouchKeyboard == null)
            {
                touchKeyboardOwner = owner;
                TouchKeyboard = TouchScreenKeyboard.Open(
                    new string(text),
                    GetType(settings.Type),
                    false,
                    settings.Muiltiline);

                TouchKeyboard.characterLimit = settings.CharactersLimit;
                TouchKeyboard.selection = settings.Selection;
            }

            if (!TouchKeyboard.active)
            {
                TouchKeyboard.active = true;
            }

            if (NativeInputFieldHidden && TouchKeyboard.active && TouchKeyboard.canSetSelection)
            {
                var prev = TouchKeyboard.selection;
                var changed = prev.start != settings.Selection.start || prev.length != settings.Selection.length;
                if (changed)
                {
                    // (artem-s): setting selection does allocate currently edited string for range checking
                    TouchKeyboard.selection = settings.Selection;
                }
            }

            touchKeyboardRequestFrame = Time.frameCount;
            
#pragma warning restore CS0162 // Unreachable code detected
        }

        private TouchScreenKeyboardType GetType(ImTouchKeyboardType type)
        {
            switch (type)
            {
                case ImTouchKeyboardType.Numeric:
                    return TouchScreenKeyboardType.NumbersAndPunctuation;
                default:
                    return TouchScreenKeyboardType.Default;
            }
        }

        public void HandleTouchKeyboard(out ImTextEvent textEvent)
        {
            var textDirty = textInputDirty;
            
            textInputDirty = false;
            textEvent = default;

            if (TouchKeyboard != null)
            {
                var shouldHide = Mathf.Abs(Time.frameCount - touchKeyboardRequestFrame) > TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD;

                switch (TouchKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Visible when NativeInputFieldHidden:
                        var sendEvent = textDirty;
#if !ENABLE_INPUT_SYSTEM
                        sendEvent |= Input.inputString?.Length > 0;
#endif

                        if (sendEvent)
                        {
                            var text = TouchKeyboard.text;
                            // TODO (artem-s): would be nice to update selection even when text itself hasn't changed
                            RangeInt? selection = TouchKeyboard.canGetSelection ? TouchKeyboard.selection : null;
                            textEvent = new ImTextEvent(ImTextEventType.Set, text, selection);
                        }
                        break;
                    case TouchScreenKeyboard.Status.Canceled:
                        textEvent = new ImTextEvent(ImTextEventType.Cancel);
                        shouldHide = true;
                        break;
                    case TouchScreenKeyboard.Status.Done:
                        textEvent = new ImTextEvent(ImTextEventType.Submit, TouchKeyboard.text);
                        shouldHide = true;
                        break;
                }

                if (shouldHide)
                {
                    TouchKeyboard.active = false;
                    TouchKeyboard = null;
                }
            }
        }

        public void Dispose()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Keyboard.current.onIMECompositionChange -= OnTextInput;
            }
#endif

            if (TouchKeyboard != null)
            {
                TouchKeyboard.active = false;
                TouchKeyboard = null;
            }
        }
    }
}