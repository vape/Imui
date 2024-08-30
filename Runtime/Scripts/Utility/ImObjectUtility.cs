namespace Imui.Utility
{
    public static class ImObjectUtility
    {
        public static void Destroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
#else
            UnityEngine.Object.Destroy(obj);
#endif
        }
    }
}