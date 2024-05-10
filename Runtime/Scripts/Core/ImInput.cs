#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
#define MACOS_INPUT_SCHEME
#endif

using System;
using UnityEngine;

namespace Imui.Core
{
    public enum ImInputKeyboardEventType
    {
        None,
        Down,
        Up
    }

    [Flags]
    public enum ImInputKeyboardCommandFlag : uint
    {
        None      = 0,
        Selection = 1 << 0,
        NextWord  = 1 << 1,
        SelectAll = 1 << 2,
        Copy      = 1 << 3,
        Paste     = 1 << 4
    }
    
    public enum ImInputMouseEventType
    {
        None,
        Down,
        Up,
        Move,
        Drag,
        Scroll
    }
    
    public enum ImInputTextEventType
    {
        None,
        Cancel,
        Submit
    }
    
    public readonly struct ImInputKeyboardEvent
    {
        public readonly ImInputKeyboardEventType Type;
        public readonly KeyCode Key;
        public readonly EventModifiers Modifiers;
        public readonly ImInputKeyboardCommandFlag Command;
        public readonly char Char;

        public ImInputKeyboardEvent(ImInputKeyboardEventType type, KeyCode key, EventModifiers modifiers, ImInputKeyboardCommandFlag command, char c)
        {
            Type = type;
            Key = key;
            Modifiers = modifiers;
            Command = command;
            Char = c;
        }
        
        public override string ToString()
        {
            return $"type:{Type} key:{Key} mod:{Modifiers} cmd:{Command} char:{Char}";
        }
    }
    
    public readonly struct ImInputMouseEvent
    {
        public readonly ImInputMouseEventType Type;
        public readonly int Button;
        public readonly EventModifiers Modifiers;
        public readonly Vector2 Delta;

        public ImInputMouseEvent(ImInputMouseEventType type, int button, EventModifiers modifiers, Vector2 delta)
        {
            Type = type;
            Button = button;
            Modifiers = modifiers;
            Delta = delta;
        }

        public override string ToString()
        {
            return $"type:{Type} btn:{Button} mod:{Modifiers} dt:{Delta}";
        }
    }
    
    public readonly struct ImInputTextEvent
    {
        public readonly ImInputTextEventType Type;
        public readonly string Text;

        public ImInputTextEvent(ImInputTextEventType type)
        {
            Type = type;
            Text = null;
        }
        
        public ImInputTextEvent(ImInputTextEventType type, string text)
        {
            Type = type;
            Text = text;
        }

        public override string ToString()
        {
            return $"type:{Type} text:{Text}";
        }
    }
    
    public abstract class ImInput : IDisposable
    {
        private const int TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD = 3;

        public virtual string Clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        public abstract Vector2 MousePosition { get; }

        public abstract ref readonly ImInputMouseEvent MouseEvent { get; }
        public abstract ref readonly ImInputTextEvent TextEvent { get; }

        public abstract int KeyboardEventsCount { get; }

        public abstract ref readonly ImInputKeyboardEvent GetKeyboardEvent(int index);
        
        protected TouchScreenKeyboard TouchKeyboard;
        protected int TouchKeyboardRequestFrame;
        
        public abstract void UseKeyboardEvent(int index);
        public abstract void UseMouseEvent();
        public abstract void UseTextEvent();

        public abstract void SetScale(float scale);
        public abstract void Pull();

        public virtual void RequestTouchKeyboard(ReadOnlySpan<char> text)
        {
            if (!TouchScreenKeyboard.isSupported)
            {
                return;
            }

            if (TouchKeyboard == null)
            {
                TouchKeyboard = TouchScreenKeyboard.Open(new string(text), TouchScreenKeyboardType.Default);
            }

            if (!TouchKeyboard.active)
            {
                TouchKeyboard.active = true;
            }
            
            TouchKeyboardRequestFrame = Time.frameCount;
        }

        protected virtual void HandleTouchKeyboard(out ImInputTextEvent textEvent)
        {
            textEvent = default;
            
            if (TouchKeyboard != null)
            {
                switch (TouchKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Canceled:
                        textEvent = new ImInputTextEvent(ImInputTextEventType.Cancel);
                        break;
                    case TouchScreenKeyboard.Status.Done:
                        textEvent = new ImInputTextEvent(ImInputTextEventType.Submit, TouchKeyboard.text);
                        break;
                }
                
                var shouldHide = Mathf.Abs(Time.frameCount - TouchKeyboardRequestFrame) > TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD;
                if (shouldHide)
                {
                    TouchKeyboard.active = false;
                    TouchKeyboard = null;
                }
            }
        }

        protected static ImInputKeyboardCommandFlag ParseKeyboardCommand(Event evt)
        {
            var result = ImInputKeyboardCommandFlag.None;
            var arrow = (int)evt.keyCode >= 274 && (int)evt.keyCode <= 276;
            var jump = false;
            var control = false;

#if MACOS_INPUT_SCHEME
            jump = evt.modifiers.HasFlag(EventModifiers.Alt); // option key
            control = evt.modifiers.HasFlag(EventModifiers.Command);
#else
            jump = evt.modifiers.HasFlag(EventModifiers.Control);
            control = evt.modifiers.HasFlag(EventModifiers.Control);
#endif
            
            if (arrow && evt.modifiers.HasFlag(EventModifiers.Shift))
            {
                result |= ImInputKeyboardCommandFlag.Selection;
            }

            if (arrow && jump)
            {
                result |= ImInputKeyboardCommandFlag.NextWord;
            }

            if (control && evt.keyCode == KeyCode.A)
            {
                result |= ImInputKeyboardCommandFlag.SelectAll;
            }

            if (control && evt.keyCode == KeyCode.C)
            {
                result |= ImInputKeyboardCommandFlag.Copy;
            }

            if (control && evt.keyCode == KeyCode.V)
            {
                result |= ImInputKeyboardCommandFlag.Paste;
            }
            
            return result;
        }
        
        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}