using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using GZIPWin.Interfaces;
using GZIPWinConsole.Helpers;
using GZIPWin.Exceptions;
using GZIPWin.Helpers;
using GZIPWinConsole.Models;
using GZIPWinConsole.Providers;

namespace GZIPWinConsole
{
    class Program
    {
        private static ChunkThreadsHandler _threadHandler;
        private static IProcessService _processService;

        static void Main(string[] args)
        {
            var processModel = CreateModel(args);
            var results = ValidateModel(processModel);

            if (results.Any())
            {
                foreach (var error in results)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return;
            }

            Init(processModel.ProcessType);

            _threadHandler.StartTrackingChunks(HandelException);

            _threadHandler.ExecuteInThread(condition =>
            {
                var reader = new FileReader(processModel.SourceFile);
                _processService.ReadFile(condition, reader);

            }, HandelException);


            _threadHandler.ExecuteInThread(condition =>
            {
                var fileWriter = new FileWriter(processModel.ResultFile);
                _processService.SaveTo(condition, fileWriter);
                _threadHandler.StopTraceChunks();
            }, HandelException);

            _threadHandler.WaitThreads();
            Environment.Exit(Environment.ExitCode);
        }

        private static void HandelException(Exception exception)
        {
            switch (exception)
            {
                case OutOfMemoryException _:
                    Console.WriteLine("Memory is not enough ");
                    break;
                case ProcessException _:
                    Console.WriteLine("File processing error");
                    break;
                default:
                    Console.WriteLine("Exception");
                    break;
            }

            _threadHandler.StopTraceChunks();
        }

        private static void Init(ProcessType processKey)
        {
            var gzipServiceFactory = new GzipServiceProvider();
            var gzipService = gzipServiceFactory.GetGzipService(processKey);
            var chunksKeeper = new ChunksKeeper();
            var compressionFactory = new ProcessServiceProvider(gzipService, chunksKeeper);

            _processService = compressionFactory.GetProcessService(processKey);
            _threadHandler = new ChunkThreadsHandler(_processService, chunksKeeper);
        }

        private static ProcessModel CreateModel(string[] args)
        {
            var processType = GetElement(args, 0);
            var sourceFile = GetElement(args, 1);
            var resultFile = GetElement(args, 2);

            Enum.TryParse(processType, true, out ProcessType result);

            return new ProcessModel
            {
                ProcessType = result,
                SourceFile = !string.IsNullOrWhiteSpace(sourceFile) ? new FileInfo(sourceFile) : null,
                ResultFile = !string.IsNullOrWhiteSpace(resultFile) ? new FileInfo(resultFile) : null
            };
        }

        private static string GetElement(IEnumerable<string> args, int index)
        {
            return args.ElementAtOrDefault(index) ?? string.Empty;
        }

        private static List<ValidationResult> ValidateModel(ProcessModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, true);

            return results;
        }
    }
}
