using System;
using Imui.Rendering;
using Imui.Rendering.Backend;
using UnityEngine;
using UnityEngine.Rendering;
using MeshRenderer = Imui.Rendering.MeshRenderer;

namespace Imui.Core
{
    public class ImGui : IDisposable, IImuiRenderer
    {
        private const int INIT_MESHES_COUNT = 1024 / 2;
        private const int INIT_VERTICES_COUNT = 1024 * 16;
        private const int INIT_INDICES_COUNT = INIT_VERTICES_COUNT * 3;

        private const float UI_SCALE_MIN = 0.05f;
        private const float UI_SCALE_MAX = 16.0f;
        
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

        public readonly MeshBuffer MeshBuffer;
        public readonly MeshRenderer Renderer;
        public readonly MeshDrawer MeshDrawer;
        public readonly TextDrawer TextDrawer;
        public readonly ImCanvas Canvas;

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
        }

        public void SetFont(Font font)
        {
            TextDrawer.LoadFont(font);
        }
        
        public void BeginFrame()
        {
            Canvas.SetScreen(fbSize, uiScale);
            Canvas.Clear();
            Canvas.PushMeshSettings(Canvas.CreateDefaultMeshSettings());
        }

        public void EndFrame()
        {
            Canvas.PopMeshSettings();
        }

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
            Renderer.Dispose();
            
            disposed = true;
        }
    }
}