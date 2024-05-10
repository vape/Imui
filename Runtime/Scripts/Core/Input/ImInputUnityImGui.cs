#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
#define IMUI_MACOS
#endif

using System;
using System.Collections.Generic;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core.Input
{
    public class ImInputUnityImGui : IDisposable, IImInput
    {
        private const int KEYBOARD_EVENTS_QUEUE_SIZE = 16;
        private const int TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD = 3;
        
        public Vector2 MousePosition => mousePosition;

        public string Clipboard
        {
            get => GUIUtility.systemCopyBuffer;
            set => GUIUtility.systemCopyBuffer = value;
        }
        
        public ref readonly ImInputMouseEvent MouseEvent => ref mouseEvent;

        public ref readonly ImInputTextEvent TextEvent => ref textEvent;

        public int KeyboardEventsCount => keyboardEvents.Count;
        
        private Vector2 mousePosition;
        private ImInputMouseEvent mouseEvent;
        private ImInputTextEvent textEvent;
        private CircularBuffer<ImInputKeyboardEvent> keyboardEvents;
        
        private Queue<ImInputMouseEvent> mouseEventsQueue = new(capacity: 4);
        private float scale = 1.0f;
        private Vector2 prevEventMousePosition;
        private ImInputMouseEvent nextMouseEvent;
        private bool disposed;
        private CircularBuffer<ImInputKeyboardEvent> nextKeyboardEventsQueue;
        private TouchScreenKeyboard touchScreenKeyboard;
        private int touchScreenKeyboardRequestedFrame;
            
        public ImInputUnityImGui()
        {
            keyboardEvents = new CircularBuffer<ImInputKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            nextKeyboardEventsQueue = new CircularBuffer<ImInputKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
        }
        
        public ref readonly ImInputKeyboardEvent GetKeyboardEvent(int index)
        {
            return ref keyboardEvents.Get(index);
        }
        
        public void UseKeyboard(int index)
        {
            keyboardEvents.Set(index, default);
        }

        public void UseMouse()
        {
            mouseEvent = default;
        }

        public void UseText()
        {
            textEvent = default;
        }
        
        public void RequestKeyboard(ReadOnlySpan<char> text)
        {
            if (!TouchScreenKeyboard.isSupported)
            {
                return;
            }

            if (touchScreenKeyboard == null)
            {
                touchScreenKeyboard = TouchScreenKeyboard.Open(new string(text), TouchScreenKeyboardType.Default);
            }

            if (!touchScreenKeyboard.active)
            {
                touchScreenKeyboard.active = true;
            }
            
            touchScreenKeyboardRequestedFrame = Time.frameCount;
        }
        
        public void Pull()
        {
            if (mouseEventsQueue.TryDequeue(out var queuedMouseEvent))
            {
                if (nextMouseEvent.Type != ImInputEventMouseType.None)
                {
                    mouseEventsQueue.Enqueue(nextMouseEvent);
                }

                nextMouseEvent = queuedMouseEvent;
            }
            
            mousePosition = UnityEngine.Input.mousePosition / scale;
            mouseEvent = nextMouseEvent;

            (nextKeyboardEventsQueue, keyboardEvents) = (keyboardEvents, nextKeyboardEventsQueue);

            nextMouseEvent = default;
            nextKeyboardEventsQueue.Clear();

            textEvent = default;
            
            if (touchScreenKeyboard != null)
            {
                switch (touchScreenKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Canceled:
                        textEvent = new ImInputTextEvent(ImInputTextEventType.Cancel);
                        break;
                    case TouchScreenKeyboard.Status.Done:
                        textEvent = new ImInputTextEvent(ImInputTextEventType.Submit, touchScreenKeyboard.text);
                        break;
                }
                
                var shouldHide = (Time.frameCount - touchScreenKeyboardRequestedFrame) > TOUCH_KEYBOARD_CLOSE_FRAMES_THRESHOLD;
                
                if (shouldHide)
                {
                    touchScreenKeyboard.active = false;
                    touchScreenKeyboard = null;
                }
            }
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void ProcessEvents()
        {
            var e = Event.current;
            if (e == null)
            {
                return;
            }

            var eventMouseDelta = Vector2.zero;
            var eventMousePosition = (Vector2)UnityEngine.Input.mousePosition / scale;

            if (e.isMouse)
            {
                eventMouseDelta = eventMousePosition - prevEventMousePosition;
                prevEventMousePosition = eventMousePosition;
            }
            
            switch (e.type)
            {
                case EventType.MouseDown:
                    // HACK (artem-s): because hovered element is determined after frame renders, and with touch input
                    // we can't predict where pointer will be before we click, mousedown needs to be deferred for one frame
                    if (IsTouchSupported() && IsTouchBegan())
                    {
                        mouseEventsQueue.Enqueue(new ImInputMouseEvent(ImInputEventMouseType.Move, e.button, e.modifiers, eventMouseDelta));
                    }
                    
                    nextMouseEvent = new ImInputMouseEvent(ImInputEventMouseType.Down, e.button, e.modifiers, eventMouseDelta);
                    break;
                case EventType.MouseUp:
                    nextMouseEvent = new ImInputMouseEvent(ImInputEventMouseType.Up, e.button, e.modifiers, eventMouseDelta);
                    break;
                case EventType.MouseMove:
                    nextMouseEvent = new ImInputMouseEvent(ImInputEventMouseType.Move, e.button, e.modifiers, eventMouseDelta);
                    break;
                case EventType.MouseDrag:
                    nextMouseEvent = new ImInputMouseEvent(ImInputEventMouseType.Drag, e.button, e.modifiers, eventMouseDelta);
                    break;
                case EventType.ScrollWheel:
                    nextMouseEvent = new ImInputMouseEvent(ImInputEventMouseType.Scroll, e.button, e.modifiers, e.delta);
                    break;
                case EventType.KeyDown:
                    nextKeyboardEventsQueue.PushFront(new ImInputKeyboardEvent(ImInputEventKeyboardType.Down, e.keyCode, e.modifiers, ParseCommand(e), e.character));
                    break;
                case EventType.KeyUp:
                    nextKeyboardEventsQueue.PushFront(new ImInputKeyboardEvent(ImInputEventKeyboardType.Up, e.keyCode, e.modifiers, ParseCommand(e), e.character));
                    break;
            }
        }

        private ImInputKeyboardCommand ParseCommand(Event e)
        {
            var command = ImInputKeyboardCommand.None;
            var code = (int)e.keyCode;
            var isArrow = code >= 274 && code <= 276;

            if (isArrow && e.modifiers.HasFlag(EventModifiers.Shift))
            {
                command |= ImInputKeyboardCommand.Selection;
            }

            var jumpModifier = false;
            var selectModifier = false;
            
            #if IMUI_MACOS
            jumpModifier = e.modifiers.HasFlag(EventModifiers.Alt);
            selectModifier = e.modifiers.HasFlag(EventModifiers.Command);
            #else
            jumpModifier = e.modifiers.HasFlag(EventModifiers.Control);
            selectModifier = e.modifiers.HasFlag(EventModifiers.Control);
            #endif

            if (isArrow && jumpModifier)
            {
                command |= ImInputKeyboardCommand.JumpWord;
            }

            if (selectModifier && e.keyCode == KeyCode.A)
            {
                command |= ImInputKeyboardCommand.SelectAll;
            }

            if (selectModifier && e.keyCode == KeyCode.C)
            {
                command |= ImInputKeyboardCommand.Copy;
            }

            if (selectModifier && e.keyCode == KeyCode.V)
            {
                command |= ImInputKeyboardCommand.Paste;
            }
            
            return command;
        }
        
        private bool IsTouchSupported()
        {
            return PlatformUtility.IsEditorSimulator() || UnityEngine.Input.touchSupported;
        }

        private bool IsTouchBegan()
        {
            var touches = UnityEngine.Input.touches;
            var count = UnityEngine.Input.touchCount;
            
            for (int i = 0; i < count; ++i)
            {
                if (touches[i].phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
        }
    }
}