using System;

namespace Imui.Style
{
    [Serializable]
    public struct ImStyleSheet
    {
        public ImTheme Theme;
        public ImStyleLayout Layout;
        public ImStyleWindow Window;
        public ImStyleText Text;
        public ImStyleButton Button;
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
    }
}