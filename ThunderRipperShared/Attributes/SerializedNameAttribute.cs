using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipperShared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializedNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public SerializedNameAttribute(string name)
        {
            Name = name;
        }
    }
}
