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
        
        public MeshRenderer Renderer;
        public MeshDrawer Drawer;
        public ImCanvas Canvas;

        private float uiScale = 1.0f;
        private Vector2 fbSize = Vector2.zero;
        
        private bool disposed;
        
        public ImGui()
        {
            Drawer = new MeshDrawer();
            Canvas = new ImCanvas(Drawer);
            Renderer = new MeshRenderer();
        }
        
        public void BeginFrame()
        {
            Canvas.SetFrame(fbSize, uiScale);
            Canvas.Begin();
        }

        public void EndFrame()
        {
            Canvas.End();
        }

        void IImuiRenderer.OnFrameBufferSizeChanged(Vector2 size)
        {
            fbSize = size;
        }
        
        void IImuiRenderer.Setup(CommandBuffer cmd)
        {
            Canvas.SetupAtlas(cmd);
        }

        void IImuiRenderer.Render(CommandBuffer cmd)
        {
            Renderer.Render(cmd, Drawer.Buffer, fbSize, uiScale);
        }
        
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            Canvas?.Dispose();
            Canvas = null;
            
            Renderer?.Dispose();
            Renderer = null;
            
            disposed = true;
        }
    }
}