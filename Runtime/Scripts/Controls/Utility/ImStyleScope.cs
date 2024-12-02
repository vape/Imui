using System.Runtime.CompilerServices;
using Imui.Core;

namespace Imui.Style
{
    public ref struct ImStyleScope<T> where T : unmanaged
    {
        private ImGui gui;
        private bool disposed;

        public ImStyleScope(ImGui gui, ref T property, in T style)
        {
            this.gui = gui;
            this.gui.PushStyle(ref property, in style);

            disposed = false;
        }
        
        public ImStyleScope(ImGui gui, ref T property)
        {
            this.gui = gui;
            this.gui.PushStyle(ref property);

            disposed = false;
        }
        
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            gui.PopStyle<T>();
        }
    }

    public static class ImControlStyleExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImStyleScope<T> StyleScope<T>(this ImGui gui, ref T prop, in T style) where T: unmanaged
        {
            return new ImStyleScope<T>(gui, ref prop, in style);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImStyleScope<T> StyleScope<T>(this ImGui gui, ref T prop) where T: unmanaged
        {
            return new ImStyleScope<T>(gui, ref prop);
        }
    }
}