﻿namespace ThunderRipperShared.YAML
{
    /// <summary>
    /// Specifies the style of a sequence.
    /// </summary>
    public enum SequenceStyle
    {
        /// <summary>
        /// The block sequence style
        /// </summary>
        Block,

        /// <summary>
        /// The block sequence style but with curly braces
        /// </summary>
        BlockCurve,

        /// <summary>
        /// The flow sequence style
        /// </summary>
        Flow,

        /// <summary>
        /// Single line with hex data
        /// </summary>
        Raw,
    }
}
