using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class AssetInfo
    {
        public long Index { get; set; }
        public long Offset { get; set; }
        public uint Size { get; set; }
        public int TypeIndexOrID { get; set; }
        public int TypeID { get; set; }
        public ushort InheritedUnityClass { get; set; }
        public ushort ScriptIndex { get; set; }
        public byte Unknown { get; set; }

        public AssetInfo(SerializedReader reader, uint headerVersion, TypeTree typeTree)
        {
            reader.Align();
            Index = headerVersion >= 14 ? reader.ReadInt64() : reader.ReadUInt32();
            Offset = headerVersion >= 22 ? reader.ReadInt64() : reader.ReadUInt32();
            Size = reader.ReadUInt32();
            TypeIndexOrID = reader.ReadInt32();
            if (headerVersion < 16)
            {
                InheritedUnityClass = reader.ReadUInt16();
            }
            if (headerVersion <= 16)
            {
                ScriptIndex = reader.ReadUInt16();
            }
            if (headerVersion == 15 || headerVersion == 16)
            {
                Unknown = reader.ReadByte();
            }

            if (headerVersion < 16)
            {
                TypeID = TypeIndexOrID < 0 ? 114 : TypeIndexOrID;
            }
            else
            {
                TypeID = typeTree.Types[TypeIndexOrID].ClassID;
            }
        }
    }
}