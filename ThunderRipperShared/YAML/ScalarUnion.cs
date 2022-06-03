using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using ThunderRipperShared.YAML.Extensions;

namespace ThunderRipperShared.YAML
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ScalarUnion
    {
        [FieldOffset(0)]
        private ScalarType type;
        public ScalarType Type => type;

        [FieldOffset(1)]
        private bool boolValue;
        public bool BoolValue
        {
            get => type == ScalarType.Boolean ? boolValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Boolean;
                boolValue = value;
            }
        }

        [FieldOffset(1)]
        private char charValue;
        public char CharValue
        {
            get => type == ScalarType.Char ? charValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Char;
                charValue = value;
            }
        }

        [FieldOffset(1)]
        private byte byteValue;
        public byte ByteValue
        {
            get => type == ScalarType.Byte ? byteValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Byte;
                byteValue = value;
            }
        }

        [FieldOffset(1)]
        private sbyte sbyteValue;
        public sbyte SByteValue
        {
            get => type == ScalarType.SByte ? sbyteValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.SByte;
                sbyteValue = value;
            }
        }

        [FieldOffset(1)]
        private short shortValue;
        public short ShortValue
        {
            get => type == ScalarType.Int16 ? shortValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Int16;
                shortValue = value;
            }
        }

        [FieldOffset(1)]
        private ushort ushortValue;
        public ushort UShortValue
        {
            get => type == ScalarType.UInt16 ? ushortValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.UInt16;
                ushortValue = value;
            }
        }

        [FieldOffset(1)]
        private uint uintValue;
        public uint UIntValue
        {
            get => type == ScalarType.UInt32 ? uintValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.UInt32;
                uintValue = value;
            }
        }

        [FieldOffset(1)]
        private int intValue;
        public int IntValue
        {
            get => type == ScalarType.Int32 ? intValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Int32;
                intValue = value;
            }
        }

        [FieldOffset(1)]
        private ulong ulongValue;
        public ulong ULongValue
        {
            get => type == ScalarType.UInt64 ? ulongValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.UInt64;
                ulongValue = value;
            }
        }

        [FieldOffset(1)]
        private long longValue;
        public long LongValue
        {
            get => type == ScalarType.Int64 ? longValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Int64;
                longValue = value;
            }
        }

        [FieldOffset(1)]
        private float floatValue;
        public float FloatValue
        {
            get => type == ScalarType.Single ? floatValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Single;
                floatValue = value;
            }
        }

        [FieldOffset(1)]
        private double doubleValue;
        public double DoubleValue
        {
            get => type == ScalarType.Double ? doubleValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.Double;
                doubleValue = value;
            }
        }

        [FieldOffset(1)]
        private string stringValue;
        public string StringValue
        {
            get => type == ScalarType.String ? stringValue : throw new InvalidCastException();
            set
            {
                type = ScalarType.String;
                stringValue = value;
            }
        }

        public void Set<T>(T value)
        {
            if (value is bool boolValue)
            {
                this.BoolValue = boolValue;
            }
            else if (value is char charValue)
            {
                this.CharValue = charValue;
            }
            else if (value is byte byteValue)
            {
                this.ByteValue = byteValue;
            }
            else if (value is sbyte sbyteValue)
            {
                this.SByteValue = sbyteValue;
            }
            else if (value is short shortValue)
            {
                this.ShortValue = shortValue;
            }
            else if (value is ushort ushortValue)
            {
                this.UShortValue = ushortValue;
            }
            else if (value is uint uintValue)
            {
                this.UIntValue = uintValue;
            }
            else if (value is int intValue)
            {
                this.IntValue = intValue;
            }
            else if (value is ulong ulongValue)
            {
                this.ULongValue = ulongValue;
            }
            else if (value is long longValue)
            {
                this.LongValue = longValue;
            }
            else if (value is float floatValue)
            {
                this.FloatValue = floatValue;
            }
            else if (value is double doubleValue)
            {
                this.DoubleValue = doubleValue;
            }
            else if (value is string stringValue)
            {
                this.StringValue = stringValue;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ScalarType.Boolean:
                    return BoolValue ? "true" : "false";
                case ScalarType.Char:
                    return CharValue.ToString();
                case ScalarType.Byte:
                    return ByteValue.ToString();
                case ScalarType.SByte:
                    return SByteValue.ToString();
                case ScalarType.Int16:
                    return ShortValue.ToString();
                case ScalarType.UInt16:
                    return UShortValue.ToString();
                case ScalarType.Int32:
                    return IntValue.ToString();
                case ScalarType.UInt32:
                    return UIntValue.ToString();
                case ScalarType.Int64:
                    return LongValue.ToString();
                case ScalarType.UInt64:
                    return ULongValue.ToString();
                case ScalarType.Single:
                    return FloatValue.ToString(CultureInfo.InvariantCulture);
                case ScalarType.Double:
                    return DoubleValue.ToString(CultureInfo.InvariantCulture);
                case ScalarType.String:
                    return StringValue;

                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }

        public string ToHexString()
        {
            switch (Type)
            {
                case ScalarType.Byte:
                    return ByteValue.ToHexString();
                case ScalarType.SByte:
                    return ByteValue.ToHexString();
                case ScalarType.Int16:
                    return ShortValue.ToHexString();
                case ScalarType.UInt16:
                    return UShortValue.ToHexString();
                case ScalarType.Int32:
                    return IntValue.ToHexString();
                case ScalarType.UInt32:
                    return UIntValue.ToHexString();
                case ScalarType.Int64:
                    return LongValue.ToHexString();
                case ScalarType.UInt64:
                    return ULongValue.ToHexString();
                case ScalarType.Single:
                    return FloatValue.ToHexString();
                case ScalarType.Double:
                    return DoubleValue.ToHexString();
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }

        public Emitter WriteTo(Emitter emitter, ScalarStyle style)
        {
            if (style == ScalarStyle.Hex)
            {
                return emitter.Write(ToHexString());
            }

            switch (Type)
            {
                case ScalarType.Boolean:
                    return emitter.Write(BoolValue);
                case ScalarType.Char:
                    return emitter.Write(CharValue);
                case ScalarType.Byte:
                    return emitter.Write(ByteValue);
                case ScalarType.SByte:
                    return emitter.Write(SByteValue);
                case ScalarType.Int16:
                    return emitter.Write(ShortValue);
                case ScalarType.UInt16:
                    return emitter.Write(UShortValue);
                case ScalarType.Int32:
                    return emitter.Write(IntValue);
                case ScalarType.UInt32:
                    return emitter.Write(UIntValue);
                case ScalarType.Int64:
                    return emitter.Write(LongValue);
                case ScalarType.UInt64:
                    return emitter.Write(ULongValue);
                case ScalarType.Single:
                    return emitter.Write(FloatValue);
                case ScalarType.Double:
                    return emitter.Write(DoubleValue);
                case ScalarType.String:
                    return WriteString(emitter, style);

                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }

        private Emitter WriteString(Emitter emitter, ScalarStyle style)
        {
            switch (style)
            {
                case ScalarStyle.Plain:
                    if (emitter.IsFormatKeys && emitter.IsKey)
                    {
                        emitter.WriteFormat(StringValue);
                    }
                    else
                    {
                        emitter.Write(StringValue);
                    }
                    break;
                case ScalarStyle.SingleQuoted:
                    emitter.WriteDelayed();
                    for (int i = 0; i < StringValue.Length; i++)
                    {
                        var c = StringValue[i];
                        emitter.WriteRaw(c);
                        if (c == '\'')
                        {
                            emitter.WriteRaw(c);
                        }
                        else if (c == '\n')
                        {
                            emitter.WriteRaw("\n    ");
                        }
                    }
                    break;
                case ScalarStyle.DoubleQuoted:
                    emitter.WriteDelayed();
                    for (int i = 0; i < StringValue.Length; i++)
                    {
                        var c = StringValue[i];
                        switch (c)
                        {
                            case '\\':
                                emitter.WriteRaw('\\').WriteRaw('\\');
                                break;
                            case '\n':
                                emitter.WriteRaw('\\').WriteRaw('n');
                                break;
                            case '\r':
                                emitter.WriteRaw('\\').WriteRaw('r');
                                break;
                            case '\t':
                                emitter.WriteRaw('\\').WriteRaw('t');
                                break;
                            case '"':
                                emitter.WriteRaw('\\').WriteRaw('"');
                                break;

                            default:
                                emitter.WriteRaw(c);
                                break;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(style.ToString());
            }
            return emitter;
        }
    }
    }
