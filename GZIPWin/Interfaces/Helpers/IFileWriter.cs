using System;

namespace GZIPWin.Interfaces.Helpers
{
    public interface IFileWriter : IDisposable
    {
        void Write(byte[] array, int count);
    }
}