using System;
using System.Diagnostics;

// ReSharper disable CheckNamespace

internal class ImuiAssertException : Exception
{
    public ImuiAssertException(string message) : base(message) { }
}

internal static class ImAssert
{
    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void True(bool value, string message)
    {
        if (!value)
        {
            throw new ImuiAssertException(message);
        }
    }
    
    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void False(bool value, string message)
    {
        if (value)
        {
            throw new ImuiAssertException(message);
        }
    }

    [Conditional("IMUI_DEBUG")]
    [Conditional("IMUI_VALIDATION")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("UNITY_EDITOR")]
    public static void Error(string message)
    {
        throw new ImuiAssertException(message);
    }
}