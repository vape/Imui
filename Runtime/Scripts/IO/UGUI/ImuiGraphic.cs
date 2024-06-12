using System;
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
    public class ImuiGraphic : Graphic, 
        IRenderingBackend, 
        IInputBackend, 
        IPointerDownHandler, 
        IPointerUpHandler, 
        IDragHandler, 
        IBeginDragHandler, 
        IScrollHandler
    {
        private const int COMMAND_BUFFER_POOL_INITIAL_SIZE = 2;
        private const int MOUSE_EVENTS_QUEUE_SIZE = 4;
        private const int KEYBOARD_EVENTS_QUEUE_SIZE = 16;
        
        private static Texture2D ClearTexture;
        private static readonly Vector3[] TempBuffer = new Vector3[4];

        public Vector2 MousePosition => mousePosition;
        public ref readonly ImMouseEvent MouseEvent => ref mouseEvent;
        public ref readonly ImTextEvent TextEvent => ref textEvent;
        public int KeyboardEventsCount => keyboardEvents.Count;
        
        public override Texture mainTexture => textureRenderer?.Texture == null ? ClearTexture : textureRenderer.Texture;

        private InputRaycaster raycaster;
        private TextureRenderer textureRenderer;
        private DynamicArray<CommandBuffer> commandBufferPool;
        private float scale;
        private CircularBuffer<ImMouseEvent> mouseEventsQueue;
        private CircularBuffer<ImKeyboardEvent> nextKeyboardEvents;
        private CircularBuffer<ImKeyboardEvent> keyboardEvents;
        private Vector2 mousePosition;
        private ImMouseEvent mouseEvent;
        private ImTextEvent textEvent;
        private TouchKeyboardHandler touchKeyboardHandler;
        private bool elementHovered;
        
        protected override void Awake()
        {
            base.Awake();

            if (!Application.isPlaying)
            {
                return;
            }
            
            useGUILayout = false;
            mouseEventsQueue = new CircularBuffer<ImMouseEvent>(MOUSE_EVENTS_QUEUE_SIZE);
            keyboardEvents = new CircularBuffer<ImKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            nextKeyboardEvents = new CircularBuffer<ImKeyboardEvent>(KEYBOARD_EVENTS_QUEUE_SIZE);
            touchKeyboardHandler = new TouchKeyboardHandler();
            commandBufferPool = new DynamicArray<CommandBuffer>(COMMAND_BUFFER_POOL_INITIAL_SIZE);
            textureRenderer = new TextureRenderer();
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
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (touchKeyboardHandler != null)
            {
                touchKeyboardHandler.Dispose();
                touchKeyboardHandler = null;
            }
        }

        public void SetRaycaster(InputRaycaster raycaster)
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

            var screenRect = GetScreenRect();
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
            mousePosition = GetMousePosition();
            
            if (mouseEventsQueue.TryPopBack(out var queuedMouseEvent))
            {
                mouseEvent = queuedMouseEvent;
            }
            else
            {
                mouseEvent = default;
            }

            (nextKeyboardEvents, keyboardEvents) = (keyboardEvents, nextKeyboardEvents);
            nextKeyboardEvents.Clear();
            
            touchKeyboardHandler.HandleTouchKeyboard(out textEvent);
        }
        
        public Vector2 GetMousePosition()
        {
            return ((Vector2)Input.mousePosition - GetScreenRect().position) / scale;
        }

        public void RequestTouchKeyboard(ReadOnlySpan<char> text)
        {
            touchKeyboardHandler.RequestTouchKeyboard(text);
        }

        private void OnGUI()
        {
            var evt = Event.current;
            if (evt == null)
            {
                return;
            }

            KeyboardEventsUtility.TryParse(evt, ref nextKeyboardEvents);
        }

        
        public void OnPointerDown(PointerEventData eventData)
        {
            // (artem-s): with touch input, defer down event one frame so controls first could understand they are hovered
            // before processing actual click
            
            if (IsTouchSupported() && IsTouchBegan())
            {
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Move, (int)eventData.button, EventModifiers.None, eventData.delta / scale));
            }
            
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Down, (int)eventData.button, EventModifiers.None, eventData.delta / scale));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Up, (int)eventData.button, EventModifiers.None, eventData.delta / scale));
        }

        public void OnDrag(PointerEventData eventData)
        {
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Drag, (int)eventData.button, EventModifiers.None, eventData.delta / scale));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.BeginDrag, (int)eventData.button, EventModifiers.None, eventData.delta / scale));
        }

        public void OnScroll(PointerEventData eventData)
        {
            var delta = new Vector2(eventData.scrollDelta.x, -eventData.scrollDelta.y);
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Scroll, (int)eventData.button, EventModifiers.None, delta));
        }
        
        public Rect GetScreenRect()
        {
            var cam = canvas.worldCamera;
            
            rectTransform.GetWorldCorners(TempBuffer);

            var screenBottomLeft = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[0]);
            var screenTopRight = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[2]);

            return new Rect(
                screenBottomLeft.x, 
                screenBottomLeft.y, 
                screenTopRight.x - screenBottomLeft.x, 
                screenTopRight.y - screenBottomLeft.y);
        }
        
        private bool IsTouchSupported()
        {
#if UNITY_EDITOR
            var isRunningInDeviceSimulator = UnityEngine.Device.SystemInfo.deviceType != DeviceType.Desktop;
#else
            var isRunningInDeviceSimulator = false;
#endif
            
            return isRunningInDeviceSimulator || Input.touchSupported;
        }

        private bool IsTouchBegan()
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
        
        CommandBuffer IRenderingBackend.CreateCommandBuffer()
        {
            if (commandBufferPool.Count == 0)
            {
                var cmd = new CommandBuffer();
                commandBufferPool.Add(cmd);
            }

            return commandBufferPool.Pop();
        }

        void IRenderingBackend.ReleaseCommandBuffer(CommandBuffer cmd)
        {
            cmd.Clear();
            commandBufferPool.Add(cmd);
        }
        
        void IRenderingBackend.SetupRenderTarget(CommandBuffer cmd)
        {
            var rect = GetScreenRect();
            var size = new Vector2Int((int)rect.width, (int)rect.height);
            
            textureRenderer.SetupRenderTarget(cmd, size, out var textureChanged);

            if (textureChanged)
            {
                UpdateMaterial();
            }
        }

        void IRenderingBackend.Execute(CommandBuffer cmd)
        {
            Graphics.ExecuteCommandBuffer(cmd);
        }
    }
}