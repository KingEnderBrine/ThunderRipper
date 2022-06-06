using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderClassGenerator.Extensions;

namespace ThunderClassGenerator
{
    [DebuggerDisplay("{FullName}")]
    public class TypeUsageDef
    {
        public SimpleTypeDef Type { get; set; }
        public List<TypeUsageDef> GenericArgs { get; } = new();
        public int GenericIndex { get; set; } = -1;
        public int MetaFlags { get; set; }

        public string FullName
        {
            get
            {
                if (GenericIndex != -1)
                {
                    return $"T{GenericIndex + 1}";
                }
                if (Type.GenericCount == 0)
                {
                    return Type.VersionnedName;
                }

                return $"{Type.VersionnedName}<{string.Join(", ", GenericArgs.Select(el => el.FullName))}>";
            }
        }

        public bool Equals(TypeUsageDef other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Type.Equals(other.Type) && GenericIndex == other.GenericIndex && GenericArgs.Count == other.GenericArgs.Count && GenericArgs.All((el, i) => el.Equals(other.GenericArgs[i]));
        }
    }
}
