using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Utilities;
using ThunderRipperShared.YAML;
using ThunderRipperShared.YAML.Extensions;

namespace ThunderRipperShared.Assets
{
    public abstract class AssetBase : IBinaryReadable, IYAMLExportable
    {
        public abstract MappingStyle MappingStyle { get; }
        public abstract int Version { get; }

        public virtual void ReadBinary(SerializedReader reader) { }

        public virtual YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(Version);

            return node;
        }
    }
}
