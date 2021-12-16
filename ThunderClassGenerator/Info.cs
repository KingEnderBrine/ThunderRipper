using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class Info
    {
        public string Version { get; set; }
        public List<StringInfo> Strings { get; set; }
        public List<UnityClass> Classes { get; set; }
    }
}
