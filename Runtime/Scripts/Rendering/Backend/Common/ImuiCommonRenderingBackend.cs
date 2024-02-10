using Imui.Utility;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Imui.Rendering.Backend.Common
{
    public class ImuiCommonRenderingBackend : MonoBehaviour, IImuiRenderingBackend
    {
        private const CameraEvent RENDER_EVENT = CameraEvent.AfterEverything;
        private const float RES_SCALE_MIN = 0.2f;
        private const float RES_SCALE_MAX = 4.0f;
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        public Camera Camera
        {
            get
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                // ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
                return camera == null ? Camera.main : camera;
            }
            set
            {
                RemoveRenderBuffer(renderCmd);
                camera = value;
                AddRenderBuffer(renderCmd);
            }
        }
        
        [SerializeField] private new Camera camera;
        [Range(RES_SCALE_MIN, RES_SCALE_MAX)]
        [SerializeField] private float resolutionScale = 1.0f;
        
        private CommandBuffer renderCmd;
        private CommandBuffer setupCmd;
        private RenderTexture rt;
        private Shader blitShader;
        private Material blitMaterial;
        private DynamicArray<IImuiRenderer> renderers = new(1);
        
        private void OnEnable()
        {
            blitShader = Resources.Load<Shader>("imui_blit_fullscreen");
            blitMaterial = new Material(blitShader);
            renderCmd = new CommandBuffer();
            setupCmd = new CommandBuffer();
            
            UpdateRenderTexture();
            AddRenderBuffer(renderCmd);
        }

        private void OnDisable()
        {
            ReleaseRenderTexture();
            RemoveRenderBuffer(renderCmd);
            
            Destroy(blitMaterial);
            blitMaterial = null;
            
            Resources.UnloadAsset(blitShader);
            blitShader = null;

            renderCmd.Dispose();
            renderCmd = null;
            
            setupCmd.Dispose();
            setupCmd = null;
        }

        private void OnValidate()
        {
            Camera = Camera;
            UpdateRenderTexture();
        }

        private void UpdateRenderTexture()
        {
            resolutionScale = Mathf.Clamp(resolutionScale, RES_SCALE_MIN, RES_SCALE_MAX);
            
            var w = (int)(Screen.width * resolutionScale);
            var h = (int)(Screen.height * resolutionScale);

            if (w == 0 || h == 0)
            {
                return;
            }

            if (rt == null || rt.width != w || rt.height != h)
            {
                ReleaseRenderTexture();
                rt = new RenderTexture(w, h, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);

                for (int i = 0; i < renderers.Count; ++i)
                {
                    renderers.Array[i].OnFrameBufferSizeChanged(new Vector2(w, h));
                }
            }
        }

        private void ReleaseRenderTexture()
        {
            if (rt != null)
            {
                rt.Release();
                rt = null;
            }
        }

        private void AddRenderBuffer(CommandBuffer cmd)
        {
            if (cmd == null)
            {
                return;
            }
            
            var cam = Camera;
            if (cam != null)
            {
                Camera.AddCommandBuffer(RENDER_EVENT, cmd);
            }
        }

        private void RemoveRenderBuffer(CommandBuffer cmd)
        {
            if (cmd == null)
            {
                return;
            }
            
            var cam = Camera;
            if (cam != null)
            {
                Camera.RemoveCommandBuffer(RENDER_EVENT, cmd);
            }
        }

        private void Update()
        {
            UpdateRenderTexture();
        }

        // ReSharper disable once ParameterHidesMember
        public void AddRenderer(IImuiRenderer renderer)
        {
            renderers.Add(renderer);

            if (rt != null)
            {
                renderer.OnFrameBufferSizeChanged(new Vector2(rt.width, rt.height));
            }
        }

        // ReSharper disable once ParameterHidesMember
        public void RemoveRenderer(IImuiRenderer renderer)
        {
            for (int i = 0; i < renderers.Count; ++i)
            {
                if (ReferenceEquals(renderers.Array[i], renderer))
                {
                    renderers.RemoveAtUnordered(i);
                    break;
                }
            }
        }
        
        public void Setup()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            setupCmd.Clear();
            for (int i = 0; i < renderers.Count; ++i)
            {
                renderers.Array[i].Setup(setupCmd);
            }
            
            Graphics.ExecuteCommandBuffer(setupCmd);
        }

        public void Render()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            renderCmd.Clear();
            renderCmd.SetRenderTarget(rt);
            renderCmd.ClearRenderTarget(true, true, Color.clear);
            for (int i = 0; i < renderers.Count; ++i)
            {
                renderers.Array[i].Render(renderCmd);
            }
            
            blitMaterial.SetTexture(MainTexId, rt);
            renderCmd.Blit(rt, Camera.targetTexture, blitMaterial);
        }
    }
}