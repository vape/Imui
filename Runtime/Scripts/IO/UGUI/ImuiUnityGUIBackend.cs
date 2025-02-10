using System;
using System.Runtime.CompilerServices;
using Imui.IO.Events;
using Imui.IO.Utility;
using Imui.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Imui.IO.UGUI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteAlways]
    public class ImuiUnityGUIBackend: Graphic, IImuiRenderer, IImuiInput, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler,
                                      IScrollHandler
    {
        private const int COMMAND_BUFFER_POOL_INITIAL_SIZE = 2;
        private const int MOUSE_EVENTS_QUEUE_SIZE = 4;
        private const int KEYBOARD_EVENTS_QUEUE_SIZE = 16;

        private const float HELD_DOWN_DELAY = 0.2f;
        private const float MULTI_CLICK_TIME_THRESHOLD = 0.2f;
        private const float MULTI_CLICK_POS_THRESHOLD = 20.0f;
        private const float CLICK_POS_THRESHOLD = 8.0f;
        private const int MAX_MOUSE_BUTTONS = 3;

        private static Texture2D ClearTexture;
        private static readonly Vector3[] TempBuffer = new Vector3[4];

        public Vector2 MousePosition => mousePosition;
        public ref readonly ImMouseEvent MouseEvent => ref mouseEvent;
        public ref readonly ImTextEvent TextEvent => ref textEvent;
        public int KeyboardEventsCount => keyboardEvents.Count;

        public override Texture mainTexture => textureRenderer?.Texture == null ? ClearTexture : textureRenderer.Texture;

        private ImInputRaycaster raycaster;
        private ImTextureRenderer textureRenderer;
        private ImDynamicArray<CommandBuffer> commandBufferPool;
        private float scale;
        private ImCircularBuffer<ImMouseEvent> mouseEventsQueue;
        private ImCircularBuffer<ImKeyboardEvent> nextKeyboardEvents;
        private ImCircularBuffer<ImKeyboardEvent> keyboardEvents;
        private Vector2 mousePosition;
        private ImMouseEvent mouseEvent;
        private ImTextEvent textEvent;
        private ImTouchKeyboard touchKeyboardHandler;
        private bool elementHovered;

        private bool mouseHeldDown;
        private ImMouseDevice mouseDownDevice;
        private float[] mouseDownTime = new float[MAX_MOUSE_BUTTONS];
        private int[] mouseDownCount = new int[MAX_MOUSE_BUTTONS];
        private Vector2[] mouseDownPos = new Vector2[MAX_MOUSE_BUTTONS];
        private bool[] possibleClick = new bool[MAX_MOUSE_BUTTONS];

        protected override void Awake()
        {
            base.Awake();

            useGUILayout = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (textureRenderer != null)
            {
                textureRenderer.Dispose();
                textureRenderer = null;
            }

            if (touchKeyboardHandler != null)
            {
                touchKeyboardHandler.Dispose();
                touchKeyboardHandler = null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (ClearTexture == null)
            {
                ClearTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                ClearTexture.SetPixel(0, 0, Color.clear);
                ClearTexture.Apply();
            }

            if (mouseEventsQueue.Array == null)
            {
                mouseEventsQueue = new ImCircularBuffer<ImMouseEvent>(MOUSE_EVENTS_QUEUE_SIZE);
            }

            if (keyboardEvents.Array == null)
            {
                keyboardEvents = new ImCircularBuffer<ImKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            }

            if (nextKeyboardEvents.Array == null)
            {
                nextKeyboardEvents = new ImCircularBuffer<ImKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            }

            if (commandBufferPool.Array == null)
            {
                commandBufferPool = new ImDynamicArray<CommandBuffer>(COMMAND_BUFFER_POOL_INITIAL_SIZE);
            }

            touchKeyboardHandler ??= new ImTouchKeyboard();
            textureRenderer ??= new ImTextureRenderer();
        }

        public void SetRaycaster(ImInputRaycaster raycaster)
        {
            this.raycaster = raycaster;
            SetRaycastDirty();
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if (raycaster == null)
            {
                return false;
            }

            var screenRect = GetWorldRect();
            sp -= screenRect.position;

            return raycaster(sp.x / scale, sp.y / scale);
        }

        public ref readonly ImKeyboardEvent GetKeyboardEvent(int index)
        {
            if (index < 0 || index >= keyboardEvents.Count)
            {
                throw new IndexOutOfRangeException($"Event at {index} is out of range");
            }

            return ref keyboardEvents.Get(index);
        }

        public void UseKeyboardEvent(int index)
        {
            if (index < 0 || index >= keyboardEvents.Count)
            {
                throw new IndexOutOfRangeException($"Event at {index} is out of range");
            }

            keyboardEvents.Set(index, default);
        }

        public void UseMouseEvent()
        {
            mouseEvent = default;
        }

        public void UseTextEvent()
        {
            textEvent = default;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void Pull()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            var mouseBtnLeft = (int)PointerEventData.InputButton.Left;

            mousePosition = GetMousePosition();

            if (mouseEventsQueue.TryPopBack(out var queuedMouseEvent))
            {
                mouseEvent = queuedMouseEvent;
            }
            else if (mouseHeldDown && (Time.unscaledTime - mouseDownTime[mouseBtnLeft]) > HELD_DOWN_DELAY)
            {
                var delta = new Vector2(Time.unscaledTime - mouseDownTime[mouseBtnLeft], 0);
                var count = mouseDownCount[mouseBtnLeft];

                mouseEvent = new ImMouseEvent(ImMouseEventType.Hold, mouseBtnLeft, EventModifiers.None, delta, mouseDownDevice, count);
            }
            else
            {
                mouseEvent = default;
            }

            for (int i = 0; i < possibleClick.Length; ++i)
            {
                if (!possibleClick[i])
                {
                    continue;
                }

                var distance = Vector2.Distance(mousePosition, mouseDownPos[i]);
                if (distance > CLICK_POS_THRESHOLD)
                {
                    possibleClick[i] = false;
                }
            }

            (nextKeyboardEvents, keyboardEvents) = (keyboardEvents, nextKeyboardEvents);
            nextKeyboardEvents.Clear();

            touchKeyboardHandler.HandleTouchKeyboard(out textEvent);
        }

        public Vector2 GetMousePosition()
        {
            return ((Vector2)Input.mousePosition - GetWorldRect().position) / scale;
        }

        public void RequestTouchKeyboard(uint owner, ReadOnlySpan<char> text, ImTouchKeyboardSettings settings)
        {
            touchKeyboardHandler.RequestTouchKeyboard(owner, text, settings);
        }

        private void OnGUI()
        {
            var evt = Event.current;
            if (evt == null)
            {
                return;
            }

            ImKeyboardEventsUtility.TryParse(evt, ref nextKeyboardEvents);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            // (artem-s): with touch input, defer down event one frame so controls first could understand they are hovered
            // before processing actual click

            var device = GetDeviceType(eventData);
            if (device == ImMouseDevice.Touch && IsAnyTouchBegan())
            {
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Move, (int)eventData.button, GetMouseEventModifiers(), eventData.delta / scale,
                    device));
            }

            var btn = (int)eventData.button;
            var pos = GetMousePosition();

            if (Time.unscaledTime - mouseDownTime[btn] >= MULTI_CLICK_TIME_THRESHOLD || (pos - mouseDownPos[btn]).magnitude >= MULTI_CLICK_POS_THRESHOLD)
            {
                mouseDownCount[btn] = 0;
            }

            mouseDownPos[btn] = pos;
            mouseDownCount[btn] += 1;
            mouseDownTime[btn] = Time.unscaledTime;
            possibleClick[btn] = true;
            mouseDownDevice = device;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                mouseHeldDown = true;
            }

            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Down, (int)eventData.button, GetMouseEventModifiers(), eventData.delta / scale,
                device, mouseDownCount[btn]));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var device = GetDeviceType(eventData);
            var button = (int)eventData.button;

            mouseHeldDown = false;
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Up, button, GetMouseEventModifiers(), eventData.delta / scale, device));

            if (!possibleClick[button])
            {
                return;
            }

            var distance = Vector2.Distance(mouseDownPos[button], GetMousePosition());
            if (distance < CLICK_POS_THRESHOLD)
            {
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Click, button, GetMouseEventModifiers(), default, device));
                possibleClick[button] = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var device = GetDeviceType(eventData);

            mouseHeldDown = false;
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Drag, (int)eventData.button, GetMouseEventModifiers(), eventData.delta / scale, device));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var device = GetDeviceType(eventData);

            mouseHeldDown = false;
            mouseEventsQueue.PushFront(
                new ImMouseEvent(ImMouseEventType.BeginDrag, (int)eventData.button, GetMouseEventModifiers(), eventData.delta / scale, device));
        }

        public void OnScroll(PointerEventData eventData)
        {
            mouseHeldDown = false;

            var dx = eventData.scrollDelta.x;
            var dy = eventData.scrollDelta.y;

#if UNITY_WEBGL && !UNITY_EDITOR
            // (artem-s): I fucking hate web
            dx = dx / 3.0f;
            dy = dy / 3.0f;
#endif

            var device = GetDeviceType(eventData);
            var delta = new Vector2(dx, -dy);
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Scroll, (int)eventData.button, EventModifiers.None, delta, device));
        }

        private EventModifiers GetMouseEventModifiers()
        {
            // TODO (artem-s): add support for new input system
#if NEW_INPUT_SYSTEM_ENABLED
            return EventModifiers.None;
#endif

            var result = EventModifiers.None;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                result |= EventModifiers.Control;
            }

            return result;
        }
        
        private ImMouseDevice GetDeviceType(PointerEventData e)
        {
            return e.pointerId >= 0 ? ImMouseDevice.Touch : ImMouseDevice.Mouse;
        }

        private bool IsAnyTouchBegan()
        {
            var touches = Input.touches;
            var count = Input.touchCount;

            for (int i = 0; i < count; ++i)
            {
                if (touches[i].phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }

        private Rect GetWorldRect()
        {
            var cam = canvas.worldCamera;

            rectTransform.GetWorldCorners(TempBuffer);

            var screenBottomLeft = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[0]);
            var screenTopRight = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[2]);

            return new Rect(screenBottomLeft.x, screenBottomLeft.y, screenTopRight.x - screenBottomLeft.x, screenTopRight.y - screenBottomLeft.y);
        }
        
        Vector2 IImuiRenderer.GetTargetSize()
        {
            return GetWorldRect().size;
        }

        CommandBuffer IImuiRenderer.CreateCommandBuffer()
        {
            if (commandBufferPool.Count == 0)
            {
                var cmd = new CommandBuffer() { name = "Imui" };

                commandBufferPool.Add(cmd);
            }

            return commandBufferPool.Pop();
        }

        void IImuiRenderer.ReleaseCommandBuffer(CommandBuffer cmd)
        {
            cmd.Clear();
            commandBufferPool.Add(cmd);
        }

        Vector2Int IImuiRenderer.SetupRenderTarget(CommandBuffer cmd)
        {
            var rect = GetWorldRect();
            var size = new Vector2Int((int)rect.width, (int)rect.height);
            var targetSize = textureRenderer.SetupRenderTarget(cmd, size, out var textureChanged);

            if (textureChanged)
            {
                UpdateMaterial();
            }

            return targetSize;
        }

        void IImuiRenderer.Execute(CommandBuffer cmd)
        {
            Graphics.ExecuteCommandBuffer(cmd);
        }
    }
}