using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class FieldDef
    {
        public string Name { get; set; }
        public SimpleTypeDef Type { get; set; }
        public int GenericIndex { get; set; }
        public int MetaFlags { get; set; }
    }
}
