using GZIPWin.Interfaces;
using System.IO;
using System.IO.Compression;

namespace GZIPWin.Services
{
    public class GzipDecompressService : IGzipService
    {
        public byte[] Process(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
