using System;

namespace Imui.Style
{
    [Serializable]
    public struct ImTheme
    {
        public float ReadOnlyColorMultiplier;
        
        public ImLayoutStyle Layout;
        public ImWindowStyle Window;
        public ImTextStyle Text;
        public ImButtonStyle Button;
        public ImCheckboxStyle Checkbox;
        public ImFoldoutStyle Foldout;
        public ImScrollStyle Scroll;
        public ImTextEditStyle TextEdit;
        public ImDropdownStyle Dropdown;
        public ImSliderStyle Slider;
        public ImListStyle List;
        public ImRadioboxStyle Radiobox;
        public ImSeparatorStyle Separator;
        public ImTreeStyle Tree;
    }
}