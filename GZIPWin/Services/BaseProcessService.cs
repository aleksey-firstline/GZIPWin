using System;
using System.Collections.Generic;
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
        protected const int MaxUnsavedChunks = 2;
        protected const int MaxChunks = 2;
        protected const int ChunkSize = 1048576;

        private readonly IChunksKeeper _chunksKeeper;
        private readonly IGzipService _gzipService;

        protected BaseProcessService(IGzipService gzipService, IChunksKeeper chunksKeeper)
        {
            _gzipService = gzipService;
            _chunksKeeper = chunksKeeper;
        }

        public Chunk ProcessChunk(Chunk chunk)
        {
            var bytes = _gzipService?.Process(chunk.Buffer);
            var processedChunk = new Chunk(chunk.Index, bytes, chunk.Length, chunk.FileLength);

            return processedChunk;
        }

        public abstract IEnumerable<Chunk> ReadFile(Func<bool> condition, IFileReader fileReader);

        public abstract void SaveTo(IEnumerable<Chunk> chunks, IFileWriter fileWriter);

        protected void Wait()
        {
            SpinWait.SpinUntil(() => _chunksKeeper.ChunksLength < MaxChunks, TimeSpan.FromSeconds(1));
            SpinWait.SpinUntil(() => _chunksKeeper.ProcessedChunksLength < MaxUnsavedChunks, TimeSpan.FromSeconds(1));
        }
    }
}