using System;
using System.Diagnostics;

// ReSharper disable CheckNamespace

internal class ImuiAssertException : Exception
{
    public ImuiAssertException(string message) : base(message) { }
}

internal static class ImuiAssert
{
    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    public static void True(bool value, string message)
    {
        if (!value)
        {
            throw new ImuiAssertException(message);
        }
    }
    
    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    public static void False(bool value, string message)
    {
        if (value)
        {
            throw new ImuiAssertException(message);
        }
    }

    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    public static void Error(string message)
    {
        throw new ImuiAssertException(message);
    }
}