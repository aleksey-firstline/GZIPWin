using GZIPWin.Models;

namespace GZIPWin.Interfaces.Helpers
{
    public interface IChunksKeeper
    {
        int ProcessedChunksLength { get; }
        int ChunksLength { get; }
        void AddChunk(Chunk chunk);
        void AddProcessedChunk(Chunk chunk);
        Chunk GetChunk();
        Chunk GetProcessedChunk(int index);
    }
}