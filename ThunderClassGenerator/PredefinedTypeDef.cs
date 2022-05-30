using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class PredefinedTypeDef : SimpleTypeDef
    {
        public static PredefinedTypeDef String { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "StringWrapper" };
        public static PredefinedTypeDef Int { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "IntWrapper" };
        public static PredefinedTypeDef Byte { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "ByteWrapper" };
        public static PredefinedTypeDef Long { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "LongWrapper" };
        public static PredefinedTypeDef Char { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "CharWrapper" };
        public static PredefinedTypeDef Float { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "FloatWrapper" };
        public static PredefinedTypeDef Bool { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "BoolWrapper" };
        public static PredefinedTypeDef Double { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "DoubleWrapper" };
        public static PredefinedTypeDef SByte { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "SByteWrapper" };
        public static PredefinedTypeDef Short { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "ShortWrapper" };
        public static PredefinedTypeDef UShort { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "UShortWrapper" };
        public static PredefinedTypeDef UInt { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "UIntWrapper" };
        public static PredefinedTypeDef ULong { get; } = new PredefinedTypeDef { Namespace = Strings.ThunderRipperWrappers, Name = "ULongWrapper" };

        public static PredefinedTypeDef KeyValuePair { get; } = new PredefinedTypeDef
        {
            Namespace = Strings.ThunderRipperWrappers,
            Name = "KeyValuePairWrapper",
            GenericCount = 2,
            Fields =
            {
                ["first"] = new FieldDef
                {
                    Type = new TypeUsageDef
                    {
                        GenericIndex = 0,
                    },
                    Name = "first",
                },
                ["second"] = new FieldDef
                {
                    Type = new TypeUsageDef
                    {
                        GenericIndex = 1,
                    },
                    Name = "second",
                }
            }
        };

        public static PredefinedTypeDef List { get; } = new PredefinedTypeDef
        {
            Namespace = Strings.ThunderRipperWrappers,
            Name = "AssetList",
            GenericCount = 1,
            Fields =
            {
                ["data"] = new FieldDef
                {
                    Type = new TypeUsageDef
                    {
                        GenericIndex = 0,
                    },
                    Name = "data",
                },
            }
        };

        public override string VersionnedName => Name;

        public PredefinedTypeDef()
        {
            Done = true;
        }
    }
}
