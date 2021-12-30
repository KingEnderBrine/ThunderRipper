using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class TypeUsageDef
    {
        public SimpleTypeDef Type { get; set; }
        public List<TypeUsageDef> GenericArgs { get; } = new();
        public int GenericIndex { get; set; }
    }
}
