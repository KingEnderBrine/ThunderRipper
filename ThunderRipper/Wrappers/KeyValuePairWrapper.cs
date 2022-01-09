using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ThunderRipper.Assets;
using ThunderRipper.Utilities;

namespace ThunderRipper.Wrappers
{
    [DebuggerDisplay("{value}")]
    public struct KeyValuePairWrapper<TKey, TValue> :
        IBinaryReadable
        where TKey : IBinaryReadable, new()
        where TValue : IBinaryReadable, new()
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
        public static implicit operator KeyValuePairWrapper<TKey, TValue>(KeyValuePair<TKey, TValue> value) => new KeyValuePairWrapper<TKey, TValue>() { value = value };
        public static implicit operator KeyValuePair<TKey, TValue>(KeyValuePairWrapper<TKey, TValue> value) => value.value;
    }
}
