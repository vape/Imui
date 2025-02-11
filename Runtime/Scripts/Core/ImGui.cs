using System;
using System.Runtime.CompilerServices;
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

    public unsafe class ImGui: IDisposable
    {
        private const int CONTROL_IDS_STACK_CAPACITY = 32;

        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;

        private const int FLOATING_CONTROLS_CAPACITY = 128;
        private const int HOVERED_GROUPS_CAPACITY = 16;
        private const int READONLY_STACK_CAPACITY = 4;
        private const int CONTROL_SCOPE_STACK_CAPACITY = 64;
        private const int STYLE_SCOPE_STACK_CAPACITY = 16;

        private const int INITIAL_STORAGE_ENTRIES = 256;
        private const int INITIAL_STORAGE_CAPACITY = 1024 * 1024;
        private const int DEFAULT_ARENA_CAPACITY = 1024 * 1024;

        private struct StyleProp
        {
            public int Offset;
            public void* Original;

            public StyleProp(int offset, void* original)
            {
                Offset = offset;
                Original = original;
            }
        }

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

        internal struct ImControlScope
        {
            public uint Id;
            public int Type;

            internal void* Ptr;
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
        public readonly IImuiInput Input;
        public readonly IImuiRenderer Renderer;
        public readonly ImFormatter Formatter;

        public ImStyleSheet Style;

        // ReSharper disable InconsistentNaming
        internal FrameData nextFrameData;
        internal FrameData frameData;
        // ReSharper restore InconsistentNaming

        private ImDynamicArray<ControlId> idsStack;
        private ImDynamicArray<bool> readOnlyStack;
        private uint activeControl;
        private ImControlFlag activeControlFlag;
        private uint lastControl;
        private ImRect lastControlRect;
        private ImDynamicArray<ImControlScope> controlScopesStack;
        private ImDynamicArray<StyleProp> styleStack;

        private bool disposed;

        public ImGui(IImuiRenderer renderer, IImuiInput input)
        {
            MeshBuffer = new ImMeshBuffer(INIT_MESHES_COUNT, INIT_VERTICES_COUNT, INIT_INDICES_COUNT);
            MeshDrawer = new ImMeshDrawer(MeshBuffer);
            TextDrawer = new ImTextDrawer(MeshBuffer);
            Arena = new ImArena(DEFAULT_ARENA_CAPACITY);
            Canvas = new ImCanvas(MeshDrawer, TextDrawer, Arena);
            MeshRenderer = new ImMeshRenderer();
            Layout = new ImLayout();
            Storage = new ImStorage(INITIAL_STORAGE_ENTRIES, INITIAL_STORAGE_CAPACITY);
            WindowManager = new ImWindowManager();
            Input = input;
            Renderer = renderer;
            Formatter = new ImFormatter(Arena);

            frameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            nextFrameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            idsStack = new ImDynamicArray<ControlId>(CONTROL_IDS_STACK_CAPACITY);
            readOnlyStack = new ImDynamicArray<bool>(READONLY_STACK_CAPACITY);
            controlScopesStack = new ImDynamicArray<ImControlScope>(CONTROL_SCOPE_STACK_CAPACITY);
            styleStack = new ImDynamicArray<StyleProp>(STYLE_SCOPE_STACK_CAPACITY);

            Input.UseRaycaster(Raycast);
            SetTheme(ImThemeBuiltin.Light());
        }

        public void BeginFrame()
        {
            if (!TextDrawer.IsFontLoaded)
            {
                LoadDefaultFont();
            }
            
            Arena.Clear();

            idsStack.Clear(false);

            (nextFrameData, frameData) = (frameData, nextFrameData);
            nextFrameData.Clear();

            var uiScale = Renderer.GetScale();
            var scaledTargetSize = Renderer.GetScreenSize() / Renderer.GetScale();

            Input.Pull();

            Canvas.Clear();
            Canvas.ConfigureScreen(scaledTargetSize, uiScale);
            Canvas.PushSettings(Canvas.CreateDefaultSettings());

            Layout.Push(ImAxis.Vertical, new ImRect(Vector2.zero, scaledTargetSize));

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

            Storage.FindUnused();
            Storage.Collect();

            WindowManager.HandleFrameEnded();
        }

        public void LoadDefaultFont()
        {
            TextDrawer.LoadFont(Resources.Load<Font>("Imui/FiraMono-Regular"));
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

        public void PushStyle<T>(ref T style) where T: unmanaged
        {
            PushStyle<T>(ref style, in style);
        }

        public void PushStyle<T>(ref T style, in T value) where T: unmanaged
        {
            var original = Arena.AllocUnsafe<T>();
            *original = style;
            style = value;

            fixed (void* start = &Style)
            fixed (void* prop = &style)
            {
                var offset = (int)((byte*)prop - (byte*)start);

                ImAssert.IsTrue(offset > 0, "offset > 0");
                ImAssert.IsTrue(offset < sizeof(ImStyleSheet), "offset < sizeof(ImStyleSheet)");

                styleStack.Push(new StyleProp(offset, original));
            }
        }

        public void PopStyle<T>() where T: unmanaged
        {
            var prop = styleStack.Pop();
            fixed (void* start = &Style)
            {
                *(T*)((byte*)start + prop.Offset) = *(T*)prop.Original;
            }
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

        public uint GetControlId(uint id, uint parent)
        {
            return ImHash.Get(id, parent);
        }

        public uint GetControlId(ReadOnlySpan<char> name, uint parent)
        {
            return ImHash.Get(name, parent);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TState BeginScope<TState>(uint id, TState @default = default) where TState: unmanaged => ref *BeginScopeUnsafe(id, @default);

        public unsafe TState* BeginScopeUnsafe<TState>(uint id, TState @default = default) where TState: unmanaged
        {
            var ptr = Storage.GetUnsafe(id, @default);
            var scope = new ImControlScope()
            {
                Id = id,
                Type = typeof(TState).GetHashCode(),
                Ptr = ptr
            };

            controlScopesStack.Push(in scope);

            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TState EndScope<TState>() where TState: unmanaged => ref *EndScopeUnsafe<TState>(out _);

        public unsafe ref TState EndScope<TState>(out uint id) where TState: unmanaged => ref *EndScopeUnsafe<TState>(out id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TState* EndScopeUnsafe<TState>() where TState: unmanaged => EndScopeUnsafe<TState>(out _);

        public unsafe TState* EndScopeUnsafe<TState>(out uint id) where TState: unmanaged
        {
            ref var scope = ref FindControlScopeOrFail<TState>(out var index);

            id = scope.Id;
            var ptr = (TState*)scope.Ptr;
            controlScopesStack.RemoveAtFast(index);

            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TState GetCurrentScope<TState>() where TState: unmanaged => ref *GetCurrentScopeUnsafe<TState>(out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TState GetCurrentScope<TState>(out uint id) where TState: unmanaged => ref *GetCurrentScopeUnsafe<TState>(out id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe TState* GetCurrentScopeUnsafe<TState>() where TState: unmanaged => GetCurrentScopeUnsafe<TState>(out _);

        public unsafe TState* GetCurrentScopeUnsafe<TState>(out uint id) where TState: unmanaged
        {
            ref var scope = ref FindControlScopeOrFail<TState>(out _);

            id = scope.Id;
            var ptr = (TState*)scope.Ptr;

            return ptr;
        }

        public unsafe bool TryGetCurrentScopeUnsafe<TState>(out TState* state) where TState: unmanaged
        {
            if (!TryFindControlScope<TState>(out var index))
            {
                state = default;
                return false;
            }

            state = (TState*)controlScopesStack.Array[index].Ptr;
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
            ImProfiler.BeginSample("ImGui.Render");

            nextFrameData.VerticesCount = MeshDrawer.buffer.VerticesCount;
            nextFrameData.IndicesCount = MeshDrawer.buffer.IndicesCount;
            nextFrameData.ArenaSize = Arena.Size;

            var renderCmd = Renderer.CreateCommandBuffer();
            var screenSize = Renderer.GetScreenSize();
            var uiScale = Renderer.GetScale();
            var targetSize = Renderer.SetupRenderTarget(renderCmd);

            MeshRenderer.Render(renderCmd, MeshBuffer, screenSize, uiScale, targetSize);
            Renderer.Execute(renderCmd);
            Renderer.ReleaseCommandBuffer(renderCmd);

            ImProfiler.EndSample();
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

            Input.UseRaycaster(null);
            Canvas.Dispose();
            TextDrawer.Dispose();
            MeshRenderer.Dispose();
            Storage.Dispose();
            Arena.Dispose();

            disposed = true;
        }
    }
}