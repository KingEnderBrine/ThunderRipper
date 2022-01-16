using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ThunderRipperShared.Utilities
{
    public static class Constants
    {
        public static readonly string RelativeWorkersPath = Path.Combine("Workers");
        public static readonly string ThunderRipperWorker = nameof(ThunderRipperWorker);
    }
}
