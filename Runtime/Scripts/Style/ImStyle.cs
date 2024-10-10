using System;
using UnityEngine;

namespace Imui.Style
{
    // TODO:
    // * Make it work for dark themes
    // * Readonly should modify colors different way (dunno how)
    // * Remove old ImLight/DarkTheme and move missing parts here  
    //     * Fix scroll bar styling
    
    // * Make base dark and light themes
    // * Move style into ImGui
    // * ...
    // * Clean up
    
    [Serializable]
    public class ImStyle
    {
        public bool IsDark;

        public float TextSize;
        public float Spacing;
        public float InnerSpacing;
        public float Indent;
        public float ExtraRowSize;
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