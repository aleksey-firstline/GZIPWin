using System;
using System.Collections.Generic;
using System.IO;
using GZIPWin.Helpers;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;
using GZIPWin.Services;

namespace GZIPWin.Interfaces
{
    public interface IProcessService
    {
        IEnumerable<Chunk> ReadFile(Func<bool> condition, IFileReader fileReader);
        void SaveTo(IEnumerable<Chunk> chunks, IFileWriter fileWriter);
        Chunk ProcessChunk(Chunk chunk);
    }
}