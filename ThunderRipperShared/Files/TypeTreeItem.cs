using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Files;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class TypeTreeItem
    {
        public int ClassID { get; set; }
        public byte Unknown { get; set; }
        public ushort ScriptIndex { get; set; }
        public Guid ScriptHash { get; set; }
        public Guid TypeHash { get; set; }
        public List<TypeTreeItemField> Fields { get; }
        public string StringTable { get; set; }
        public List<int> Dependencies { get; }

        public TypeTreeItem(SerializedReader reader, bool hasTypeTree, uint headerVersion)
        {
            ClassID = reader.ReadInt32();

            if (headerVersion >= 16)
            {
                Unknown = reader.ReadByte();
            }
            if (headerVersion >= 17)
            {
                ScriptIndex = reader.ReadUInt16();
            }
            if ((headerVersion < 17 && ClassID < 0) || (headerVersion >= 17 && ClassID == 114))
            {
                ScriptHash = HashUtilities.UnityHashToGuid(reader.ReadBytes(16));
            }
            TypeHash = HashUtilities.UnityHashToGuid(reader.ReadBytes(16));
            
            if (hasTypeTree)
            {
                var fieldsCount = reader.ReadInt32();
                var stringTableLength = reader.ReadInt32();
                Fields = new List<TypeTreeItemField>(fieldsCount);
                for (var i = 0; i < fieldsCount; i++)
                {
                    Fields.Add(new TypeTreeItemField(reader, headerVersion));
                }
                StringTable = reader.ReadStringLength(stringTableLength);
                if (headerVersion >= 21)
                {
                    var dependenciesCount = reader.ReadInt32();
                    Dependencies = new List<int>(dependenciesCount);
                    for (var i = 0; i < dependenciesCount; i++)
                    {
                        Dependencies.Add(reader.ReadInt32());
                    }
                }
            }
        }
    }
}