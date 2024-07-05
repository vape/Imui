using System;
using Imui.IO;
using Imui.Rendering;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    // TODO (artem-s):
    // * Check for per-frame allocations and overall optimization
    
    public class ImGui : IDisposable
    {
        private const int CONTROL_IDS_CAPACITY = 32;
        
        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;
        
        private const float UI_SCALE_MIN = 0.05f;
        private const float UI_SCALE_MAX = 16.0f;

        private const int FLOATING_CONTROLS_CAPACITY = 128;
        private const int HOVERED_GROUPS_CAPACITY = 16;
        private const int SCOPES_STACK_CAPACITY = 32;

        private const int DEFAULT_STORAGE_CAPACITY = 2048;

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

            public FrameData(int hoveredGroupsCapacity, int floatingControlsCapacity)
            {
                HoveredControl = default;
                HoveredGroups = new ImDynamicArray<ControlData>(hoveredGroupsCapacity);
                FloatingControls = new ImDynamicArray<ImRect>(floatingControlsCapacity);
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
            get
            {
                return uiScale;
            }
            set
            {
                uiScale = Mathf.Clamp(value, UI_SCALE_MIN, UI_SCALE_MAX);
            }
        }
        
        public readonly ImMeshBuffer MeshBuffer;
        public readonly ImMeshRenderer MeshRenderer;
        public readonly ImMeshDrawer MeshDrawer;
        public readonly ImTextDrawer TextDrawer;
        public readonly ImCanvas Canvas;
        public readonly ImLayout Layout;
        public readonly ImStorage Storage;
        public readonly ImWindowManager WindowManager;
        public readonly IImInputBackend Input;
        public readonly IImRenderingBackend Renderer;
        
        internal FrameData nextFrameData;
        internal FrameData frameData;

        private float uiScale = 1.0f;
        private ImDynamicArray<ControlId> idsStack;
        private ImDynamicArray<uint> scopes;
        private uint activeControl;
        private ImControlFlag activeControlFlag;
        
        private bool disposed;
        
        public ImGui(IImRenderingBackend renderer, IImInputBackend input)
        {
            MeshBuffer = new ImMeshBuffer(INIT_MESHES_COUNT, INIT_VERTICES_COUNT, INIT_INDICES_COUNT);
            MeshDrawer = new ImMeshDrawer(MeshBuffer);
            TextDrawer = new ImTextDrawer(MeshBuffer);
            Canvas = new ImCanvas(MeshDrawer, TextDrawer);
            MeshRenderer = new ImMeshRenderer();
            Layout = new ImLayout();
            Storage = new ImStorage(DEFAULT_STORAGE_CAPACITY);
            WindowManager = new ImWindowManager();
            Input = input;
            Renderer = renderer;

            frameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            nextFrameData = new FrameData(HOVERED_GROUPS_CAPACITY, FLOATING_CONTROLS_CAPACITY);
            idsStack = new ImDynamicArray<ControlId>(CONTROL_IDS_CAPACITY);
            scopes = new ImDynamicArray<uint>(SCOPES_STACK_CAPACITY);
            
            Input.SetRaycaster(Raycast);
        }
        
        public void BeginFrame()
        {
            idsStack.Clear(false);

            (nextFrameData, frameData) = (frameData, nextFrameData);
            nextFrameData.Clear();

            var screenRect = Renderer.GetScreenRect();
            var scaledScreenSize = screenRect.size / uiScale;

            Input.SetScale(UiScale);
            Input.Pull();

            Canvas.SetScreen(scaledScreenSize);
            Canvas.Clear();
            Canvas.PushSettings(Canvas.CreateDefaultSettings());
            
            WindowManager.SetScreenSize(scaledScreenSize);
            
            Layout.Push(ImAxis.Vertical, new ImRect(Vector2.zero, scaledScreenSize));
            
            idsStack.Push(new ControlId(ImHash.Get("root", 0)));
        }

        public void EndFrame()
        {
            idsStack.Pop();
            
            Layout.Pop();
            
            Canvas.PopSettings();
            
            Storage.CollectAndCompact();
        }

        public void BeginScope(uint id)
        {
            scopes.Push(id);
        }

        public uint GetScope()
        {
            return scopes.Peek();
        }

        public void EndScope(out uint id)
        {
            id = scopes.Pop();
        }

        public uint PushId()
        {
            var id = GetNextControlId();
            idsStack.Push(new ControlId(id));
            return id;
        }

        public uint PushId(uint id)
        {
            var newId = GetControlId(id);
            idsStack.Push(new ControlId(newId));
            return newId;
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

        public uint GetNextControlId()
        {
            ref var parent = ref idsStack.Peek();
            return ImHash.Get(++parent.Gen, parent.Id);
        }

        public uint GetControlId(in ReadOnlySpan<char> name)
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

        public void RegisterRaycastTarget(ImRect rect)
        {
            nextFrameData.FloatingControls.Add(rect);
        }
        
        public void RegisterControl(uint controlId, ImRect rect)
        {
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
        
        public void Render()
        {
            var renderCmd = Renderer.CreateCommandBuffer();
            var screenSize = Renderer.GetScreenRect().size;
            Renderer.SetupRenderTarget(renderCmd);
            MeshRenderer.Render(renderCmd, MeshBuffer, screenSize, UiScale);
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
