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

        public bool Contains(UnityVersion version)
        {
            return (HasMin || HasMax) && (!HasMin || min <= version) && (!HasMax || version < max);
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

        public override bool Equals(object obj)
        {
            return obj is UnityVersionRange range &&
                   min.Equals(range.min) &&
                   max.Equals(range.max);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(min, max);
        }

        public static bool operator ==(UnityVersionRange left, UnityVersionRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnityVersionRange left, UnityVersionRange right)
        {
            return !(left == right);
        }

        public static bool operator <(UnityVersionRange left, UnityVersionRange right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(UnityVersionRange left, UnityVersionRange right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(UnityVersionRange left, UnityVersionRange right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(UnityVersionRange left, UnityVersionRange right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
