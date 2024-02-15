using System;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextSettings
    {
        public float Size;
        [Range(0, 1)] 
        public float AlignX;
        [Range(0, 1)]
        public float AlignY;
    }
}