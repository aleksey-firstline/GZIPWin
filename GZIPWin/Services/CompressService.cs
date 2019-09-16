using System;
using System.Collections.Generic;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWin.Services
{
    public class CompressService : BaseProcessService
    {
        public CompressService(IGzipService gzipService, IChunksKeeper chunksKeeper)
            : base(gzipService, chunksKeeper)
        {
        }

        public override IEnumerable<Chunk> ReadFile(Func<bool> condition, IFileReader fileReader)
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
                while (condition.Invoke() && offset < fileLength)
                {
                    Wait();

                    var bytes = fileReader.Read(offset, ChunkSize);
                    offset += bytes.Length;

                    var chunk = new Chunk(index++, bytes, bytes.Length, fileLength);

                    yield return chunk;
                }
            }
        }

        public override void SaveTo(IEnumerable<Chunk> chunks, IFileWriter fileWriter)
        {
            if (fileWriter == null)
            {
                throw new ArgumentNullException(nameof(fileWriter));
            }

            long lengthTotal = 0;
            using (fileWriter)
            {
                foreach (var item in chunks)
                {
                    if (item != null)
                    {
                        var buffer = item.Buffer;
                        var totalSize = BitConverter.GetBytes(buffer.Length);
                        var size = BitConverter.GetBytes(item.Length);
                        buffer = ConcatArrays(totalSize, buffer, size);

                        fileWriter.Write(buffer, buffer.Length);
                        lengthTotal += item.Length;
                    }
                }

                WriteTotalLength(fileWriter, lengthTotal);
            }
        }

        private byte[] ConcatArrays(byte[] array1, byte[] array2, byte[] array3)
        {
            var array = new byte[array1.Length + array2.Length + array3.Length];
            array1.CopyTo(array, 0);
            array2.CopyTo(array, array1.Length);
            array3.CopyTo(array, array1.Length + array2.Length);

            return array;
        }

        private void WriteTotalLength(IFileWriter fileWriter, long lengthTotal)
        {
            var totalLength = BitConverter.GetBytes(lengthTotal);
            fileWriter.Write(totalLength, Int64Size);
        }
    }
}
