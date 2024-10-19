using System;
using Imui.Core; // TODO (artem-s): styling should not depend on Core module
using UnityEngine;

namespace Imui.Style
{
    [Serializable]
    public struct ImStyleBox
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
        public float BorderThickness;
        public ImRectRadius BorderRadius;

        public ImStyleBox MakeAdjacent(ImAdjacency adjacency)
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
    public struct ImStyleLayout
    {
        public float ExtraRowHeight;
        public float TextSize;
        public float Spacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Indent;
    }
    
    [Serializable]
    public struct ImStyleButtonState
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
    }
    
    [Serializable]
    public struct ImStyleButton
    {
        public ImStyleButtonState Normal;
        public ImStyleButtonState Hovered;
        public ImStyleButtonState Pressed;
        public float BorderThickness;
        public float BorderRadius;
        public ImAlignment Alignment;
    }
    
    [Serializable]
    public struct ImStyleCheckbox
    {
        public float CheckmarkScale;
        public ImStyleButton Normal;
        public ImStyleButton Checked;
    }
    
    [Serializable]
    public struct ImStyleDropdown
    {
        public float ArrowScale;
        public ImStyleButton Button;
        public float MaxListHeight;
        public float MinListWidth;
    }
    
    [Serializable]
    public struct ImStyleFoldout
    {
        public float ArrowScale;
        public ImStyleButton Button;
    }
    
    [Serializable]
    public struct ImStyleList
    {
        public ImStyleBox Box;
        public ImPadding Padding;
        public ImStyleButton ItemNormal;
        public ImStyleButton ItemSelected;
    }
    
    [Serializable]
    public struct ImStyleRadiobox
    {
        public float KnobScale;
        public ImStyleButton Normal;
        public ImStyleButton Checked;
    }
    
    [Serializable]
    public struct ImStyleScrollBarState
    {
        public Color32 BackColor;
        public Color32 FrontColor;
    }

    [Serializable]
    public struct ImStyleScrollbar
    {
        public float Size;
        public float BorderThickness;
        public float BorderRadius;
        public ImPadding VMargin;
        public ImPadding HMargin;
        public ImStyleScrollBarState NormalState;
        public ImStyleScrollBarState HoveredState;
        public ImStyleScrollBarState PressedState;
    }
    
    [Serializable]
    public struct ImStyleSeparator
    {
        public float Thickness;
        public Color32 Color;
    }
    
    [Serializable]
    public struct ImStyleSlider
    {
        public ImStyleBox Normal;
        public ImStyleBox Selected;
        public ImStyleButton Handle;
        public float BackScale;
    }
        
    [Serializable]
    public struct ImStyleText
    {
        public Color32 Color;
        public ImAlignment Alignment;
    }
    
    [Serializable]
    public struct ImStyleTextEditState
    {
        public ImStyleBox Box;
        public Color32 SelectionColor;
    }
    
    [Serializable]
    public struct ImStyleTextEdit
    {
        public ImStyleTextEditState Normal;
        public ImStyleTextEditState Selected;
        public float CaretWidth;
        public ImAlignment Alignment;
        public bool TextWrap;
    }
    
    [Serializable]
    public struct ImStyleTree
    {
        public float ArrowScale;
        public ImStyleButton ItemNormal;
        public ImStyleButton ItemSelected;
    }
        
    [Serializable]
    public struct ImStyleWindowTitleBar
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public ImAlignment Alignment;
        public ImStyleButton CloseButton;
    }
        
    [Serializable]
    public struct ImStyleWindow
    {
        public ImStyleBox Box;
        public Color32 ResizeHandleColor;
        public float ResizeHandleSize;
        public ImPadding ContentPadding;
        public ImStyleWindowTitleBar TitleBar;
    }

    [Serializable]
    public struct ImStyleTooltip
    {
        public ImStyleBox Box;
        public ImPadding Padding;
        public Vector2 Offset;
    }

    [Serializable]
    public struct ImStyleMenu
    {
        public ImStyleBox Box;
        public ImPadding Padding;
        public ImStyleButton ItemNormal;
        public ImStyleButton ItemActive;
        public float ArrowScale;
        public float MinWidth;
        public float MinHeight;
    }

    [Serializable]
    public struct ImStyleMenuBar
    {
        public ImStyleBox Box;
        public ImStyleButton ItemNormal;
        public ImStyleButton ItemActive;
    }
}