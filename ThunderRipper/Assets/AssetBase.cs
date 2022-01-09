using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipper.Utilities;

namespace ThunderRipper.Assets
{
    public abstract class AssetBase : IBinaryReadable
    {
        public abstract int Version { get; }

        public virtual void ReadBinary(SerializedReader reader) { }
    }
}
