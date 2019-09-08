using System;
using System.Collections.Generic;
using GZIPWin;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Services;
using GZIPWinConsole.Helpers;

namespace GZIPWinConsole.Providers
{
    public class ProcessServiceProvider
    {
        private readonly Dictionary<ProcessType, Func<IProcessService>> _compressingServices;

        public ProcessServiceProvider(IGzipService gzipService, IChunksKeeper chunksKeeper)
        {
            _compressingServices = new Dictionary<ProcessType, Func<IProcessService>>
            {
                { ProcessType.Decompress, () => new DecompressService(gzipService, chunksKeeper)},
                { ProcessType.Compress, () => new CompressService(gzipService, chunksKeeper)}
            };

        }

        public IProcessService GetProcessService(ProcessType processKey)
        {
            _compressingServices.TryGetValue(processKey, out var compressingService);
            return compressingService?.Invoke();
        }
    }
}
