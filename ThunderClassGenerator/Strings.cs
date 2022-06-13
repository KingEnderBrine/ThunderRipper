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
        public static string DefaultNamespace { get; } = "ThunderRipperWorker";
        public static string SharedNamespace { get; } = "ThunderRipperShared";
        public static string OutputComponentNamespace { get; } = $"{DefaultNamespace}.UnityComponents";
        public static string OutputClassNamespace { get; } = $"{DefaultNamespace}.UnityClasses";
        public static string CollectionsGeneric { get; } = "System.Collections.Generic";
        public static string ThunderRipperAttributes { get; } = $"{SharedNamespace}.Attributes";
        public static string ThunderRipperAssets { get; } = $"{SharedNamespace}.Assets";
        public static string ThunderRipperUtilities { get; } = $"{SharedNamespace}.Utilities";
        public static string ThunderRipperWrappers { get; } = $"{SharedNamespace}.Wrappers";
        public static string ThunderRipperYAML { get; } = $"{SharedNamespace}.YAML";
        public static string ThunderRipperYAMLExtensions { get; } = $"{ThunderRipperYAML}.Extensions";
        
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
