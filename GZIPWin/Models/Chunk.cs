namespace GZIPWin.Models
{
    public class Chunk
    {
        public Chunk(int index, byte[] buffer, int length, long fileLength)
        {
            Index = index;
            Buffer = buffer;
            Length = length;
            FileLength = fileLength;
        }

        public int Index { get; }
        public byte[] Buffer { get; }
        public int Length { get; }
        public long FileLength { get; }
    }
}
