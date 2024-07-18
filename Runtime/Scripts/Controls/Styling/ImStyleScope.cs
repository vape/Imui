namespace Imui.Controls.Styling
{
    public unsafe ref struct ImStyleScope<T> where T : unmanaged
    {
        private readonly T* property;
        private readonly T original;
        
        private bool disposed;

        public ImStyleScope(ref T property, in T style) : this(ref property)
        {
            property = style;
        }
        
        public ImStyleScope(ref T property)
        {
            fixed (T* ptr = &property)
            {
                this.property = ptr;
            }

            original = property;
            disposed = false;
        }
        
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            *property = original;
            disposed = true;
        }
    }
}