using System;
using System.Linq.Expressions;
using ThunderRipperShared.Utilities;
using ThunderRipperShared.YAML;
using ThunderRipperShared.YAML.Extensions;

namespace ThunderRipperShared.Assets
{
    public abstract class AssetBase : IBinaryReadable, IYAMLExportable
    {
        private static readonly Action<AssetBase, SerializedReader> readBinaryAction = (asset, reader) => { };
        protected virtual Action<AssetBase, SerializedReader> ReadBinaryAction => readBinaryAction;

        public abstract MappingStyle MappingStyle { get; }
        public abstract int Version { get; }

        public virtual void ReadBinary(SerializedReader reader)
        {
            ReadBinaryAction(this, reader);
        }

        public virtual YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.AddSerializedVersion(Version);

            return node;
        }

        protected static Action<AssetBase, SerializedReader> CompileReadBinary(Expression<Action<AssetBase, SerializedReader>>[] sortedActions)
        {
            return Expression.Lambda<Action<AssetBase, SerializedReader>>(Expression.Block(sortedActions), Expression.Parameter(typeof(AssetBase)), Expression.Parameter(typeof(SerializedReader))).Compile();
        }
    }
}
