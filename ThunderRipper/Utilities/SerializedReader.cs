using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThunderRipper.Utilities
{
    public class SerializedReader : BinaryReader
    {
        public bool BigEndian { get; set; }
        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public SerializedReader(Stream input) : base(input) { }

        public override short ReadInt16() => BigEndian ? (short)ReverseShort((ushort)base.ReadInt16()) : base.ReadInt16();
        public override ushort ReadUInt16() => BigEndian ? ReverseShort(base.ReadUInt16()) : base.ReadUInt16();
        public override int ReadInt32() => BigEndian ? (int)ReverseInt((uint)base.ReadInt32()) : base.ReadInt32();
        public override uint ReadUInt32() => BigEndian ? ReverseInt(base.ReadUInt32()) : base.ReadUInt32();
        public override long ReadInt64() => BigEndian ? (long)ReverseLong((ulong)base.ReadInt64()) : base.ReadInt64();
        public override ulong ReadUInt64() => BigEndian ? ReverseLong(base.ReadUInt64()) : base.ReadUInt64();

        public ushort ReverseShort(ushort value)
        {
            return (ushort)(((value & 0xFF00) >> 8) | (value & 0x00FF) << 8);
        }

        public uint ReverseInt(uint value)
        {
            value = (value >> 16) | (value << 16);
            return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
        }

        public ulong ReverseLong(ulong value)
        {
            value = (value >> 32) | (value << 32);
            value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
            return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
        }

        public void Align() => AlignSize(4, 2);
        public void Align8() => AlignSize(8, 3);
        public void Align16() => AlignSize(16, 4);

        private void AlignSize(int size, int power)
        {
            Position = ((Position + size - 1) >> power) << power;
        }

        public string ReadStringLength(int len) => Encoding.UTF8.GetString(ReadBytes(len));
        public string ReadCountString() => ReadStringLength(ReadInt32());
        public string ReadCountStringInt16() => ReadStringLength(ReadUInt16());
        public string ReadCountStringInt32() => ReadStringLength(ReadInt32());

        public string ReadNullTerminated()
        {
            var output = new StringBuilder();
            char curChar;
            while ((curChar = ReadChar()) != 0x00)
            {
                output.Append(curChar);
            }
            return output.ToString();
        }
    }
}
