using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipperShared.Utilities
{
    public interface IWorkerManager
    {
        IWorker GetWorker(UnityVersion version);
    }
}
