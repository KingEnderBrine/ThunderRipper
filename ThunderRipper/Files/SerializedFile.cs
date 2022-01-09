using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThunderRipper.Assets;
using ThunderRipper.UnityClasses;
using ThunderRipper.Utilities;

namespace ThunderRipper.Files
{
    public class SerializedFile : IDisposable
    {
        public SerializedFileHeader Header { get; }
        public TypeTree TypeTree { get; }
        public List<AssetInfo> AssetTable { get; }
        public List<AssetPPtr> PreloadTable { get; }
        public List<SerializedFileDependency> Dependencies { get; }

        private readonly SerializedReader reader;

        public SerializedFile(Stream stream)
        {
            reader = new SerializedReader(stream);
            Header = new SerializedFileHeader(reader);
            TypeTree = new TypeTree(reader, Header.Version);
            AssetTable = ReadAssetTable();
            PreloadTable = ReadPreloadTable();
            Dependencies = ReadDependencies();
        }

        public AssetBase ReadAsset(long index)
        {
            var info = AssetTable.FirstOrDefault(el => el.Index == index);
            if (info == null)
            {
                return null;
            }
            
            reader.Position = Header.DataOffset + info.Offset;
            var assetType = TypeToClass.Dict[info.TypeID];
            var asset = Activator.CreateInstance(assetType) as AssetBase;
            asset.ReadBinary(reader);

            return asset;
        }

        private List<SerializedFileDependency> ReadDependencies()
        {
            var count = reader.ReadInt32();
            var list = new List<SerializedFileDependency>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(new SerializedFileDependency(reader, Header.Version));
            }

            return list;
        }

        private List<AssetPPtr> ReadPreloadTable()
        {
            var count = reader.ReadInt32();
            var list = new List<AssetPPtr>(count);
            if (Header.Version > 11)
            {
                for (var i = 0; i < count; i++)
                {
                    var item = new AssetPPtr()
                    {
                        FileID = reader.ReadInt32(),
                        PathID = reader.ReadInt64()
                    };

                    list.Add(item);
                }
            }

            return list;
        }

        private List<AssetInfo> ReadAssetTable()
        {
            var count = reader.ReadInt32();
            var list = new List<AssetInfo>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(new AssetInfo(reader, Header.Version, TypeTree));
            }

            return list;
        }

        public void Dispose()
        {
            reader?.Dispose();
        }
    }
}
