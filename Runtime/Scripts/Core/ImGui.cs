using System;
using Imui.IO;
using Imui.Rendering;
using Imui.Style;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    [Flags]
    public enum ImControlFlag
    {
        None = 0,
        Draggable = 1 << 0
    }
    
    public struct ImControlScope
    {
        public uint Id;
        public int Type;
    }

    public class ImGui : IDisposable
    {
        private const int CONTROL_IDS_STACK_CAPACITY = 32;

        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;

        private const float UI_SCALE_MIN = 0.05f;
        private const float UI_SCALE_MAX = 16.0f;

        private const int FLOATING_CONTROLS_CAPACITY = 128;
        private const int HOVERED_GROUPS_CAPACITY = 16;
        private const int READONLY_STACK_CAPACITY = 4;
        private const int CONTROL_SCOPE_STACK_CAPACITY = 64;

        private const int DEFAULT_STORAGE_CAPACITY = 2048;
        private const int DEFAULT_ARENA_CAPACITY = 1024 * 1024;

        private struct ControlId
        {
            public uint Id;
            public int Gen;

            public ControlId(uint id)
            {
                Id = id;
                Gen = 0;
            }
        }

        internal struct ControlData
        {
            public uint Id;
            public int Order;
            public ImRect Rect;
        }

        internal struct FrameData
        {
            public ControlData HoveredControl;
            public ImDynamicArray<ControlData> HoveredGroups;
            public ImDynamicArray<ImRect> FloatingControls;
            public int VerticesCount;
            public int IndicesCount;
            public int ArenaSize;

            public FrameData(int hoveredGroupsCapacity, int floatingControlsCapacity)
            {
                HoveredControl = default;
                HoveredGroups = new ImDynamicArray<ControlData>(hoveredGroupsCapacity);
                FloatingControls = new ImDynamicArray<ImRect>(floatingControlsCapacity);
                IndicesCount = 0;
                VerticesCount = 0;
                ArenaSize = 0;
            }

            public void Clear()
            {
                HoveredControl = default;
                HoveredControl.Order = ImCanvas.DEFAULT_ORDER;
                HoveredGroups.Clear(false);
                FloatingControls.Clear(false);
            }
        }

        public float UiScale
        {
            get => uiScale;
            set => uiScale = Mathf.Clamp(value, UI_SCALE_MIN, UI_SCALE_MAX);
        }

        public bool IsReadOnly => readOnlyStack.TryPeek(@default: false);
        public uint LastControl => lastControl;
        public ImRect LastControlRect => lastControlRect;

        public readonly ImMeshBuffer MeshBuffer;
        public readonly ImMeshRenderer MeshRenderer;
        public readonly ImMeshDrawer MeshDrawer;
        public readonly ImTextDrawer TextDrawer;
        public readonly ImArena Arena;
        public readonly ImCanvas Canvas;
        public readonly ImLayout Layout;
        public readonly ImStorage Storage;
        public readonly ImWindowManager WindowManager;
        public readonly IImInputBackend Input;
        public readonly IImRenderingBackend Renderer;
        public readonly ImFormatter Formatter;

        public ImStyleSheet Style;

        // ReSharper disable InconsistentNaming
        internal FrameData nextFrameData;
        internal FrameData frameData;
        // ReSharper restore InconsistentNaming

        private float uiScale = 1.0f;
        private ImDynamicArray<ControlId> idsStack;
        private ImDynamicArray<bool> readOnlyStack;
        private uint activeControl;
        private ImControlFlag activeControlFlag;
        private uint lastControl;
        private ImRect lastControlRect;
        private ImDynamicArray<ImControlScope> controlScopesStack;

        private bool disposed;

        public ImGui(IImRenderingBackend renderer, IImInputBackend input)
        {
            MeshBuffer = new ImMeshBuffer(INIT_MESHES_COUNT, INIT_VERTICES_COUNT, INIT_INDICES_COUNT);
            MeshDrawer = new ImMeshDrawer(MeshBuffer);
            TextDrawer = new ImTextDrawer(MeshBuffer);
            Arena = new ImArena(DEFAULT_ARENA_CAPACITY);
            Canvas = new ImCanvas(MeshDrawer, TextDrawer, Arena);
            MeshRenderer = new ImMeshRenderer();
            Layout = new ImLayout();
            Storage = new ImStorage(DEFAULT_STORAGE_CAPACITY);
            WindowManager = new ImWindowManager();
            Input = input;
            Renderer = renderer;
            Formatter = new ImFormatter(Arena);

            frameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            nextFrameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            idsStack = new ImDynamicArray<ControlId>(CONTROL_IDS_STACK_CAPACITY);
            readOnlyStack = new ImDynamicArray<bool>(READONLY_STACK_CAPACITY);
            controlScopesStack = new ImDynamicArray<ImControlScope>(CONTROL_SCOPE_STACK_CAPACITY);

            Input.SetRaycaster(Raycast);
            SetTheme(ImThemeBuiltin.Light());
        }

        public void BeginFrame()
        {
            Arena.Clear();

            idsStack.Clear(false);

            (nextFrameData, frameData) = (frameData, nextFrameData);
            nextFrameData.Clear();

            var screenRect = Renderer.GetScreenRect();
            var scaledScreenSize = screenRect.size / uiScale;

            Input.SetScale(UiScale);
            Input.Pull();

            Canvas.SetScreen(scaledScreenSize, uiScale);
            Canvas.Clear();
            Canvas.PushSettings(Canvas.CreateDefaultSettings());

            WindowManager.SetScreenSize(scaledScreenSize);

            Layout.Push(ImAxis.Vertical, new ImRect(Vector2.zero, scaledScreenSize));

            idsStack.Push(new ControlId(ImHash.Get("root", 0)));
        }

        public void EndFrame()
        {
            if (controlScopesStack.Count > 0)
            {
                Debug.LogError($"There are still {controlScopesStack.Count} control scopes on the stack. Check Begin(X)/End(X) calls.");
                controlScopesStack.Clear(false);
            }

            idsStack.Pop();

            if (idsStack.Count > 0)
            {
                Debug.LogError($"There are still {idsStack.Count} ids on the stack. Check PushId/PopId calls.");
                idsStack.Clear(false);
            }

            Layout.Pop();

            Canvas.PopSettings();

            Storage.CollectAndCompact();

            WindowManager.HandleFrameEnded();
        }

        public void BeginReadOnly(bool isReadOnly)
        {
            readOnlyStack.Push(isReadOnly);

            if (isReadOnly)
            {
                Canvas.PushInvColorMul(1 - Style.Theme.ReadOnlyColorMultiplier);
            }
            else
            {
                Canvas.PushDefaultInvColorMul();
            }
        }

        public void EndReadOnly()
        {
            readOnlyStack.Pop();
            Canvas.PopInvColorMul();
        }

        public void PushId(uint id)
        {
            idsStack.Push(new ControlId(id));
        }

        public uint PushId(ReadOnlySpan<char> name)
        {
            var id = GetControlId(name);
            idsStack.Push(new ControlId(id));
            return id;
        }

        public uint PopId()
        {
            return idsStack.Pop().Id;
        }

        public uint PeekId()
        {
            return idsStack.Peek().Id;
        }

        public bool TryPeekId(out uint id)
        {
            if (idsStack.TryPeek(out var controlId))
            {
                id = controlId.Id;
                return true;
            }

            id = default;
            return false;
        }

        public uint GetNextControlId()
        {
            ref var parent = ref idsStack.Peek();
            return ImHash.Get(++parent.Gen, parent.Id);
        }

        public uint GetControlId(ReadOnlySpan<char> name)
        {
            ref var parent = ref idsStack.Peek();
            return ImHash.Get(name, parent.Id);
        }

        public uint GetControlId(uint id)
        {
            ref var parent = ref idsStack.Peek();
            return ImHash.Get(id, parent.Id);
        }

        public uint GetHoveredControl()
        {
            return frameData.HoveredControl.Id;
        }

        public uint GetActiveControl()
        {
            return activeControl;
        }

        public ImControlFlag GetActiveControlFlag()
        {
            return activeControlFlag;
        }

        public bool ActiveControlIs(ImControlFlag flag)
        {
            return (activeControlFlag & flag) == flag;
        }

        public void SetActiveControl(uint controlId, ImControlFlag flag = ImControlFlag.None)
        {
            activeControl = controlId;
            activeControlFlag = flag;
        }

        public void ResetActiveControl()
        {
            activeControl = default;
            activeControlFlag = default;
        }

        public bool IsControlActive(uint controlId)
        {
            return activeControl == controlId;
        }

        public bool IsControlHovered(uint controlId)
        {
            return frameData.HoveredControl.Id == controlId;
        }

        public bool IsGroupHovered(uint controlId)
        {
            for (int i = 0; i < frameData.HoveredGroups.Count; ++i)
            {
                if (frameData.HoveredGroups.Array[i].Id == controlId)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetTheme(ImTheme theme)
        {
            Style = ImStyleSheetBuilder.Build(theme);
        }

        public void RegisterRaycastTarget(ImRect rect)
        {
            nextFrameData.FloatingControls.Add(rect);
        }

        public void SetLastControl(uint id, ImRect rect)
        {
            lastControl = id;
            lastControlRect = rect;
        }

        public void RegisterControl(uint controlId, ImRect rect)
        {
            lastControl = controlId;
            lastControlRect = rect;

            ref readonly var meshProperties = ref Canvas.GetActiveSettings();

            if (meshProperties.ClipRect.Enabled && !meshProperties.ClipRect.Rect.Contains(Input.MousePosition))
            {
                return;
            }

            if (!WindowManager.IsDrawingWindow())
            {
                nextFrameData.FloatingControls.Add(rect);
            }

            if (meshProperties.Order >= nextFrameData.HoveredControl.Order && rect.Contains(Input.MousePosition))
            {
                nextFrameData.HoveredControl.Id = controlId;
                nextFrameData.HoveredControl.Order = meshProperties.Order;
                nextFrameData.HoveredControl.Rect = rect;
            }
        }

        public void RegisterGroup(uint controlId, ImRect rect)
        {
            ref readonly var meshProperties = ref Canvas.GetActiveSettings();

            if (meshProperties.ClipRect.Enabled && !meshProperties.ClipRect.Rect.Contains(Input.MousePosition))
            {
                return;
            }

            if (rect.Contains(Input.MousePosition))
            {
                var currentOrder = meshProperties.Order;

                for (int i = nextFrameData.HoveredGroups.Count - 1; i >= 0; --i)
                {
                    var order = nextFrameData.HoveredGroups.Array[i].Order;
                    if (order < currentOrder)
                    {
                        nextFrameData.HoveredGroups.RemoveAtFast(i);
                    }
                    else if (order > currentOrder)
                    {
                        return;
                    }
                }

                nextFrameData.HoveredGroups.Add(new ControlData()
                {
                    Id = controlId,
                    Order = meshProperties.Order,
                    Rect = rect
                });
            }
        }

        public unsafe ref TState PushControlScope<TState>(uint id, TState @default = default) where TState: unmanaged => ref *PushControlScopePtr(id, @default);

        public unsafe TState* PushControlScopePtr<TState>(uint id, TState @default = default) where TState: unmanaged
        {
            var stateReference = new ImControlScope() { Id = id, Type = typeof(TState).GetHashCode() };

            controlScopesStack.Push(in stateReference);

            return Storage.GetPtr(id, @default);
        }

        public unsafe ref TState PopControlScope<TState>() where TState: unmanaged => ref *PopControlScopePtr<TState>(out _);
        public unsafe ref TState PopControlScope<TState>(out uint id) where TState: unmanaged => ref *PopControlScopePtr<TState>(out id);

        public unsafe TState* PopControlScopePtr<TState>() where TState: unmanaged => PopControlScopePtr<TState>(out _);

        public unsafe TState* PopControlScopePtr<TState>(out uint id) where TState: unmanaged
        {
            ref var reference = ref FindControlScopeOrFail<TState>(out var index);

            id = reference.Id;
            controlScopesStack.RemoveAtFast(index);

            return Storage.GetPtr<TState>(id);
        }

        public unsafe ref TState PeekControlScope<TState>() where TState: unmanaged => ref *PeekControlScopePtr<TState>(out _);
        public unsafe ref TState PeekControlScope<TState>(out uint id) where TState: unmanaged => ref *PeekControlScopePtr<TState>(out id);

        public unsafe TState* PeekControlScopePtr<TState>() where TState: unmanaged => PeekControlScopePtr<TState>(out _);

        public unsafe TState* PeekControlScopePtr<TState>(out uint id) where TState: unmanaged
        {
            ref var reference = ref FindControlScopeOrFail<TState>(out _);

            id = reference.Id;
            return Storage.GetPtr<TState>(id);
        }

        public unsafe bool TryPeekControlScopePtr<TState>(out TState* state) where TState: unmanaged
        {
            if (!TryFindControlScope<TState>(out var index))
            {
                state = default;
                return false;
            }

            state = Storage.GetPtr<TState>(controlScopesStack.Array[index].Id);
            return true;
        }

        private ref ImControlScope FindControlScopeOrFail<T>(out int index)
        {
            if (TryFindControlScope<T>(out index))
            {
                return ref controlScopesStack.Array[index];
            }

            if (controlScopesStack.Count == 0)
            {
                throw new InvalidOperationException($"State stack is empty when trying to get state for type {typeof(T)}. Check for missing Begin call.");
            }
            else
            {
                throw new ArgumentException($"Failed to find state of type {typeof(T)}. Check for missing Begin call.");
            }
        }

        private bool TryFindControlScope<T>(out int index)
        {
            index = default;

            if (controlScopesStack.Count == 0)
            {
                return false;
            }

            var type = typeof(T).GetHashCode();
            index = controlScopesStack.Count;

            while (--index >= 0)
            {
                ref var reference = ref controlScopesStack.Array[index];
                if (reference.Type != type)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public void Render()
        {
            nextFrameData.VerticesCount = MeshDrawer.buffer.VerticesCount;
            nextFrameData.IndicesCount = MeshDrawer.buffer.IndicesCount;
            nextFrameData.ArenaSize = Arena.Size;

            var renderCmd = Renderer.CreateCommandBuffer();
            var screenSize = Renderer.GetScreenRect().size;
            var targetSize = Renderer.SetupRenderTarget(renderCmd);

            MeshRenderer.Render(renderCmd, MeshBuffer, screenSize, UiScale, targetSize);
            Renderer.Execute(renderCmd);
            Renderer.ReleaseCommandBuffer(renderCmd);
        }

        public bool Raycast(float x, float y)
        {
            // (artem-s): I don't really like this approach of checking whether we should capture input,
            // but for now I can't come up with something better given that raycast is happening before we render frame
            // with actual cursor position

            var result = WindowManager.Raycast(x, y);
            if (!result)
            {
                for (int i = 0; i < frameData.FloatingControls.Count; ++i)
                {
                    if (frameData.FloatingControls.Array[i].Contains(x, y))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Input.SetRaycaster(null);
            Canvas.Dispose();
            TextDrawer.Dispose();
            MeshRenderer.Dispose();
            Storage.Dispose();

            disposed = true;
        }
    }
}