using System;

namespace Imui.Style
{
    [Serializable]
    public struct ImAlignment
    {
        public float X;
        public float Y;

        public ImAlignment(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}