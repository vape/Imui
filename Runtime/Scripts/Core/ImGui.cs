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
        public MeshRenderer Renderer;
        public MeshDrawer Drawer;
        public ImCanvas Canvas;

        private bool disposed;
        
        public ImGui()
        {
            Drawer = new MeshDrawer();
            Canvas = new ImCanvas(Drawer);
            Renderer = new MeshRenderer();
        }

        public Rect GetScreen()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
        
        public void Clear()
        {
            Canvas.Clear();
        }
        
        public void Setup(CommandBuffer cmd)
        {
            Canvas.Setup(cmd);
        }

        public void Render(CommandBuffer cmd)
        {
            Renderer.Render(cmd, GetScreen(), Drawer.Buffer);
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