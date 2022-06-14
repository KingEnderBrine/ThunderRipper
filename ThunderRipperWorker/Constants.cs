using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Utilities;

namespace ThunderRipperWorker
{
    public static class Constants
    {
        public static readonly Dictionary<int, Type> TypeIDToType = new Dictionary<int, Type>
        {
        	[0] = typeof(Object_V1),
        	[0] = typeof(Object_V1),
        	[1111111111] = typeof(int),
        	[1111111111] = typeof(int)
        };
    }
}
