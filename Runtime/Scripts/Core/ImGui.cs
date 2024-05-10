using System;
using Imui.Rendering;
using Imui.Rendering.Backend;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Rendering;
using MeshRenderer = Imui.Rendering.MeshRenderer;

namespace Imui.Core
{
    // TODO (artem-s):
    // * Check for per-frame allocations and overall optimization
    
    public class ImGui : IDisposable, IImuiRenderer
    {
        private const int CONTROL_IDS_CAPACITY = 32;
        
        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;

        private const float UI_SCALE_MIN = 0.05f;
        private const float UI_SCALE_MAX = 16.0f;

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
            public DynamicArray<ControlData> HoveredGroups;

            public FrameData(int hoveredGroupsCapacity)
            {
                HoveredControl = default;
                HoveredGroups = new DynamicArray<ControlData>(hoveredGroupsCapacity);
            }
            
            public void Clear()
            {
                HoveredControl = default;
                HoveredControl.Order = ImCanvas.DEFAULT_ORDER;
                HoveredGroups.Clear(false);
            }
        }
        
        public float Scale
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

        public uint ActiveControl;

        public readonly MeshBuffer MeshBuffer;
        public readonly MeshRenderer Renderer;
        public readonly MeshDrawer MeshDrawer;
        public readonly TextDrawer TextDrawer;
        public readonly ImCanvas Canvas;
        public readonly ImInput Input;
        public readonly ImLayout Layout;
        public readonly ImStorage Storage;
        public readonly ImWindowManager WindowManager;
        
        internal FrameData nextFrameData;
        internal FrameData frameData;
        private DynamicArray<ControlId> idsStack;
        private DynamicArray<uint> scopes;
        
        private float uiScale = 1.0f;
        private Vector2 fbSize = Vector2.zero;
        
        private bool disposed;
        
        public ImGui()
        {
            MeshBuffer = new MeshBuffer(INIT_MESHES_COUNT, INIT_VERTICES_COUNT, INIT_INDICES_COUNT);
            MeshDrawer = new MeshDrawer(MeshBuffer);
            TextDrawer = new TextDrawer(MeshBuffer);
            Canvas = new ImCanvas(MeshDrawer, TextDrawer);
            Renderer = new MeshRenderer();
            Input = new ImInputLegacy();
            Layout = new ImLayout();
            Storage = new ImStorage(DEFAULT_STORAGE_CAPACITY);
            WindowManager = new ImWindowManager();

            frameData = new FrameData(HOVERED_GROUPS_CAPACITY);
            nextFrameData = new FrameData(HOVERED_GROUPS_CAPACITY);
            idsStack = new DynamicArray<ControlId>(CONTROL_IDS_CAPACITY);
            scopes = new DynamicArray<uint>(SCOPES_STACK_CAPACITY);
        }

        public void SetFont(Font font)
        {
            TextDrawer.LoadFont(font);
        }
        
        public void BeginFrame()
        {
            idsStack.Clear(false);
            
            // ReSharper disable once SwapViaDeconstruction
            frameData = nextFrameData;
            nextFrameData.Clear();
            
            Input.SetScale(uiScale);
            Input.Pull();
            
            Canvas.SetScreen(fbSize, uiScale);
            Canvas.Clear();
            Canvas.PushMeshSettings(Canvas.CreateDefaultMeshSettings());

            Layout.Push(new ImRect(Vector2.zero, fbSize / uiScale), ImAxis.Vertical);
            Layout.SetFlags(ImLayoutFlag.Root);

            idsStack.Push(new ControlId(ImHash.Get("root", 0)));
        }

        public void EndFrame()
        {
            idsStack.Pop();
            
            Layout.Pop();
            
            Canvas.PopMeshSettings();
            
            Storage.CollectAndCompact();
        }

        public void BeginScope(uint id)
        {
            scopes.Push(id);
        }

        public void EndScope(out uint id)
        {
            id = scopes.Pop();
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

        public uint GetHoveredControl()
        {
            return frameData.HoveredControl.Id;
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
        
        public void HandleControl(uint controlId, ImRect rect)
        {
            ref readonly var meshProperties = ref Canvas.GetActiveMeshSettingsRef();
            
            if (meshProperties.ClipRect.Enabled && !meshProperties.ClipRect.Rect.Contains(Input.MousePosition))
            {
                return;
            }

            if (meshProperties.Order >= nextFrameData.HoveredControl.Order && rect.Contains(Input.MousePosition))
            {
                nextFrameData.HoveredControl.Id = controlId;
                nextFrameData.HoveredControl.Order = meshProperties.Order;
                nextFrameData.HoveredControl.Rect = rect;
            }
        }

        public void HandleGroup(uint controlId, ImRect rect)
        {
            ref readonly var meshProperties = ref Canvas.GetActiveMeshSettingsRef();
            
            if (meshProperties.ClipRect.Enabled && !meshProperties.ClipRect.Rect.Contains(Input.MousePosition))
            {
                return;
            }

            if (rect.Contains(Input.MousePosition))
            {
                nextFrameData.HoveredGroups.Add(new ControlData()
                {
                    Id = controlId,
                    Order = meshProperties.Order,
                    Rect = rect
                });
            }
        }

        // TODO (artem-s): move to begin frame
        void IImuiRenderer.OnFrameBufferSizeChanged(Vector2 size)
        {
            fbSize = size;
        }
        
        void IImuiRenderer.Setup(CommandBuffer cmd)
        {
            Canvas.Setup(cmd);
        }

        void IImuiRenderer.Render(CommandBuffer cmd)
        {
            Renderer.Render(cmd, MeshBuffer, fbSize, uiScale);
        }
        
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            Canvas.Dispose();
            TextDrawer.Dispose();
            Renderer.Dispose();
            Storage.Dispose();
            Input.Dispose();
            
            disposed = true;
        }
    }
}
