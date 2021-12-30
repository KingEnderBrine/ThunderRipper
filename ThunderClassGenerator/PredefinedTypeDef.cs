using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class PredefinedTypeDef : SimpleTypeDef
    {
        public static PredefinedTypeDef String { get; } = new PredefinedTypeDef { Name = "string" };
        public static PredefinedTypeDef Int { get; } = new PredefinedTypeDef { Name = "int" };
        public static PredefinedTypeDef Byte { get; } = new PredefinedTypeDef { Name = "byte" };
        public static PredefinedTypeDef Long { get; } = new PredefinedTypeDef { Name = "long" };
        public static PredefinedTypeDef Char { get; } = new PredefinedTypeDef { Name = "char" };
        public static PredefinedTypeDef Float { get; } = new PredefinedTypeDef { Name = "float" };
        public static PredefinedTypeDef Bool { get; } = new PredefinedTypeDef { Name = "bool" };
        public static PredefinedTypeDef Double { get; } = new PredefinedTypeDef { Name = "double" };
        public static PredefinedTypeDef SByte { get; } = new PredefinedTypeDef { Name = "sbyte" };
        public static PredefinedTypeDef Short { get; } = new PredefinedTypeDef { Name = "short" };
        public static PredefinedTypeDef UShort { get; } = new PredefinedTypeDef { Name = "ushort" };
        public static PredefinedTypeDef UInt { get; } = new PredefinedTypeDef { Name = "uint" };
        public static PredefinedTypeDef ULong { get; } = new PredefinedTypeDef { Name = "ulong" };
        
        public static PredefinedTypeDef KeyValuePair { get; } = new PredefinedTypeDef
        {
            Namespace = "System.Collections.Generic",
            Name = "KeyValuePair",
            GenericCount = 2,
        };

        public static PredefinedTypeDef Dictionary { get; } = new PredefinedTypeDef
        {
            Namespace = "System.Collections.Generic",
            Name = "Dictionary",
            GenericCount = 2,
        };

        public static PredefinedTypeDef HashSet { get; } = new PredefinedTypeDef
        {
            Namespace = "System.Collections.Generic",
            Name = "HashSet",
            GenericCount = 1,
        };

        public static PredefinedTypeDef List { get; } = new PredefinedTypeDef
        {
            Namespace = "System.Collections.Generic",
            Name = "List",
            GenericCount = 1,
        };

        //public static PredefinedTypeDef[] All { get; } = typeof(PredefinedTypeDef).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(el => el.PropertyType == typeof(PredefinedTypeDef)).Select(el => el.GetValue(null) as PredefinedTypeDef).ToArray();

        public override string VersionnedName => Name;

        public PredefinedTypeDef()
        {
            Done = true;
        }
    }
}
