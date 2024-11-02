using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

namespace Imui.Utility
{
    public static class ImProfiler
    {
        [Conditional("IMUI_PROFILE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginSample(string name)
        {
           Profiler.BeginSample(name);
        }

        [Conditional("IMUI_PROFILE")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndSample()
        {
           Profiler.EndSample();
        }
    }
}