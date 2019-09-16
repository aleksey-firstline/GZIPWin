using System;
using System.Collections.Generic;
using System.Threading;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWin.Helpers
{
    public class ChunksKeeper : IChunksKeeper
    {
        private readonly Dictionary<int, Chunk> _savingChunks = new Dictionary<int, Chunk>();
        private readonly Queue<Chunk> _chunks = new Queue<Chunk>();

        private readonly object _locker = new object();

        public int ProcessedChunksLength
        {
            get
            {
                lock (_locker)
                {
                    return _savingChunks.Count;
                }
            }
        }

        public int ChunksLength => _chunks.Count;

        public void AddChunk(Chunk chunk)
        {
            _chunks.Enqueue(chunk);
        }

        public Chunk GetChunk()
        {
            var chunk = _chunks.Count > 0 ? _chunks.Dequeue() : null;
            return chunk;
        }

        public void AddProcessedChunk(Chunk chunk)
        {
            lock (_locker)
            {
                _savingChunks.Add(chunk.Index, chunk);
            }
        }

        public Chunk GetProcessedChunk(int index)
        {
            lock (_locker)
            {
                if (_savingChunks.TryGetValue(index, out var chunk))
                {
                    _savingChunks.Remove(index);
                }
                return chunk;
            }
        }

        public bool ContainsProcessedChunk(int index)
        {
            lock (_locker)
            {
                var contains = _savingChunks.ContainsKey(index);
                return contains;
            }
        }
    }
}