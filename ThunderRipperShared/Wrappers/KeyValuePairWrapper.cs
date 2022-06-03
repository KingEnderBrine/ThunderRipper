using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ThunderRipperShared.Assets;
using ThunderRipperShared.Utilities;
using ThunderRipperShared.YAML;

namespace ThunderRipperShared.Wrappers
{
    [DebuggerDisplay("{value}")]
    public struct KeyValuePairWrapper<TKey, TValue> :
        IBinaryReadable, IYAMLExportable
        where TKey : IBinaryReadable, IYAMLExportable, new()
        where TValue : IBinaryReadable, IYAMLExportable, new()
    {
        public KeyValuePair<TKey, TValue> value;

        public void ReadBinary(SerializedReader reader)
        {
            var key = new TKey();
            key.ReadBinary(reader);
            var value = new TValue();
            value.ReadBinary(reader);
            this.value = new KeyValuePair<TKey, TValue>(key, value);
        }

        public YAMLNode ExportYAML()
        {
            var node = new YAMLMappingNode();
            node.Add("first", value.Key.ExportYAML());
            node.Add("second", value.Value.ExportYAML());
            return node;
        }

        public static implicit operator KeyValuePairWrapper<TKey, TValue>(KeyValuePair<TKey, TValue> value) => new KeyValuePairWrapper<TKey, TValue>() { value = value };
        public static implicit operator KeyValuePair<TKey, TValue>(KeyValuePairWrapper<TKey, TValue> value) => value.value;
    }
}
