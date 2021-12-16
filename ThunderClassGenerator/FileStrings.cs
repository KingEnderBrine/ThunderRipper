using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderClassGenerator
{
    public static class FileStrings
    {
        public static string Namespace { get; set; } = "ThunderRipper.Unity";

        public static string UtilitiesFileHeader { get; } =
$@"
using System.Collections.Generic;

namespace {Namespace}
{{
    partial class Utilities
    {{";
        public static string UtilitiesFileFooter { get; } =
$@"    }}
}}";
    }
}