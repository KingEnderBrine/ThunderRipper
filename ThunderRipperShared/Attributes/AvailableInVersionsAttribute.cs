using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipperShared.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AvailableInVersionsAttribute : Attribute
    {
        public string From { get; private set; }
        public string To { get; private set; }

    }
}
