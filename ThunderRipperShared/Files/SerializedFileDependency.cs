using System;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Files
{
    public class SerializedFileDependency
    {
        public string EmptyString { get; set; }
        public Guid Guid { get; set; }
        public int Type { get; set; }
        public string Path { get; set; }
        public string OriginalPath { get; set; }

        public SerializedFileDependency(SerializedReader reader, uint headerVersion)
        {
            if (headerVersion >= 5)
            {
                if (headerVersion >= 6)
                {
                    EmptyString = reader.ReadNullTerminated();
                }
                Guid = HashUtilities.UnityHashToGuid(reader.ReadBytes(16));
                Type = reader.ReadInt32();
                OriginalPath = reader.ReadNullTerminated();
            }
            else
            {
                OriginalPath = reader.ReadNullTerminated();
            }

            if (OriginalPath == "resources/unity_builtin_extra")
            {
                Path = "Resources/unity_builtin_extra";
            }
            else if (OriginalPath == "library/unity default resources" || OriginalPath == "Library/unity default resources")
            {
                Path = "Resources/unity default resources";
            }
            else if (OriginalPath == "library/unity editor resources" || OriginalPath == "Library/unity editor resources")
            {
                Path = "Resources/unity editor resources";
            }
            else
            {
                Path = OriginalPath;
            }
        }
    }
}