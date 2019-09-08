using System.IO;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Services;

namespace GZIPWin.Helpers
{
    public class FileWriter : IFileWriter
    {
        private readonly FileStream _fileStream;
        public FileWriter(FileInfo file)
        {
            _fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write);
        }

        public void Write(byte[] array, int count)
        {
            _fileStream?.Write(array, 0, count);
        }

        public void Dispose()
        {
            _fileStream?.Flush();
            _fileStream?.Dispose();
        }
    }
}