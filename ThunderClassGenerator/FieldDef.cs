using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    [DebuggerDisplay("{Name} ({Type})")]
    public class FieldDef
    {
        public string Name { get; set; }
        public TypeUsageDef Type { get; set; }
        public bool ExistsInBase { get; set; }
        public int FixedLength { get; set; } = -1;
    }
}
