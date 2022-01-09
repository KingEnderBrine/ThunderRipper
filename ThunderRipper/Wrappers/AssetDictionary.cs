using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipper.Assets;
using ThunderRipper.Utilities;

namespace ThunderRipper.Wrappers
{
    public class AssetDictionary<TKey, TValue> : 
        Dictionary<TKey, TValue>, IBinaryReadable 
        where TKey : IBinaryReadable, new()
        where TValue : IBinaryReadable, new()
    {
        public void ReadBinary(SerializedReader reader)
        {
            Clear();
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var key = new TKey();
                key.ReadBinary(reader);
                var value = new TValue();
                value.ReadBinary(reader);
                Add(key, value);
            }
        }
    }
}
