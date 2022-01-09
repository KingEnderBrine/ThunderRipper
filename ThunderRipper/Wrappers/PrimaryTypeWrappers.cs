using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ThunderRipper.Assets;
using ThunderRipper.Utilities;

namespace ThunderRipper.Wrappers
{
    [DebuggerDisplay("{value}")]
    public struct CharWrapper : IBinaryReadable
    {
        public char value;
        public void ReadBinary(SerializedReader reader) => value = Convert.ToChar(reader.ReadByte());
        public static implicit operator CharWrapper(char value) => new CharWrapper() { value = value };
        public static implicit operator char(CharWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct BoolWrapper : IBinaryReadable
    {
        public bool value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadBoolean();
        public static implicit operator BoolWrapper(bool value) => new BoolWrapper() { value = value };
        public static implicit operator bool(BoolWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ByteWrapper : IBinaryReadable
    {
        public byte value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadByte();
        public static implicit operator ByteWrapper(byte value) => new ByteWrapper() { value = value };
        public static implicit operator byte(ByteWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct SByteWrapper : IBinaryReadable
    {
        public sbyte value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadSByte();
        public static implicit operator SByteWrapper(sbyte value) => new SByteWrapper() { value = value };
        public static implicit operator sbyte(SByteWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ShortWrapper : IBinaryReadable
    {
        public short value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt16();
        public static implicit operator ShortWrapper(short value) => new ShortWrapper() { value = value };
        public static implicit operator short(ShortWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct UShortWrapper : IBinaryReadable
    {
        public ushort value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt16();
        public static implicit operator UShortWrapper(ushort value) => new UShortWrapper() { value = value };
        public static implicit operator ushort(UShortWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct IntWrapper : IBinaryReadable
    {
        public int value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt32();
        public static implicit operator IntWrapper(int value) => new IntWrapper() { value = value };
        public static implicit operator int(IntWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct UIntWrapper : IBinaryReadable
    {
        public uint value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt32();
        public static implicit operator UIntWrapper(uint value) => new UIntWrapper() { value = value };
        public static implicit operator uint(UIntWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct LongWrapper : IBinaryReadable
    {
        public long value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt64();
        public static implicit operator LongWrapper(long value) => new LongWrapper() { value = value };
        public static implicit operator long(LongWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ULongWrapper : IBinaryReadable
    {
        public ulong value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt64();
        public static implicit operator ULongWrapper(ulong value) => new ULongWrapper() { value = value };
        public static implicit operator ulong(ULongWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct FloatWrapper : IBinaryReadable
    {
        public float value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadSingle();
        public static implicit operator FloatWrapper(float value) => new FloatWrapper() { value = value };
        public static implicit operator float(FloatWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct DoubleWrapper : IBinaryReadable
    {
        public double value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadDouble();
        public static implicit operator DoubleWrapper(double value) => new DoubleWrapper() { value = value };
        public static implicit operator double(DoubleWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct StringWrapper : IBinaryReadable
    {
        public string value;
        public void ReadBinary(SerializedReader reader)
        {
            value = reader.ReadCountString();
            reader.Align();
        }
        public static implicit operator StringWrapper(string value) => new StringWrapper() { value = value };
        public static implicit operator string(StringWrapper value) => value.value;
    }
}
