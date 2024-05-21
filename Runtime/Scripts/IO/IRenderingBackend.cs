using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.IO
{
    public interface IRenderingBackend
    {
        CommandBuffer CreateCommandBuffer();
        void ReleaseCommandBuffer(CommandBuffer cmd);
        
        void SetIsRaycastTarget(bool value);
        Rect GetScreenRect();
        
        void SetupRenderTarget(CommandBuffer cmd);
        void Execute(CommandBuffer cmd);
    }
}