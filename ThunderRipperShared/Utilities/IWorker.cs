using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Assets;

namespace ThunderRipperShared.Utilities
{
    public interface IWorker : IDisposable
    {
        Type GetTypeForAssetType(long typeID);
    }
}
