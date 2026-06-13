using System;
using Imui.IO.Events;
using Imui.IO.Rendering;
using Imui.IO.Utility;
using Imui.IO.Touch;
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
        public enum ScalingMode
        {
            Inherited,
            Custom
        }

        private const float CUSTOM_SCALE_MIN = 0.05f;
        private const float CUSTOM_SCALE_MAX = 16.0f;

        private const int MOUSE_EVENTS_QUEUE_SIZE = 4;
        private const int KEYBOARD_EVENTS_QUEUE_SIZE = 16;

        private const float HELD_DOWN_DELAY = 0.2f;
        private const float MULTI_CLICK_TIME_THRESHOLD = 0.2f;
        private const float MULTI_CLICK_POS_THRESHOLD = 20.0f;
        private const float CLICK_POS_THRESHOLD = 8.0f;
        private const int MAX_MOUSE_BUTTONS = 3;

        private static Texture2D ClearTexture;
        private static readonly Vector3[] TempBuffer = new Vector3[4];
        private static Material DefaultMaterial;

        public bool WasMouseDownThisFrame { get; private set; }

        public Vector2 MousePosition => mousePosition;
        public double Time => time;
        public ref readonly ImMouseEvent MouseEvent => ref mouseEvent;
        public ref readonly ImTextEvent TextEvent => ref textEvent;
        public int KeyboardEventsCount => keyboardEvents.Count;

        public override Texture mainTexture => texture?.Texture == null ? ClearTexture : texture.Texture;
        public override Material defaultMaterial => DefaultMaterial ? DefaultMaterial : base.defaultMaterial;

        public float CustomScale
        {
            get => customScale;
            set => customScale = Mathf.Clamp(value, CUSTOM_SCALE_MIN, CUSTOM_SCALE_MAX);
        }

        public ScalingMode Scaling
        {
            get => scalingMode;
            set => scalingMode = value;
        }

        [SerializeField] private ScalingMode scalingMode = ScalingMode.Inherited;
        [SerializeField] private float customScale = 1.0f;
        [Tooltip("Simulate touch input from the desktop mouse in Editor play mode, for testing app touch input when Game View is in Simulator mode.")]
        [SerializeField] private bool simulateTouchWithMouseInEditor;

        private IImuiInput.RaycasterDelegate raycaster;
        private ImDynamicRenderTexture texture;
        private ImCircularBuffer<ImMouseEvent> mouseEventsQueue;
        private ImCircularBuffer<ImKeyboardEvent> nextKeyboardEvents;
        private ImCircularBuffer<ImKeyboardEvent> keyboardEvents;
        private IImuiRenderingScheduler scheduler;
        private Vector2 mousePosition;
        private ImMouseEvent mouseEvent;
        private ImTextEvent textEvent;
        private ImTouchKeyboard touchKeyboardHandler;
        private bool elementHovered;
        private double time;

        private bool mouseHeldDown;
        private ImMouseDevice mouseDownDevice;
        private float[] mouseDownTime = new float[MAX_MOUSE_BUTTONS];
        private int[] mouseDownCount = new int[MAX_MOUSE_BUTTONS];
        private Vector2[] mouseDownPos = new Vector2[MAX_MOUSE_BUTTONS];
        private bool[] possibleClick = new bool[MAX_MOUSE_BUTTONS];
#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
        private bool polledPointerPressed;
        private bool polledPointerOwnsPress;
        private bool editorTouchSimulationRegistered;
