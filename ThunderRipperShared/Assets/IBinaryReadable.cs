using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Utilities;

namespace ThunderRipperShared.Assets
{
    public interface IBinaryReadable
    {
        void ReadBinary(SerializedReader reader);
    }
}
