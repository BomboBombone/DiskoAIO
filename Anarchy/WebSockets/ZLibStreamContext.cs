using Ionic.Zlib;
using System;

namespace DiskoAIO.Anarchy.WebSockets.Gateway
{
    public class ZlibStreamContext
    {
        private ZlibCodec _inflator;

        public ZlibStreamContext(bool expectRFC1950Header = false)
        {
            _inflator = new ZlibCodec();
            _inflator.InitializeInflate(expectRFC1950Header);
        }

        public byte[] InflateByteArray(byte[] deflatedBytes)
        {
            _inflator.InputBuffer = deflatedBytes;
            _inflator.AvailableBytesIn = deflatedBytes.Length;
            // account for a lot of possible size inflation (could be much larger than 4x)
            _inflator.OutputBuffer = new byte[deflatedBytes.Length * 100];
            _inflator.AvailableBytesOut = _inflator.OutputBuffer.Length;
            _inflator.NextIn = 0;
            _inflator.NextOut = 0;

            _inflator.Inflate(FlushType.Sync);
            byte[] target = new byte[_inflator.NextOut];
            Array.Copy(_inflator.OutputBuffer, target, _inflator.NextOut);
            return target;
        }
    }
}