#endif

        protected override void Awake()
        {
            base.Awake();

            if (!DefaultMaterial)
            {
                var shader = Resources.Load<Shader>("Imui/imui_ugui");
                if (shader)
                {
                    DefaultMaterial = new Material(shader);
                }
            }

            useGUILayout = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (texture != null)
            {
                texture.Dispose();
                texture = null;
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

            scheduler ??= GraphicsSettings.currentRenderPipeline ? new ImuiScriptableRenderingScheduler() : new ImuiBuiltinRenderingScheduler();

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

            touchKeyboardHandler ??= new ImTouchKeyboard();
            texture ??= new ImDynamicRenderTexture();

#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
            if (simulateTouchWithMouseInEditor && Application.isPlaying)
            {
                ImUnityInputWrapper.EnableEditorTouchSimulation();
                editorTouchSimulationRegistered = true;
            }
#endif
        }

        protected override void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
            if (editorTouchSimulationRegistered)
            {
                ImUnityInputWrapper.DisableEditorTouchSimulation();
                editorTouchSimulationRegistered = false;
            }
            polledPointerPressed = false;
            polledPointerOwnsPress = false;
#endif

            base.OnDisable();
        }

        // ReSharper disable once ParameterHidesMember
        public void UseRaycaster(IImuiInput.RaycasterDelegate raycaster)
        {
            this.raycaster = raycaster;
#if UNITY_2021_3_OR_NEWER
            SetRaycastDirty();
#endif
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if (raycaster == null)
            {
                return false;
            }

            var scale = GetScale();
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
            time = UnityEngine.Time.unscaledTimeAsDouble;
#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
            PollPointerFallback(mouseBtnLeft);
#endif

            if (mouseEventsQueue.TryPopBack(out var queuedMouseEvent))
            {
                mouseEvent = queuedMouseEvent;
            }
            else if (mouseHeldDown && (UnityEngine.Time.unscaledTime - mouseDownTime[mouseBtnLeft]) > HELD_DOWN_DELAY)
            {
                var delta = new Vector2(UnityEngine.Time.unscaledTime - mouseDownTime[mouseBtnLeft], 0);
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

            WasMouseDownThisFrame = mouseEvent.Type == ImMouseEventType.Down;
        }

        public Vector2 GetMousePosition()
        {
            return ((Vector2)GetScreenPointerPosition() - GetWorldRect().position) / GetScale();
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

            var keyboardEventType = evt.type switch
            {
                EventType.KeyDown => ImKeyboardEventType.Down,
                EventType.KeyUp => ImKeyboardEventType.Up,
                _ => ImKeyboardEventType.None
            };

            if (keyboardEventType != ImKeyboardEventType.None)
            {
                nextKeyboardEvents.PushFront(new ImKeyboardEvent(keyboardEventType, evt.keyCode, evt.modifiers, evt.character));
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // (artem-s): with touch input, defer down event one frame so controls first could understand they are hovered
            // before processing the actual click

            var device = GetDeviceType(eventData);
            if (device == ImMouseDevice.Touch && ImUnityInputWrapper.IsTouchBeganThisFrame())
            {
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Move,
                                                            (int)eventData.button,
                                                            GetMouseEventModifiers(),
                                                            eventData.delta / GetScale(),
                                                            device));
            }

            var btn = (int)eventData.button;
            var pos = GetMousePosition();

            if (UnityEngine.Time.unscaledTime - mouseDownTime[btn] >= MULTI_CLICK_TIME_THRESHOLD || (pos - mouseDownPos[btn]).magnitude >= MULTI_CLICK_POS_THRESHOLD)
            {
                mouseDownCount[btn] = 0;
            }

            mouseDownPos[btn] = pos;
            mouseDownCount[btn] += 1;
            mouseDownTime[btn] = UnityEngine.Time.unscaledTime;
            possibleClick[btn] = true;
            mouseDownDevice = device;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                mouseHeldDown = true;
            }

            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Down,
                                                        (int)eventData.button,
                                                        GetMouseEventModifiers(),
                                                        eventData.delta / GetScale(),
                                                        device,
                                                        mouseDownCount[btn]));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var device = GetDeviceType(eventData);
            var button = (int)eventData.button;

            mouseHeldDown = false;
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Up, button, GetMouseEventModifiers(), eventData.delta / GetScale(), device));

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
            var delta = eventData.delta / GetScale();
            var button = (int)eventData.button;
            var modifiers = GetMouseEventModifiers();

            mouseHeldDown = false;

            if (mouseEventsQueue.TryPeekFront(out var existingEvent) &&
                existingEvent.Type == ImMouseEventType.BeginDrag &&
                existingEvent.Button == button &&
                existingEvent.Modifiers == modifiers &&
                existingEvent.Device == device &&
                existingEvent.Delta == delta)
            {
                // (artem-s): skip this event, because drag delta is already handled by BeginDrag
                return;
            }

            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Drag, button, modifiers, delta, device));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var device = GetDeviceType(eventData);

            mouseHeldDown = false;
            mouseEventsQueue.PushFront(
                new ImMouseEvent(ImMouseEventType.BeginDrag, (int)eventData.button, GetMouseEventModifiers(), eventData.delta / GetScale(), device));
        }

        public void OnScroll(PointerEventData eventData)
        {
            mouseHeldDown = false;

            var dx = eventData.scrollDelta.x;
            var dy = -eventData.scrollDelta.y;
            
            var device = GetDeviceType(eventData);
            var delta = ImUnityScrollUtility.ProcessScrollDelta(dx, dy);
            mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Scroll, (int)eventData.button, EventModifiers.None, delta, device));
        }

        private EventModifiers GetMouseEventModifiers()
        {
            var result = EventModifiers.None;

            if (ImUnityInputWrapper.IsControlPressed)
            {
                result |= EventModifiers.Control;
            }

            return result;
        }

