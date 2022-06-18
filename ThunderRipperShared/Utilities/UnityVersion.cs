using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ThunderRipperShared.Utilities
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UnityVersion : IEquatable<UnityVersion>, IComparable<UnityVersion>
    {
        private static readonly Regex versionRegex = new Regex(@"u?(?<major>\d+)[\._](?<minor>\d+)[\._](?<build>\d+)((?<type>[A-Za-z])(?<typeNumber>\d+))?", RegexOptions.Compiled);
        public enum VersionType
        {
            Unknown,
            Alpha,
            Beta,
            Final,
            Patch
        }

        [FieldOffset(0)]
        private readonly ulong m_data;

        [FieldOffset(6)]
        private readonly ushort major;
        [FieldOffset(4)]
        private readonly ushort minor;
        [FieldOffset(2)]
        private readonly ushort build;
        [FieldOffset(1)]
        private readonly byte type;
        [FieldOffset(0)]
        private readonly byte typeNumber;

        public int Major => major;
        public int Minor => minor;
        public int Build => build;
        public VersionType Type => (VersionType)type;
        public int TypeNumber => typeNumber;
        public UnityVersion WithoutType => new UnityVersion(Major, Minor, Build);

        private UnityVersion(Match match) : this()
        {
            if (!match.Success)
            {
                return;
            }
            major = ushort.Parse(match.Groups["major"].Value);
            minor = ushort.Parse(match.Groups["minor"].Value);
            build = ushort.Parse(match.Groups["build"].Value);
            type = (byte)(match.Groups["type"].Success ? TypeFromLiteral(match.Groups["type"].Value) : VersionType.Unknown);
            typeNumber = (byte)(match.Groups["typeNumber"].Success ? byte.Parse(match.Groups["typeNumber"].Value) : 0);
        }

        public UnityVersion(string version) : this(versionRegex.Match(version))
        {
            if (this == default)
            {
                throw new ArgumentException("Version is not in the correct format");
            }
        }

        public UnityVersion(int major, int minor, int build) : this(major, minor, build, VersionType.Unknown, 0) { }

        public UnityVersion(int major, int minor, int build, VersionType type, int typeNumber) : this()
        {
            this.major = (ushort)major;
            this.minor = (ushort)minor;
            this.build = (ushort)build;
            this.type = (byte)type;
            this.typeNumber = (byte)typeNumber;
        }

        public static bool TryParse(string input, out UnityVersion version)
        {
            var result = versionRegex.Match(input);
            if (!result.Success)
            {
                version = default;
                return false;
            }
            version = new UnityVersion(result);
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is UnityVersion version && Equals(version);
        }

        public bool Equals(UnityVersion other)
        {
            return m_data == other.m_data;
        }

        public override int GetHashCode()
        {
            return 1064174093 + m_data.GetHashCode();
        }

        public static bool operator ==(UnityVersion left, UnityVersion right) => left.Equals(right);
        public static bool operator !=(UnityVersion left, UnityVersion right) => left.m_data != right.m_data;
        public static bool operator >(UnityVersion left, UnityVersion right) => left.m_data > right.m_data;
        public static bool operator <(UnityVersion left, UnityVersion right) => left.m_data < right.m_data;
        public static bool operator >=(UnityVersion left, UnityVersion right) => left.m_data >= right.m_data;
        public static bool operator <=(UnityVersion left, UnityVersion right) => left.m_data <= right.m_data;

        public int CompareTo(UnityVersion other) => m_data == other.m_data ? 0 : m_data < other.m_data ? -1 : 1;

        public override string ToString()
        {
            return Type == VersionType.Unknown ? $"{Major}.{Minor}.{Build}" : $"{Major}.{Minor}.{Build}{TypeToLiteral(Type)}{TypeNumber}";
        }

        public string ToDirectiveString()
        {
            return Type == VersionType.Unknown ? $"u{Major}_{Minor}_{Build}" : $"u{Major}_{Minor}_{Build}{TypeToLiteral(Type)}{TypeNumber}";
        }

        private static string TypeToLiteral(VersionType type)
        {
            switch (type)
            {
                case VersionType.Alpha:
                    return "a";
                case VersionType.Beta:
                    return "b";
                case VersionType.Final:
                    return "f";
                case VersionType.Patch:
                    return "p";
                default:
                    throw new Exception($"Unsupported vertion type {type}");
            }
        }

        private static VersionType TypeFromLiteral(string literal)
        {
            switch (literal.ToLower())
            {
                case "a":
                    return VersionType.Alpha;
                case "b":
                    return VersionType.Beta;
                case "f":
                    return VersionType.Final;
                case "p":
                    return VersionType.Patch;
                default:
                    throw new Exception($"Unsupported vertion type {literal}");
            }
        }
    }
}
