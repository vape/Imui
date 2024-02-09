using UnityEngine.Rendering;

namespace Imui.Rendering.Backend
{
    public interface IImuiRenderer
    {
        void Setup(CommandBuffer cmd);
        void Render(CommandBuffer cmd);
    }
    
    public interface IImuiRenderingBackend
    {
        void AddRenderer(IImuiRenderer renderer);
        void RemoveRenderer(IImuiRenderer imuiRenderer);

        void Setup();
        void Render();
    }
}