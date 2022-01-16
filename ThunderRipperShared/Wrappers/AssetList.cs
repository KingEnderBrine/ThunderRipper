using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Assets;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Wrappers
{
    public class AssetList<T> : 
        List<T>, IBinaryReadable
        where T : IBinaryReadable, new()
    {
        public void ReadBinary(SerializedReader reader)
        {
            Clear();
            var count = reader.ReadInt32();
            Capacity = count;
            for (var i = 0; i < count; i++)
            {
                var item = new T();
                item.ReadBinary(reader);
                Add(item);
            }
            if (typeof(byte).IsAssignableFrom(typeof(T)))
            {
                reader.Align();
            }
        }
    }
}
