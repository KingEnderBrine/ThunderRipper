namespace ThunderRipperShared.YAML.Extensions
{
    public static class PrimitiveExtensions
    {
        public static string ToHexString(this byte _this)
        {
            return _this.ToString("x2");
        }

        public static string ToHexString(this short _this)
        {
            var value = unchecked((ushort)_this);
            return value.ToHexString();
        }

        public static string ToHexString(this ushort _this)
        {
            var reverse = unchecked((ushort)((0xFF00 & _this) >> 8 | (0x00FF & _this) << 8));
            return reverse.ToString("x4");
        }

        public static string ToHexString(this int _this)
        {
            var value = unchecked((uint)_this);
            return value.ToHexString();
        }

        public static string ToHexString(this uint _this)
        {
            uint reverse = (0xFF000000 & _this) >> 24 | (0x00FF0000 & _this) >> 8 | (0x0000FF00 & _this) << 8 | (0x000000FF & _this) << 24;
            return reverse.ToString("x8");
        }

        public static string ToHexString(this long _this)
        {
            var value = unchecked((ulong)_this);
            return value.ToHexString();
        }

        public static string ToHexString(this ulong _this)
        {
            var reverse = (_this & 0x00000000000000FFUL) << 56 | (_this & 0x000000000000FF00UL) << 40 |
                    (_this & 0x0000000000FF0000UL) << 24 | (_this & 0x00000000FF000000UL) << 8 |
                    (_this & 0x000000FF00000000UL) >> 8 | (_this & 0x0000FF0000000000UL) >> 24 |
                    (_this & 0x00FF000000000000UL) >> 40 | (_this & 0xFF00000000000000UL) >> 56;
            return reverse.ToString("x16");
        }

        public static string ToHexString(this float _this)
        {
            var value = BitConverterExtensions.ToUInt32(_this);
            return value.ToHexString();
        }

        public static string ToHexString(this double _this)
        {
            var value = BitConverterExtensions.ToUInt64(_this);
            return value.ToHexString();
        }
    }
}
