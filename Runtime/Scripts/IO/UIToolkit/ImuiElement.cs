using System;
using Imui.Core;
using Imui.IO.Events;
using Imui.IO.Rendering;
using Imui.IO.Touch;
using Imui.IO.Utility;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Imui.IO.UIToolkit
{
    public interface IImuiElementDelegate
    {
        void Draw(ImGui gui);
    }

    public class ImuiElement: VisualElement, IImuiRenderer, IImuiInput, IDisposable
    {
        private const ImMouseDevice MOUSE_DEVICE = ImMouseDevice.Mouse;
        private const float DRAG_DISTANCE_THRESHOLD = 5;
        private const double DEFAULT_THROTTLE_REFRESH_DELAY = 1.0d;
        private const int THROTTLE_COOLDOWN_FRAMES = 2;

        // TODO (artem-s): use scaledPixelsPerPoint instead?
        public float PixelsPerPoint { get; set; } = 1.0f;
        public bool Throttle { get; set; } = true;

        public ref readonly ImMouseEvent MouseEvent => ref mouseEvent;
        public ref readonly ImTextEvent TextEvent => ref textEvent;
        public int KeyboardEventsCount => keyboardEvents.Count;
        public Vector2 MousePosition => mousePosition;
        public double Time => globalTime;
        public float DeltaTime => deltaTime;

        public bool WasMouseDownThisFrame { get; private set; }

        private ImMouseEvent mouseEvent;
        private ImTextEvent textEvent;
        private Vector2 mousePosition;
        private bool wasDragging;
        private Vector2 mouseDownPosition;
        private bool possibleDrag;
        private ImCircularBuffer<ImMouseEvent> mouseEventsQueue;
        private ImCircularBuffer<ImKeyboardEvent> nextKeyboardEvents;
        private ImCircularBuffer<ImKeyboardEvent> keyboardEvents;
        private double globalTime;
        private float deltaTime;
        private double lastUpdate;
        private int eventsCooldown;
        private bool contentDirty = true;
        private double throttleRefreshDelay;
        
        private readonly ImGui gui;
        private readonly IImuiElementDelegate elementDelegate;
        private readonly IImuiRenderingScheduler renderScheduler;
        private readonly ImDynamicRenderTexture textureRenderer;
        private readonly Vertex[] vertices = new Vertex[4];
        private readonly ushort[] indices = new ushort[6] { 0, 1, 2, 2, 3, 0 };
        
        private bool disposed;

        public ImuiElement(IImuiElementDelegate elementDelegate)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            focusable = true;

            this.elementDelegate = elementDelegate;

            gui = new ImGui(this, this);
            renderScheduler = new ImuiGenericRenderingScheduler();
            textureRenderer = new ImDynamicRenderTexture();
            generateVisualContent = GenerateVisualContent;

            mouseEventsQueue = new ImCircularBuffer<ImMouseEvent>(36);
            keyboardEvents = new ImCircularBuffer<ImKeyboardEvent>(8);
            nextKeyboardEvents = new ImCircularBuffer<ImKeyboardEvent>(8);

            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<WheelEvent>(OnMouseWheel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<KeyUpEvent>(OnKeyUp);
            
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        public void DoFrame(double time)
        {
            deltaTime = lastUpdate == 0 ? 0.0f : (float)(time - lastUpdate);
            globalTime = time;
            
            if (!UpdateThrottle())
            {
                return;
            }
            
            ImProfiler.BeginSample("ImuiElement.DoFrame");
            
            gui.BeginFrame();

            elementDelegate.Draw(gui);

            gui.EndFrame();
            gui.Render();

            MarkDirtyRepaint();
            
            ImProfiler.EndSample();
        }

        private bool UpdateThrottle()
        {
            if (!Throttle || contentDirty)
            {
                contentDirty = false;
                return true;
            }
            
            var timePassed = globalTime - lastUpdate;
            var anyEvents = mouseEventsQueue.Count > 0 || nextKeyboardEvents.Count > 0;
            var preferredRefreshRate = gui.GetThrottleHint().PreferredRefreshRate;
            var refreshDelay = preferredRefreshRate <= 0 ? DEFAULT_THROTTLE_REFRESH_DELAY : 1.0d / preferredRefreshRate;
            
            if (timePassed < refreshDelay && !anyEvents && eventsCooldown == 0)
            {
                return false;
            }

            lastUpdate = globalTime;
            
            if (anyEvents)
            {
                // (artem-s): draw at least N frames after any event without throttling
                eventsCooldown = THROTTLE_COOLDOWN_FRAMES;
            }
            else
            {
                eventsCooldown = eventsCooldown > 0 ? eventsCooldown - 1 : eventsCooldown;
            }

            return true;
        }

        public void Pull()
        {
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

            WasMouseDownThisFrame = mouseEvent.Type == ImMouseEventType.Down;
        }

        public Vector2 GetScreenSize()
        {
            return contentRect.size;
        }

        public float GetScale()
        {
            return 1.0f;
        }

        public Vector2Int SetupRenderTarget(CommandBuffer cmd)
        {
            var renderScale = PixelsPerPoint;
            var screenSize = GetScreenSize();
            var targetSize = textureRenderer
                .SetupRenderTarget(cmd, new Vector2Int((int)(screenSize.x * renderScale), (int)(screenSize.y * renderScale)), out _);

            return targetSize;
        }

        public void Schedule(IImuiRenderDelegate renderDelegate)
        {
            renderScheduler.Schedule(renderDelegate);
        }

        public ref readonly ImKeyboardEvent GetKeyboardEvent(int index)
        {
            return ref keyboardEvents[index];
        }

        public void UseKeyboardEvent(int index)
        {
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

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            var mesh = context.Allocate(4, 6, textureRenderer.Texture);
            var uv = mesh.uvRegion;
            var rect = contentRect;
            var color = (Color32)Color.white;

            ref var v0 = ref vertices[0];
            v0.position.x = rect.xMin;
            v0.position.y = rect.yMax;
            v0.position.z = Vertex.nearZ;
            v0.uv.x = uv.xMin;
            v0.uv.y = uv.yMin;
            v0.tint = color;

            ref var v1 = ref vertices[1];
            v1.position.x = rect.xMin;
            v1.position.y = rect.yMin;
            v1.position.z = Vertex.nearZ;
            v1.uv.x = uv.xMin;
            v1.uv.y = uv.yMax;
            v1.tint = color;

            ref var v2 = ref vertices[2];
            v2.position.x = rect.xMax;
            v2.position.y = rect.yMin;
            v2.position.z = Vertex.nearZ;
            v2.uv.x = uv.xMax;
            v2.uv.y = uv.yMax;
            v2.tint = color;

            ref var v3 = ref vertices[3];
            v3.position.x = rect.xMax;
            v3.position.y = rect.yMax;
            v3.position.z = Vertex.nearZ;
            v3.uv.x = uv.xMax;
            v3.uv.y = uv.yMin;
            v3.tint = color;

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            contentDirty = true;
        }

        private void OnKeyUp(KeyUpEvent e)
        {
            var evt = new ImKeyboardEvent(ImKeyboardEventType.Up, e.keyCode, e.modifiers, e.character);
            nextKeyboardEvents.PushFront(evt);
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            var evt = new ImKeyboardEvent(ImKeyboardEventType.Down, e.keyCode, e.modifiers, e.character);
            nextKeyboardEvents.PushFront(evt);
        }

        private void OnMouseWheel(WheelEvent e)
        {
            static bool CanMergeWithCurrent(in ImMouseEvent current, EventModifiers modifiers)
            {
                return current.Type == ImMouseEventType.Scroll &&
                       current.Modifiers == modifiers;
            }

            var delta = e.mouseDelta;
            delta.x = -delta.x;

            if (mouseEventsQueue.TryPeekFront(out var currentEvent) && CanMergeWithCurrent(in currentEvent, e.modifiers))
            {
                delta += currentEvent.Delta;
                mouseEventsQueue.TryPopFront(out _);
            }

            var evt = new ImMouseEvent(ImMouseEventType.Scroll, e.button, e.modifiers, delta, MOUSE_DEVICE);
            mouseEventsQueue.PushFront(evt);
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            var evt = new ImMouseEvent(ImMouseEventType.Up, e.button, e.modifiers, e.mouseDelta, MOUSE_DEVICE);
            mouseEventsQueue.PushFront(evt);
            possibleDrag = false;
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            var evt = new ImMouseEvent(ImMouseEventType.Down, e.button, e.modifiers, e.mouseDelta, MOUSE_DEVICE, e.clickCount);
            mouseEventsQueue.PushFront(evt);
            mouseDownPosition = mousePosition;
            possibleDrag = true;
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            static bool TryMerge(ImMouseEvent e0, ImMouseEvent e1, out ImMouseEvent merged)
            {
                if (e0.Type != e1.Type || e0.Button != e1.Button || e0.Modifiers != e1.Modifiers)
                {
                    merged = default;
                    return false;
                }

                merged = new ImMouseEvent(e0.Type, e0.Button, e0.Modifiers, e0.Delta + e1.Delta, MOUSE_DEVICE, e0.Count);
                return true;
            }

            Focus();

            var nextPosition = e.localMousePosition;
            nextPosition.y = contentRect.height - nextPosition.y;
            var delta = nextPosition - mousePosition;
            mousePosition = nextPosition;

            var distance = Vector2.Distance(mouseDownPosition, mousePosition);
            var type = e.pressedButtons != 0 && e.button == 0 && ((possibleDrag && distance > DRAG_DISTANCE_THRESHOLD) || wasDragging)
                ? ImMouseEventType.Drag
                : ImMouseEventType.Move;

            if (type == ImMouseEventType.Drag && !wasDragging)
            {
                mouseEventsQueue.PushFront(new ImMouseEvent(ImMouseEventType.BeginDrag, e.button, e.modifiers, delta, MOUSE_DEVICE));
            }

            var evt = new ImMouseEvent(type, e.button, e.modifiers, delta, MOUSE_DEVICE);

            if (mouseEventsQueue.Count > 0 && TryMerge(mouseEventsQueue.Get(mouseEventsQueue.Head), evt, out var merged))
            {
                ref var head = ref mouseEventsQueue.Get(mouseEventsQueue.Head);
                head = merged;
            }
            else
            {
                mouseEventsQueue.PushFront(evt);
            }

            wasDragging = type == ImMouseEventType.Drag;
        }

        public void RequestTouchKeyboard(uint owner, ReadOnlySpan<char> text, ImTouchKeyboardSettings settings) { }

        public void UseRaycaster(IImuiInput.RaycasterDelegate raycaster) { }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            gui.Dispose();
            renderScheduler.Dispose();
            textureRenderer.Dispose();

            disposed = true;
        }
    }
}