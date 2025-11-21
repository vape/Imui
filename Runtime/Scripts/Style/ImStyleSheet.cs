using System;

namespace Imui.Style
{
    public struct ImStyleGlobal
    {
        public float ReadOnlyModifier;
        public float EmbeddedButtonPadding;
    }
    
    [Serializable]
    public struct ImStyleSheet
    {
        public ImStyleGlobal Global;
        public ImStyleLayout Layout;
        public ImStyleWindow Window;
        public ImStyleText Text;
        public ImStyleButton Button;
        public ImStyleButton AccentButton;
        public ImStyleCheckbox Checkbox;
        public ImStyleFoldout Foldout;
        public ImStyleScrollbar Scroll;
        public ImStyleTextEdit TextEdit;
        public ImStyleDropdown Dropdown;
        public ImStyleSlider Slider;
        public ImStyleList List;
        public ImStyleRadiobox Radiobox;
        public ImStyleSeparator Separator;
        public ImStyleTree Tree;
        public ImStyleTooltip Tooltip;
        public ImStyleMenu Menu;
        public ImStyleMenuBar MenuBar;
        public ImStyleColorPicker ColorPicker;
        public ImStyleTab Tabs;
        public ImStyleTable Table;
        public ImStyleProgressBar ProgressBar;
    }
}