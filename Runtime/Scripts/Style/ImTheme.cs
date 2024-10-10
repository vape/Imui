using System;
using UnityEngine;

namespace Imui.Style
{
    [Serializable]
    public class ImTheme
    {
        // TODO (artem-s): this probably could be calculated automatically
        public bool IsDark;

        public float TextSize;
        public float Spacing;
        public float InnerSpacing;
        public float Indent;
        public float ExtraRowSize;
        // TODO (artem-s): this does not belong here
        public float ScrollSpeed;
        public float ScrollBarSize;

        public float WindowBorderRadius;
        public float WindowBorderThickness;
        
        public float BorderRadius;
        public float BorderWidth;

        public float ReadOnlyColorMultiplier;
        
        public Color32 Background;
        public Color32 Foreground;

        public Color32 AccentBackground;
        public Color32 AccentForeground;
        
        public Color32 BorderColor;
        public Color32 ButtonColor;
        public Color32 FieldColor;
    }
}