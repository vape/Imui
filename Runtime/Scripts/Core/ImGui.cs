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
        private const float UI_SCALE_MAX = 4.0f;
        
        public Vector2 Scale
        {
            get
            {
                return uiScale;
            }
            set
            {
                var x = Mathf.Clamp(value.x, UI_SCALE_MIN, UI_SCALE_MAX);
                var y = Mathf.Clamp(value.y, UI_SCALE_MIN, UI_SCALE_MAX);
                
                uiScale = new Vector2(x, y);
            }
        }
        
        public MeshRenderer Renderer;
        public MeshDrawer Drawer;
        public ImCanvas Canvas;

        private Vector2 uiScale = Vector2.one;
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