using System;
using System.IO;
using GZIPWin.Helpers;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;
using GZIPWin.Services;

namespace GZIPWin.Interfaces
{
    public interface IProcessService
    {
        void ReadFile(Func<bool> condition, IFileReader fileReader);
        void SaveTo(Func<bool> condition, IFileWriter fileWriter);
        Chunk ProcessChunk(Chunk chunk);
    }
}