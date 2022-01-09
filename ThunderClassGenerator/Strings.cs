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
        public static string ThunderRipperUtilities { get; } = $"{DefaultNamespace}.Utilities";
        public static string AssetBase { get; } = "AssetBase";
#warning TODO: improve path discovery
        public static string SolutionFolder { get; } = Path.Combine("..", "..", "..", "..");
        public static string CreatedWithComment { get; } =
$@"//------------------------------
//This class is managed by {nameof(ThunderClassGenerator)}
//Don't do any modifications in this file by hand
//------------------------------";
    }
}
