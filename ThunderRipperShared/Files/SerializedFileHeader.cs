using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class SerializedFileHeader
    {
        public uint MetadataSize { get; set; }
        public long FileSize { get; set; }
        public uint Version { get; set; }
        public long DataOffset { get; set; }
        public byte Endianness { get; set; }
        public byte[] Reserved { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }

        public SerializedFileHeader(SerializedReader reader)
        {
            //Header is always BigEndian
            reader.BigEndian = true;

            MetadataSize = reader.ReadUInt32();
            FileSize = reader.ReadUInt32();
            Version = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();
            Endianness = reader.ReadByte();
            Reserved = reader.ReadBytes(3);

            if (Version >= 22)
            {
                MetadataSize = reader.ReadUInt32();
                FileSize = reader.ReadInt64();
                DataOffset = reader.ReadInt64();
                Unknown1 = reader.ReadUInt32();
                Unknown2 = reader.ReadUInt32();
            }

            reader.BigEndian = Endianness == 1;
        }
    }
}