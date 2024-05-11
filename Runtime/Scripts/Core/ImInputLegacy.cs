using System;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public class ImInputLegacy : ImInput
    {
        private const int MOUSE_EVENTS_QUEUE_SIZE = 4;
        private const int KEYBOARD_EVENTS_QUEUE_SIZE = 16;

        public override Vector2 MousePosition => mousePosition;

        public override ref readonly ImInputMouseEvent MouseEvent => ref mouseEvent;
        public override ref readonly ImInputTextEvent TextEvent => ref textEvent;

        public override int KeyboardEventsCount => keyboardEvents.Count;

        private ImInputMouseEvent mouseEvent;
        private ImInputTextEvent textEvent;
        private CircularBuffer<ImInputKeyboardEvent> keyboardEvents;

        private CircularBuffer<ImInputMouseEvent> mouseEventsQueue;
        private ImInputTextEvent nextTextEvent;
        private CircularBuffer<ImInputKeyboardEvent> nextKeyboardEvents;
        
        private Vector2 mousePosition;
        private Vector2 prevEventMousePosition;
        private ImInputLegacyEventsHandler eventsHandler;
        private float scale;
        private bool disposed;

        public ImInputLegacy()
        {
            mouseEventsQueue = new CircularBuffer<ImInputMouseEvent>(MOUSE_EVENTS_QUEUE_SIZE);
            keyboardEvents = new CircularBuffer<ImInputKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            nextKeyboardEvents = new CircularBuffer<ImInputKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);

            eventsHandler = new GameObject("InInputLegacy").AddComponent<ImInputLegacyEventsHandler>();
            eventsHandler.gameObject.hideFlags = HideFlags.HideAndDontSave;
            eventsHandler.EventCallback = OnEvent;
        }
        
        public override ref readonly ImInputKeyboardEvent GetKeyboardEvent(int index)
        {
            if (index < 0 || index >= keyboardEvents.Count)
            {
                throw new IndexOutOfRangeException($"Event at {index} is out of range");
            }

            return ref keyboardEvents.Get(index);
        }

        public override void UseKeyboardEvent(int index)
        {
            if (index < 0 || index >= keyboardEvents.Count)
            {
                throw new IndexOutOfRangeException($"Event at {index} is out of range");
            }
            
            keyboardEvents.Set(index, default);
        }

        public override void UseMouseEvent()
        {
            mouseEvent = default;
        }

        public override void UseTextEvent()
        {
            textEvent = default;
        }

        public override void SetScale(float scale)
        {
            this.scale = scale;
        }

        public override void Pull()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ImInputLegacy));
            }
            
            mousePosition = UnityEngine.Input.mousePosition / scale;
            
            if (mouseEventsQueue.TryPopBack(out var queuedMouseEvent))
            {
                mouseEvent = queuedMouseEvent;
            }

            (nextKeyboardEvents, keyboardEvents) = (keyboardEvents, nextKeyboardEvents);
            nextKeyboardEvents.Clear();

            HandleTouchKeyboard(out textEvent);
        }

        private void OnEvent(Event evt)
        {
            if (evt == null)
            {
                return;
            }

            var eventMouseDelta = Vector2.zero;
            var eventMousePosition = (Vector2)UnityEngine.Input.mousePosition / scale;

            if (evt.isMouse)
            {
                eventMouseDelta = eventMousePosition - prevEventMousePosition;
                prevEventMousePosition = eventMousePosition;
            }
            
            switch (evt.type)
            {
                case EventType.MouseDown:
                    // HACK (artem-s): because hovered element is determined after frame renders, and with touch input
                    // we can't predict where pointer will be before we click, mousedown needs to be deferred for one frame
                    if (IsTouchSupported() && IsTouchBegan())
                    {
                        mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Move, evt.button, evt.modifiers, eventMouseDelta));
                    }
                    
                    mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Down, evt.button, evt.modifiers, eventMouseDelta));
                    break;
                case EventType.MouseUp:
                    mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Up, evt.button, evt.modifiers, eventMouseDelta));
                    break;
                case EventType.MouseMove:
                    mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Move, evt.button, evt.modifiers, eventMouseDelta));
                    break;
                case EventType.MouseDrag:
                    mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Drag, evt.button, evt.modifiers, eventMouseDelta));
                    break;
                case EventType.ScrollWheel:
                    mouseEventsQueue.PushFront(new ImInputMouseEvent(ImInputMouseEventType.Scroll, evt.button, evt.modifiers, evt.delta));
                    break;
                case EventType.KeyDown:
                    nextKeyboardEvents.PushFront(new ImInputKeyboardEvent(ImInputKeyboardEventType.Down, evt.keyCode, evt.modifiers, ParseKeyboardCommand(evt), evt.character));
                    break;
                case EventType.KeyUp:
                    nextKeyboardEvents.PushFront(new ImInputKeyboardEvent(ImInputKeyboardEventType.Up, evt.keyCode, evt.modifiers, ParseKeyboardCommand(evt), evt.character));
                    break;
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                eventsHandler.EventCallback = null;
                UnityEngine.Object.Destroy(eventsHandler.gameObject);
                eventsHandler = null;
                disposed = true;
            }
            
            base.Dispose(disposing);
        }

        private class ImInputLegacyEventsHandler : MonoBehaviour
        {
            public Action<Event> EventCallback;
            
            private void Awake()
            {
                useGUILayout = false;
            }

            private void OnGUI()
            {
                EventCallback?.Invoke(Event.current);
            }
        }
    }
}