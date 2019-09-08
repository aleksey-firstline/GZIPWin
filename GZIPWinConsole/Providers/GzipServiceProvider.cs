using System;
using System.Collections.Generic;
using GZIPWin.Interfaces;
using GZIPWin.Services;
using GZIPWinConsole.Helpers;

namespace GZIPWinConsole.Providers
{
    public class GzipServiceProvider
    {
        private readonly Dictionary<ProcessType, Func<IGzipService>> _processServices;

        public GzipServiceProvider()
        {
            _processServices = new Dictionary<ProcessType, Func<IGzipService>>
            {
                { ProcessType.Decompress, () => new GzipDecompressService()},
                { ProcessType.Compress, () => new GzipCompressService()}
            };

        }

        public IGzipService GetGzipService(ProcessType processKey)
        {
            _processServices.TryGetValue(processKey, out var process);
            return process?.Invoke();
        }
    }
}
