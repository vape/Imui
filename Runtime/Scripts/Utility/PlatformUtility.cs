using UnityEngine;

namespace Imui.Utility
{
    public static class PlatformUtility
    {
        public static bool IsEditorSimulator()
        {
#if UNITY_EDITOR
            return UnityEngine.Device.SystemInfo.deviceType != DeviceType.Desktop;
#endif
            return false;
        }
    }
}