using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRipperShared.Utilities;

namespace ThunderClassGenerator.Utilities
{
    [DebuggerDisplay("{ToDebugString()}")]
    public readonly struct UnityVersionRange : IComparable<UnityVersionRange>
    {
        public readonly UnityVersion min;
        public readonly UnityVersion max;

        public bool HasMin => min != default;
        public bool HasMax => max != default;

        public UnityVersionRange(UnityVersion min, UnityVersion max) : this()
        {
            this.min = min;
            this.max = max;
        }

        private string ToDebugString()
        {
            if (HasMin && HasMax)
            {
                return $"From {min} to {max}";
            }
            if (HasMin)
            {
                return $"From {min}";
            }
            if (HasMax)
            {
                return $"To {max}";
            }

            return "None";
        }

        public int CompareTo(UnityVersionRange other)
        {
            if (HasMin && other.HasMin)
            {
                return min.CompareTo(other.min);
            }
            if (HasMax && other.HasMax)
            {
                return max.CompareTo(other.max);
            }
            if (HasMin && other.HasMax)
            {
                return min.CompareTo(other.max);
            }
            if (HasMax && other.HasMin)
            {
                return max.CompareTo(other.min);
            }

            return 0;
        }
    }
}
