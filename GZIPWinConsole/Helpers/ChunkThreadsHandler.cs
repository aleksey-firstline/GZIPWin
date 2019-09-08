using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GZIPWin;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;

namespace GZIPWinConsole.Helpers
{
    public class ChunkThreadsHandler
    {
        private readonly int _maxThreadInPool;
        private readonly ConcurrentQueue<Action> _readyTasks;
        private readonly IProcessService _processService;
        private readonly IChunksKeeper _chunksKeeper;
        private readonly List<Thread> _threads;

        
        private static volatile Func<bool> _shouldStop = () => true;

        public ChunkThreadsHandler(IProcessService processService, IChunksKeeper chunksKeeper)
        {
            _maxThreadInPool = Environment.ProcessorCount;
            _readyTasks = new ConcurrentQueue<Action>();
            _threads = new List<Thread>();
            _processService = processService;
            _chunksKeeper = chunksKeeper;
        }

        public void StartTrackingChunks(Action<Exception> handler)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    while (_shouldStop.Invoke())
                    {
                        var chunk = _chunksKeeper.GetChunk();
                        if (chunk != null)
                        {
                            _readyTasks.Enqueue(() =>
                            {
                                var processedChunk = _processService.ProcessChunk(chunk);
                                _chunksKeeper.AddProcessedChunk(processedChunk);
                            });
                        }
                    }
                }
                catch (Exception exception)
                {
                    handler?.Invoke(exception);
                }
            }) {IsBackground = true};

            thread.Start();
            _threads.Add(thread);
            SetupTaskThreads(handler);
        }

        private void SetupTaskThreads(Action<Exception> handler)
        {
            for (var i = 0; i < _maxThreadInPool; i++)
            {
                var thread = new Thread(start: () =>
                {
                    try
                    {
                        while (_shouldStop.Invoke())
                        {
                            _readyTasks.TryDequeue(out var action);
                            action?.Invoke();
                        }
                    }
                    catch (Exception exception)
                    {
                        handler?.Invoke(exception);
                    }
                }) {IsBackground = true};

                thread.Start();
                _threads.Add(thread);
            }
        }

        public void ExecuteInThread(Action<Func<bool>> action, Action<Exception> handler)
        {
            var thread = new Thread(start: () =>
            {
                try
                {
                    action?.Invoke(_shouldStop);
                }
                catch (Exception exception)
                {
                    handler?.Invoke(exception);
                }
            }) {IsBackground = true};

            thread.Start();
            _threads.Add(thread);
        }

        public void WaitThreads()
        {
            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        public void StopTraceChunks()
        {
            _shouldStop = () => false;
        }
    }
}
