using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class GenericDef
    {
        public SimpleTypeDef TypeDef { get; set; }
        public List<GenericDef> GenericArgs { get; } = new();
    }
}
