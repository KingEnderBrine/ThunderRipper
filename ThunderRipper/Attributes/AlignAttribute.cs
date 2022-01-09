using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderRipper.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AlignAttribute : Attribute
    {
    }
}
