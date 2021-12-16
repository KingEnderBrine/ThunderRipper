using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public static class Strings
    {
        public static string DefaultNamespace { get; } = "ThunderRipper";
        public static string OutputNamespace { get; } = $"{DefaultNamespace}.UnityClasses";
        public static string CollectionsGeneric { get; } = "System.Collections.Generic";
        public static string ThunderRipperAttributes { get; } = $"{DefaultNamespace}.Attributes";
        public static string ThunderRipperAssets { get; } = $"{DefaultNamespace}.Assets";
        public static string IAsset { get; } = "IAsset";
#warning TODO: improve path discovery
        public static string SolutionFolder { get; set; } = Path.Combine("..", "..", "..", "..");
    }
}
