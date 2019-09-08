using System;
using System.Collections.Generic;
using System.Threading;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWin.Helpers
{
    public class ChunksKeeper : IChunksKeeper
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly Dictionary<int, Chunk> _savingChunks = new Dictionary<int, Chunk>();
        private readonly Queue<Chunk> _chunks = new Queue<Chunk>();

        public int ProcessedChunksLength => _savingChunks.Count;

        public int ChunksLength => _chunks.Count;

        public void AddChunk(Chunk chunk)
        {
            _chunks.Enqueue(chunk);
        }

        public Chunk GetChunk()
        {
            return _chunks.Count > 0 ? _chunks.Dequeue() : null;
        }

        public void AddProcessedChunk(Chunk chunk)
        {
            _lock.EnterWriteLock();
            _savingChunks.Add(chunk.Index, chunk);
            _lock.ExitWriteLock();
        }

        public Chunk GetProcessedChunk(int index)
        {
            _lock.EnterWriteLock();
            _savingChunks.TryGetValue(index, out var chunk);
            _savingChunks.Remove(index);
            _lock.ExitWriteLock();

            return chunk;
        }
    }
}
