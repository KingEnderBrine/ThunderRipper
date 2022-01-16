using System;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class TypeTreeItemField
    {
        public ushort Version { get; set; }
        public byte Depth { get; set; }
        public byte TypeFlags { get; set; }
        public uint TypeStringOffset { get; set; }
        public uint NameStringOffset { get; set; }
        public int Size { get; set; }
        public uint Index { get; set; }
        public uint MetaFlags { get; set; }
        public byte[] Unknown { get; set; }

        public TypeTreeItemField(SerializedReader reader, uint headerVersion)
        {
            Version = reader.ReadUInt16();
            Depth = reader.ReadByte();
            TypeFlags = reader.ReadByte();
            TypeStringOffset = reader.ReadUInt32();
            NameStringOffset = reader.ReadUInt32();
            Size = reader.ReadInt32();
            Index = reader.ReadUInt32();
            MetaFlags = reader.ReadUInt32();
            if (headerVersion >= 18)
            {
                Unknown = reader.ReadBytes(8);
            }
            else
            {
                Unknown = Array.Empty<byte>();
            }
        }
    }
}