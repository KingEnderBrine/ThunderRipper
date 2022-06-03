using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipperShared.YAML.Extensions
{
    public static class YAMLMappingNodeExtensions
    {
        public const string SerializedVersionName = "serializedVersion";

        public static void AddSerializedVersion(this YAMLMappingNode _this, int version)
        {
            if (version > 1)
            {
                _this.Add(SerializedVersionName, version);
            }
        }

        public static void ForceAddSerializedVersion(this YAMLMappingNode _this, int version)
        {
            if (version > 0)
            {
                _this.Add(SerializedVersionName, version);
            }
        }

        public static void InsertSerializedVersion(this YAMLMappingNode _this, int version)
        {
            if (version > 1)
            {
                _this.InsertBegin(SerializedVersionName, version);
            }
        }
    }
}
