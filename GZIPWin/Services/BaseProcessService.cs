using System;
using System.Threading;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWin.Services
{
    public abstract class BaseProcessService : IProcessService
    {
        protected const int Int64Size = 8;
        protected const int Int32Size = 4;
        protected const int MaxUnsavedChunks = 50;
        protected const int MaxChunks = 10;
        protected const int ChunkSize = 1048576;

        protected readonly IChunksKeeper _chunksKeeper;
        private readonly IGzipService _gzipService;

        protected BaseProcessService(IGzipService gzipService, IChunksKeeper chunksKeeper)
        {
            _gzipService = gzipService;
            _chunksKeeper = chunksKeeper;
        }

        public Chunk ProcessChunk(Chunk chunk)
        {
            var bytes = _gzipService?.Process(chunk.Buffer);
            var processedChunk =  new Chunk(chunk.Index, bytes, chunk.Length, chunk.FileLength);

            return processedChunk;
        }

        public abstract void ReadFile(Func<bool> condition, IFileReader fileReader);

        public abstract void SaveTo(Func<bool> condition, IFileWriter fileWriter);

        protected void Wait()
        {
            SpinWait.SpinUntil(() => _chunksKeeper.ChunksLength < MaxChunks);
            SpinWait.SpinUntil(() => _chunksKeeper.ProcessedChunksLength < MaxUnsavedChunks);
        }
    }
}
