using System.IO;
using GZIPWin.Interfaces.Helpers;

namespace GZIPWin.Helpers
{
    public class FileReader : IFileReader
    {
        private readonly FileStream _fileStream;
        private readonly BinaryReader _binaryReader;

        public FileReader(FileInfo file)
        {
            _fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            _binaryReader = new BinaryReader(_fileStream);
        }

        public long FileLength => _fileStream?.Length ?? 0;

        public byte[] Read(long offset, int count)
        {
            _fileStream?.Seek(offset, SeekOrigin.Begin);
            return _binaryReader?.ReadBytes(count);
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _binaryReader?.Dispose();
        }
    }
}

