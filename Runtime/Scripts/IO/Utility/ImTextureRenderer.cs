using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Imui.IO.Utility
{
    public class ImTextureRenderer : IDisposable
    {
        private const int RES_MIN = 32;
        private const int RES_MAX = 4096;
        
        public RenderTexture Texture { get; private set; }

        private bool disposed;
        
        public void SetupRenderTarget(CommandBuffer cmd, Vector2Int textureSize, out bool textureChanged)
        {
            AssertDisposed();
            
            textureChanged = SetupTexture(textureSize, 1.0f);
            
            cmd.Clear();
            cmd.SetRenderTarget(Texture);
            cmd.ClearRenderTarget(true, true, Color.clear);
        }
        
        private bool SetupTexture(Vector2Int size, float scale)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ImTextureRenderer));
            }
            
            var w = Mathf.Clamp((int)(size.x * scale), RES_MIN, RES_MAX);
            var h = Mathf.Clamp((int)(size.y * scale), RES_MIN, RES_MAX);

            if (w == 0 || h == 0)
            {
                return false;
            }

            if (Texture != null && Texture.IsCreated() && Texture.width == w && Texture.height == h)
            {
                return false;
            }
            
            ReleaseTexture();
            
            Texture = new RenderTexture(w, h, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
            Texture.name = "ImuiRenderBuffer";
            
            return Texture.Create();
        }
        
        private void ReleaseTexture()
        {
            if (Texture != null)
            {
                Texture.Release();
                Texture = null;
            }
        }

        private void AssertDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ImTextureRenderer));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            ReleaseTexture();
            disposed = true;
        }
    }
}