using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Imui.IO.Utility
{
    public class ImTextureRenderer : IDisposable
    {
        private const float RES_SCALE_MIN = 0.2f;
        private const float RES_SCALE_MAX = 4.0f;
        
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
            
            scale = Mathf.Clamp(scale, RES_SCALE_MIN, RES_SCALE_MAX);
            
            var w = (int)(size.x * scale);
            var h = (int)(size.y * scale);

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