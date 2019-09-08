using System;
using System.Linq;
using GZIPWin.Exceptions;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWin.Services
{
    public class DecompressService : BaseProcessService
    {

        public DecompressService(IGzipService gzipService, IChunksKeeper chunksKeeper) 
            : base(gzipService, chunksKeeper)
        {
        }

        public override void ReadFile(Func<bool> condition, IFileReader fileReader)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (fileReader == null)
            {
                throw new ArgumentNullException(nameof(fileReader));
            }

            var index = 0;
            long offset = 0;
            using (fileReader)
            {
                var fileLength = fileReader.FileLength;
                var initialFileLengthBytes = fileReader.Read(fileLength - Int64Size, Int64Size);
                var initialFileLength = BitConverter.ToInt64(initialFileLengthBytes, 0);

                if (initialFileLength <= 0)
                {
                    throw new ProcessException();
                }

                do
                {
                    Wait();

                    var chunkLengthBytes = fileReader.Read(offset, Int32Size);
                    var chunkLength = BitConverter.ToInt32(chunkLengthBytes, 0);

                    offset += Int32Size;
                    var chunkSize = chunkLength + Int32Size;

                    var chunkBytes = fileReader.Read(offset, chunkSize);
                    var compressedBytes = chunkBytes.Take(chunkLength).ToArray();
                    var initialLengthBytes = chunkBytes.Skip(chunkLength).Take(Int32Size).ToArray();
                    var initialLength = BitConverter.ToInt32(initialLengthBytes, 0);

                    if (initialLength <= 0)
                    {
                        throw new ProcessException();
                    }

                    offset += chunkSize;

                    _chunksKeeper.AddChunk(new Chunk(index++, compressedBytes, initialLength, initialFileLength));

                } while (condition.Invoke() && offset < fileLength - Int64Size);
            }
        }

        public override void SaveTo(Func<bool> condition, IFileWriter fileWriter)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (fileWriter == null)
            {
                throw new ArgumentNullException(nameof(fileWriter));
            }

            var index = 0;
            long totalLength = 0;
            using (fileWriter)
            {
                while (condition.Invoke())
                {
                    var item = _chunksKeeper.GetProcessedChunk(index); 
                    if (item != null)
                    {
                        totalLength += item.Length;
                        fileWriter.Write(item.Buffer, item.Length);

                        if (totalLength >= item.FileLength)
                        {
                            break;
                        }

                        index++;
                    }
                }
            }
        }
    }
}
