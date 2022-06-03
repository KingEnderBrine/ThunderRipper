namespace ThunderRipperShared.YAML.Extensions
{
    public static class SequenceStyleExtensions
    {
        public static bool IsRaw(this SequenceStyle _this)
        {
            return _this == SequenceStyle.Raw;
        }

        public static bool IsAnyBlock(this SequenceStyle _this)
        {
            return _this == SequenceStyle.Block || _this == SequenceStyle.BlockCurve;
        }

        /// <summary>
        /// Get scalar style corresponding to current sequence style
        /// </summary>
        /// <param name="_this">Sequence style</param>
        /// <returns>Corresponding scalar style</returns>
        public static ScalarStyle ToScalarStyle(this SequenceStyle _this)
        {
            return _this == SequenceStyle.Raw ? ScalarStyle.Hex : ScalarStyle.Plain;
        }
    }
}
