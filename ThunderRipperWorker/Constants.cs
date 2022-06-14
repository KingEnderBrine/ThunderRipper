using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.Utilities;

namespace ThunderRipperWorker
{
    public static class Constants
    {
        public static readonly Dictionary<long, Type> TypeIDToType = new Dictionary<long, Type>
        {
        };
		public static readonly UnityVersion[] SupportedVersions = new UnityVersion[]

		{
			new UnityVersion("2018.4.16f1"),
			new UnityVersion("2018.4.16f1"),
			new UnityVersion("2018.4.16f1")
		};
    }
}
