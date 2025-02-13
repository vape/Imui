using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.IO
{
    public interface IImuiRenderer
    {
        Vector2 GetScreenSize();
        float GetScale();
        
        CommandBuffer CreateCommandBuffer();
        void ReleaseCommandBuffer(CommandBuffer cmd);

        Vector2Int SetupRenderTarget(CommandBuffer cmd);
        void Execute(CommandBuffer cmd);
    }
}