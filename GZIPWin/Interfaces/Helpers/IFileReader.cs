using System;

namespace GZIPWin.Interfaces.Helpers
{
    public interface IFileReader : IDisposable
    {
        long FileLength { get; }
        byte[] Read(long offset, int count);
    }
}