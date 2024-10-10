using System;
using Imui.Core; // TODO (artem-s): styling should not depent on Core module
using UnityEngine;

namespace Imui.Style
{
    [Serializable]
    public struct ImBoxStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
        public float BorderWidth;
        public ImRectRadius BorderRadius;

        public ImBoxStyle Apply(ImAdjacency adjacency)
        {
            if ((adjacency & ImAdjacency.Left) != 0)
            {
                BorderRadius.BottomRight = 0;
                BorderRadius.TopRight = 0;
            }

            if ((adjacency & ImAdjacency.Right) != 0)
            {
                BorderRadius.BottomLeft = 0;
                BorderRadius.TopLeft = 0;
            }

            return this;
        }
    }
    
    [Serializable]
    public struct ImLayoutStyle
    {
        public float ExtraRowHeight;
        public float TextSize;
        public float ControlsSpacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Indent;
    }
    
    [Serializable]
    public struct ImButtonStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
    }
    
    [Serializable]
    public struct ImButtonStyle
    {
        public ImButtonStateStyle Normal;
        public ImButtonStateStyle Hovered;
        public ImButtonStateStyle Pressed;
        public float BorderThickness;
        public float BorderRadius;
        public ImAlignment Alignment;
    }
    
    [Serializable]
    public struct ImCheckboxStyle
    {
        public float CheckmarkScale;
        public ImButtonStyle Normal;
        public ImButtonStyle Checked;
    }
    
    [Serializable]
    public struct ImDropdownStyle
    {
        public float ArrowScale;
        public ImButtonStyle Button;
        public float MaxListHeight;
        public float MinListWidth;
    }
    
    [Serializable]
    public struct ImFoldoutStyle
    {
        public float ArrowScale;
        public ImButtonStyle Button;
    }
    
    [Serializable]
    public struct ImListStyle
    {
        public ImBoxStyle Box;
        public ImPadding Padding;
        public ImButtonStyle ItemNormal;
        public ImButtonStyle ItemSelected;
    }
    
    [Serializable]
    public struct ImRadioboxStyle
    {
        public float KnobScale;
        public ImButtonStyle Normal;
        public ImButtonStyle Checked;
    }
    
    [Serializable]
    public struct ImScrollBarStateStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
    }

    [Serializable]
    public struct ImScrollStyle
    {
        public float Size;
        public float Padding;
        public float BorderRadius;
        public ImPadding VMargin;
        public ImPadding HMargin;
        public ImScrollBarStateStyle NormalState;
        public ImScrollBarStateStyle HoveredState;
        public ImScrollBarStateStyle PressedState;
    }
    
    [Serializable]
    public struct ImSeparatorStyle
    {
        public float Thickness;
        public Color32 Color;
    }
    
    [Serializable]
    public struct ImSliderStyle
    {
        public ImBoxStyle Box;
        public ImButtonStyle Handle;
    }
        
    [Serializable]
    public struct ImTextStyle
    {
        public Color32 Color;
        public ImAlignment Alignment;
    }
    
    [Serializable]
    public struct ImTextEditStateStyle
    {
        public ImBoxStyle Box;
        public Color32 SelectionColor;
    }
    
    [Serializable]
    public struct ImTextEditStyle
    {
        public ImTextEditStateStyle Normal;
        public ImTextEditStateStyle Selected;
        public float CaretWidth;
        public ImAlignment Alignment;
        public bool TextWrap;
    }
    
    [Serializable]
    public struct ImTreeStyle
    {
        public float ArrowScale;
        public ImButtonStyle ItemNormal;
        public ImButtonStyle ItemSelected;
    }
        
    [Serializable]
    public struct ImWindowTitleBarStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public ImAlignment Alignment;
    }
        
    [Serializable]
    public struct ImWindowStyle
    {
        public ImBoxStyle Box;
        public Color32 ResizeHandleColor;
        public float ResizeHandleSize;
        public ImPadding ContentPadding;
        public ImWindowTitleBarStyle TitleBar;
    }
}