using System.Collections.Generic;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class TypeTree
    {
        public string UnityVersion { get; set; }
        public uint Version { get; set; }
        public bool HasTypeTree { get; set; }
        public List<TypeTreeItem> Types { get; }
        public byte[] Unknown { get; set; }

        public TypeTree(SerializedReader reader, uint headerVersion)
        {
            UnityVersion = reader.ReadNullTerminated();
            Version = reader.ReadUInt32();
            
            if (headerVersion >= 13)
            {
                HasTypeTree = reader.ReadBoolean();
            }

            var typeCount = reader.ReadInt32();
            Types = new List<TypeTreeItem>(typeCount);
            for (var i = 0; i < typeCount; i++)
            {
                Types.Add(new TypeTreeItem(reader, HasTypeTree, headerVersion));
            }

            if (headerVersion < 14)
            {
                Unknown = reader.ReadBytes(3);
            }
        }
    }
}