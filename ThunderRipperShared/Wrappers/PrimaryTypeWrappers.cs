using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ThunderRipperShared.Assets;
using ThunderRipperShared.Utilities;
using ThunderRipperShared.YAML;

namespace ThunderRipperShared.Wrappers
{
    [DebuggerDisplay("{value}")]
    public struct CharWrapper : IBinaryReadable, IYAMLExportable
    {
        public char value;

        public void ReadBinary(SerializedReader reader) => value = Convert.ToChar(reader.ReadByte());
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator CharWrapper(char value) => new CharWrapper() { value = value };
        public static implicit operator char(CharWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct BoolWrapper : IBinaryReadable, IYAMLExportable
    {
        public bool value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadBoolean();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator BoolWrapper(bool value) => new BoolWrapper() { value = value };
        public static implicit operator bool(BoolWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ByteWrapper : IBinaryReadable, IYAMLExportable
    {
        public byte value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadByte();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator ByteWrapper(byte value) => new ByteWrapper() { value = value };
        public static implicit operator byte(ByteWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct SByteWrapper : IBinaryReadable, IYAMLExportable
    {
        public sbyte value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadSByte();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator SByteWrapper(sbyte value) => new SByteWrapper() { value = value };
        public static implicit operator sbyte(SByteWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ShortWrapper : IBinaryReadable, IYAMLExportable
    {
        public short value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt16();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator ShortWrapper(short value) => new ShortWrapper() { value = value };
        public static implicit operator short(ShortWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct UShortWrapper : IBinaryReadable, IYAMLExportable
    {
        public ushort value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt16();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator UShortWrapper(ushort value) => new UShortWrapper() { value = value };
        public static implicit operator ushort(UShortWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct IntWrapper : IBinaryReadable, IYAMLExportable
    {
        public int value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt32();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator IntWrapper(int value) => new IntWrapper() { value = value };
        public static implicit operator int(IntWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct UIntWrapper : IBinaryReadable, IYAMLExportable
    {
        public uint value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt32();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator UIntWrapper(uint value) => new UIntWrapper() { value = value };
        public static implicit operator uint(UIntWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct LongWrapper : IBinaryReadable, IYAMLExportable
    {
        public long value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadInt64();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator LongWrapper(long value) => new LongWrapper() { value = value };
        public static implicit operator long(LongWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct ULongWrapper : IBinaryReadable, IYAMLExportable
    {
        public ulong value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadUInt64();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator ULongWrapper(ulong value) => new ULongWrapper() { value = value };
        public static implicit operator ulong(ULongWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct FloatWrapper : IBinaryReadable, IYAMLExportable
    {
        public float value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadSingle();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator FloatWrapper(float value) => new FloatWrapper() { value = value };
        public static implicit operator float(FloatWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct DoubleWrapper : IBinaryReadable, IYAMLExportable
    {
        public double value;
        public void ReadBinary(SerializedReader reader) => value = reader.ReadDouble();
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator DoubleWrapper(double value) => new DoubleWrapper() { value = value };
        public static implicit operator double(DoubleWrapper value) => value.value;
    }

    [DebuggerDisplay("{value}")]
    public struct StringWrapper : IBinaryReadable, IYAMLExportable
    {
        public string value;
        public void ReadBinary(SerializedReader reader)
        {
            value = reader.ReadCountString();
            reader.Align();
        }
        public YAMLNode ExportYAML() => new YAMLScalarNode(value);
        public static implicit operator StringWrapper(string value) => new StringWrapper() { value = value };
        public static implicit operator string(StringWrapper value) => value.value;
    }
}
