using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipper.Utilities;

namespace ThunderRipper.Assets
{
    public interface IBinaryReadable
    {
        void ReadBinary(SerializedReader reader);
    }
}