#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
        private void PollPointerFallback(int button)
        {
            // Game View Simulator testing can use the desktop mouse as simulated touch input.
            if (!simulateTouchWithMouseInEditor)
            {
                polledPointerPressed = false;
                polledPointerOwnsPress = false;
                return;
            }

            bool pressed = ImUnityInputWrapper.PointerPressed;
            if (mouseEventsQueue.Count > 0)
            {
                polledPointerPressed = pressed;
                return;
            }

            bool pressedThisFrame = ImUnityInputWrapper.PointerPressedThisFrame || (pressed && !polledPointerPressed);
            bool releasedThisFrame = !pressed && polledPointerPressed;

            if (pressedThisFrame)
            {
                if (!ImUnityInputWrapper.TryGetPointerPosition(out Vector2 screenPosition) || !RaycastScreenPosition(screenPosition))
                {
                    polledPointerPressed = pressed;
                    return;
                }

                polledPointerOwnsPress = true;

                if (UnityEngine.Time.unscaledTime - mouseDownTime[button] >= MULTI_CLICK_TIME_THRESHOLD
                    || (mousePosition - mouseDownPos[button]).magnitude >= MULTI_CLICK_POS_THRESHOLD)
                    mouseDownCount[button] = 0;

                mouseDownPos[button] = mousePosition;
                mouseDownCount[button] += 1;
                mouseDownTime[button] = UnityEngine.Time.unscaledTime;
                possibleClick[button] = true;
                mouseDownDevice = ImUnityInputWrapper.TouchScreenSupported ? ImMouseDevice.Touch : ImMouseDevice.Mouse;
                mouseHeldDown = true;

                mouseEventsQueue.PushFront(new ImMouseEvent(
                    ImMouseEventType.Down,
                    button,
                    GetMouseEventModifiers(),
                    default,
                    mouseDownDevice,
                    mouseDownCount[button]));
            }
            else if (releasedThisFrame && polledPointerOwnsPress)
            {
                mouseHeldDown = false;
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Up, button, GetMouseEventModifiers(), default, mouseDownDevice));

                if (possibleClick[button] && Vector2.Distance(mouseDownPos[button], mousePosition) < CLICK_POS_THRESHOLD)
                {
                    mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.Click, button, GetMouseEventModifiers(), default, mouseDownDevice));
                    possibleClick[button] = false;
                }

                polledPointerOwnsPress = false;
            }

            polledPointerPressed = pressed;
        }
#endif

        private static Vector2 GetScreenPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return ImUnityInputWrapper.TryGetPointerPosition(out Vector2 position) ? position : default;
#else
            return ImUnityInputWrapper.MousePosition;
#endif
        }

        private bool RaycastScreenPosition(Vector2 screenPosition)
        {
            if (raycaster == null)
                return false;

            var scale = GetScale();
            var screenRect = GetWorldRect();
            if (!screenRect.Contains(screenPosition))
                return false;

            var localPosition = (screenPosition - screenRect.position) / scale;
            return raycaster(localPosition.x, localPosition.y);
        }

        private ImMouseDevice GetDeviceType(PointerEventData e)
        {
            return ImUnityInputWrapper.TouchScreenSupported && e.pointerId >= 0 ? ImMouseDevice.Touch : ImMouseDevice.Mouse;
        }

        private Rect GetWorldRect()
        {
            var cam = canvas.worldCamera;

            rectTransform.GetWorldCorners(TempBuffer);

            var screenBottomLeft = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[0]);
            var screenTopRight = RectTransformUtility.WorldToScreenPoint(cam, TempBuffer[2]);

            return new Rect(screenBottomLeft.x, screenBottomLeft.y, screenTopRight.x - screenBottomLeft.x, screenTopRight.y - screenBottomLeft.y);
        }

        public Vector2 GetScreenSize()
        {
            return GetWorldRect().size;
        }

        public float GetScale()
        {
            return scalingMode == ScalingMode.Inherited ? canvas.scaleFactor : customScale;
        }

        Vector2Int IImuiRenderer.SetupRenderTarget(CommandBuffer cmd)
        {
            var rect = GetWorldRect();
            var size = new Vector2Int((int)rect.width, (int)rect.height);
            var targetSize = texture.SetupRenderTarget(cmd, size, out var textureChanged);

            if (textureChanged)
            {
                UpdateMaterial();
            }

            return targetSize;
        }

        public void Schedule(IImuiRenderDelegate renderDelegate)
        {
            scheduler.Schedule(renderDelegate);
        }
    }
}
