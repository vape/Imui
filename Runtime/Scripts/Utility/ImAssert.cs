using System;
using System.Diagnostics;

// ReSharper disable CheckNamespace

internal class ImuiAssertException: Exception
{
    public ImuiAssertException(string message): base(message) { }
}

internal static class ImAssert
{
    [Conditional("IMUI_DEBUG")]
    public static void IsTrue(bool value, string message)
    {
        if (!value)
        {
            throw new ImuiAssertException(message);
        }
    }

    [Conditional("IMUI_DEBUG")]
    public static void IsFalse(bool value, string message)
    {
        if (value)
        {
            throw new ImuiAssertException(message);
        }
    }
}