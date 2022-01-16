using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class PredefinedTypeDef : SimpleTypeDef
    {
        public static PredefinedTypeDef String { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "StringWrapper" };
        public static PredefinedTypeDef Int { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "IntWrapper" };
        public static PredefinedTypeDef Byte { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "ByteWrapper" };
        public static PredefinedTypeDef Long { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "LongWrapper" };
        public static PredefinedTypeDef Char { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "CharWrapper" };
        public static PredefinedTypeDef Float { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "FloatWrapper" };
        public static PredefinedTypeDef Bool { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "BoolWrapper" };
        public static PredefinedTypeDef Double { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "DoubleWrapper" };
        public static PredefinedTypeDef SByte { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "SByteWrapper" };
        public static PredefinedTypeDef Short { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "ShortWrapper" };
        public static PredefinedTypeDef UShort { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "UShortWrapper" };
        public static PredefinedTypeDef UInt { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "UIntWrapper" };
        public static PredefinedTypeDef ULong { get; } = new PredefinedTypeDef { Namespace = "ThunderRipper.Wrappers", Name = "ULongWrapper" };

        public static PredefinedTypeDef KeyValuePair { get; } = new PredefinedTypeDef
        {
            Namespace = "ThunderRipper.Wrappers",
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
            Namespace = "ThunderRipper.Wrappers",
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

        //public static PredefinedTypeDef[] All { get; } = typeof(PredefinedTypeDef).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(el => el.PropertyType == typeof(PredefinedTypeDef)).Select(el => el.GetValue(null) as PredefinedTypeDef).ToArray();

        public override string VersionnedName => Name;

        public PredefinedTypeDef()
        {
            Done = true;
        }
    }
}
