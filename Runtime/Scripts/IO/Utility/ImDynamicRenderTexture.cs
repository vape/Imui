using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.IO.Utility
{
    public class ImDynamicRenderTexture: IDisposable
    {
        public const int RES_MIN = 32;
        public const int RES_MAX = 4096;

        public RenderTexture Texture { get; private set; }

        private RenderTexture prevTexture;
        private bool disposed;

        public Vector2Int SetupRenderTarget(CommandBuffer cmd, Vector2Int requestedSize, out bool textureChanged)
        {
            AssertDisposed();

            textureChanged = SetupTexture(requestedSize, 1.0f, out var targetSize);

            cmd.Clear();
            cmd.SetRenderTarget(Texture);
            cmd.ClearRenderTarget(true, true, Color.clear);

            return targetSize;
        }

        private bool SetupTexture(Vector2Int size, float scale, out Vector2Int targetSize)
        {
            AssertDisposed();
            ReleasePrevTexture();

            var w = Mathf.Clamp((int)(size.x * scale), RES_MIN, RES_MAX);
            var h = Mathf.Clamp((int)(size.y * scale), RES_MIN, RES_MAX);

            targetSize = new Vector2Int(w, h);

            if (w == 0 || h == 0)
            {
                return false;
            }

            if (Texture && Texture.IsCreated() && Texture.width == w && Texture.height == h)
            {
                return false;
            }

            if (Texture)
            {
                if (!prevTexture)
                {
                    prevTexture = Texture;
                    Texture = null;
                }
                else
                {
                    ReleaseActiveTexture();
                }
            }
            
            var desc = new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGB32, 0, 0, RenderTextureReadWrite.Linear);
            
            Texture = new RenderTexture(desc)
            {
                name = "ImuiRenderBuffer"
            };

            return Texture.Create();
        }

        private void ReleasePrevTexture()
        {
            if (prevTexture)
            {
                prevTexture.Release();
                prevTexture = null;
            }
        }

        private void ReleaseActiveTexture()
        {
            if (Texture)
            {
                Texture.Release();
                Texture = null;
            }
        }
        
#if UNITY_2022_2_OR_NEWER
        [HideInCallstack]
#endif
        private void AssertDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ImDynamicRenderTexture));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            ReleasePrevTexture();
            ReleaseActiveTexture();
            disposed = true;
        }
    }
}