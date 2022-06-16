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
#if CG || (u2018_4_16f1 && !u2019_4_26f1) || u2018_4_6f2 || (u2017_3_16f1 && !u2020_2_26f1) || !u2021_4_26f1 || (u2015_4_16f1 && !u2016_4_26f1)
			[1] = typeof(Object_V1),
#endif
			[2] = typeof(Object_V1)
		};
		public static readonly UnityVersion[] SupportedVersions = new UnityVersion[]
		{
			new UnityVersion("2018.4.16f1")
		};
    }
}
