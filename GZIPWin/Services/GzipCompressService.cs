using System.IO;
using System.IO.Compression;
using GZIPWin.Interfaces;

namespace GZIPWin.Services
{
    public class GzipCompressService : IGzipService
    {
        public byte[] Process(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
    }
}
