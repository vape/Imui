using System;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;

// ReSharper disable StaticMemberInGenericType

namespace Imui.Utility
{
    // (artem-s): all of that just use enums in somewhat generic way *without* gc allocations

    internal readonly struct ImEnumValue<TEnum> where TEnum: struct, Enum
    {
        private readonly long longValue;
        private readonly ulong ulongValue;
        private readonly bool signed;

        public ImEnumValue(long value)
        {
            longValue = value;
            ulongValue = default;
            signed = true;
        }

        public ImEnumValue(ulong value)
        {
            longValue = default;
            ulongValue = value;
            signed = false;
        }

        public static ImEnumValue<TEnum> operator |(ImEnumValue<TEnum> val0, ImEnumValue<TEnum> val1)
        {
            return val0.signed ? val0.longValue | val1.longValue : val0.ulongValue | val1.ulongValue;
        }

        public static ImEnumValue<TEnum> operator &(ImEnumValue<TEnum> val0, ImEnumValue<TEnum> val1)
        {
            return val0.signed ? val0.longValue & val1.longValue : val0.ulongValue & val1.ulongValue;
        }

        public static ImEnumValue<TEnum> operator ~(ImEnumValue<TEnum> val)
        {
            return val.signed ? ~val.longValue : ~val.ulongValue;
        }

        public static bool operator ==(ImEnumValue<TEnum> val0, ImEnumValue<TEnum> val1)
        {
            return val0.Equals(val1);
        }

        public static bool operator !=(ImEnumValue<TEnum> val0, ImEnumValue<TEnum> val1)
        {
            return !val0.Equals(val1);
        }

        public static bool operator ==(ImEnumValue<TEnum> val0, int val1)
        {
            return val0.Equals(val1);
        }

        public static bool operator !=(ImEnumValue<TEnum> val0, int val1)
        {
            return !val0.Equals(val1);
        }

        public static implicit operator ImEnumValue<TEnum>(int val) => ImEnumUtility<TEnum>.Signed ? new(val) : new((ulong)val);
        public static implicit operator ImEnumValue<TEnum>(long val) => new(val);
        public static implicit operator ImEnumValue<TEnum>(ulong val) => new(val);

        public TEnum ToEnumType()
        {
            return signed ? ImEnumUtility<TEnum>.FromValueSigned(longValue) : ImEnumUtility<TEnum>.FromValueUnsigned(ulongValue);
        }

        public bool Equals(ImEnumValue<TEnum> other)
        {
            return signed == other.signed && signed ? longValue == other.longValue : ulongValue == other.ulongValue;
        }

        public override bool Equals(object obj)
        {
            return obj is ImEnumValue<TEnum> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(longValue, ulongValue, signed);
        }
    }

    internal static class ImEnumUtility<TEnum> where TEnum: struct, Enum
    {
        public static readonly bool IsFlags = typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null;
        public static readonly string[] Names = Enum.GetNames(typeof(TEnum));
        public static readonly TEnum[] Values = Enum.GetValues(typeof(TEnum)) as TEnum[];
        public static readonly Type Type = Enum.GetUnderlyingType(typeof(TEnum));
        public static readonly bool Signed = Type == typeof(SByte) || Type == typeof(Int16) || Type == typeof(Int32) || Type == typeof(Int64);

        public static ImEnumValue<TEnum> ToValue(TEnum e)
        {
            return Signed ? ToValueSigned(e) : ToValueUnsigned(e);
        }

        public static TEnum FromValueUnsigned(ulong value)
        {
            if (Type == typeof(Byte))
            {
                var typedValue = (Byte)value;
                return UnsafeUtility.As<Byte, TEnum>(ref typedValue);
            }
            else if (Type == typeof(UInt16))
            {
                var typedValue = (UInt16)value;
                return UnsafeUtility.As<UInt16, TEnum>(ref typedValue);
            }
            else if (Type == typeof(UInt32))
            {
                var typedValue = (UInt32)value;
                return UnsafeUtility.As<UInt32, TEnum>(ref typedValue);
            }
            else if (Type == typeof(UInt64))
            {
                return UnsafeUtility.As<UInt64, TEnum>(ref value);
            }

            throw new Exception($"Underlying type of {typeof(TEnum)} is signed");
        }

        public static TEnum FromValueSigned(long value)
        {
            if (Type == typeof(SByte))
            {
                var typedValue = (SByte)value;
                return UnsafeUtility.As<SByte, TEnum>(ref typedValue);
            }
            else if (Type == typeof(Int16))
            {
                var typedValue = (Int16)value;
                return UnsafeUtility.As<Int16, TEnum>(ref typedValue);
            }
            else if (Type == typeof(Int32))
            {
                var typedValue = (Int32)value;
                return UnsafeUtility.As<Int32, TEnum>(ref typedValue);
            }
            else if (Type == typeof(Int64))
            {
                return UnsafeUtility.As<Int64, TEnum>(ref value);
            }

            throw new Exception($"Underlying type of {typeof(TEnum)} is unsigned");
        }

        public static long ToValueSigned(TEnum value)
        {
            if (Type == typeof(SByte))
            {
                return UnsafeUtility.As<TEnum, SByte>(ref value);
            }
            else if (Type == typeof(Int16))
            {
                return UnsafeUtility.As<TEnum, Int16>(ref value);
            }
            else if (Type == typeof(Int32))
            {
                return UnsafeUtility.As<TEnum, Int32>(ref value);
            }
            else if (Type == typeof(Int64))
            {
                return UnsafeUtility.As<TEnum, Int64>(ref value);
            }

            throw new Exception($"Underlying type of {typeof(TEnum)} is unsigned");
        }

        public static ulong ToValueUnsigned(TEnum value)
        {
            if (Type == typeof(Byte))
            {
                return UnsafeUtility.As<TEnum, Byte>(ref value);
            }
            else if (Type == typeof(UInt16))
            {
                return UnsafeUtility.As<TEnum, UInt16>(ref value);
            }
            else if (Type == typeof(UInt32))
            {
                return UnsafeUtility.As<TEnum, UInt32>(ref value);
            }
            else if (Type == typeof(UInt64))
            {
                return UnsafeUtility.As<TEnum, UInt64>(ref value);
            }

            throw new Exception($"Underlying type of {typeof(TEnum)} is signed");
        }
    }
}