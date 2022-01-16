using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipperShared.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AlignAttribute : Attribute
    {
    }
}
