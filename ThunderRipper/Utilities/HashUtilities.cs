using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThunderRipper.Utilities
{
    public static class HashUtilities
    {
        public static Guid UnityHashToGuid(uint first, uint second, uint third, uint fourth) => UnityHashToGuid(new[] { first, second, third, fourth });

        public static Guid UnityHashToGuid(IEnumerable<uint> data) => UnityHashToGuid(data.SelectMany(BitConverter.GetBytes).ToArray());

        public static Guid UnityHashToGuid(byte[] data)
        {
            return new Guid(
                ReverseHalfs(data[0]) << 24 |
                ReverseHalfs(data[1]) << 16 |
                ReverseHalfs(data[2]) << 8 |
                ReverseHalfs(data[3]),

                (short)(
                ReverseHalfs(data[4]) << 8 |
                ReverseHalfs(data[5])),
                (short)(
                ReverseHalfs(data[6]) << 8 |
                ReverseHalfs(data[7])),

                ReverseHalfs(data[8]),
                ReverseHalfs(data[9]),
                ReverseHalfs(data[10]),
                ReverseHalfs(data[11]),
                ReverseHalfs(data[12]),
                ReverseHalfs(data[13]),
                ReverseHalfs(data[14]),
                ReverseHalfs(data[15])
            );
        }

        private static byte ReverseHalfs(byte b)
        {
            return (byte)(b >> 4 | b << 4);
        }
    }
}
