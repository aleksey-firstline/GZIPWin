using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GZIPWin.Interfaces;
using GZIPWin.Interfaces.Helpers;
using GZIPWin.Models;

namespace GZIPWinConsole.Helpers
{
    public class ChunkThreadsHandler
    {
        private int _chunkIndex;
        private readonly int _maxThreadInPool;

        private readonly ConcurrentQueue<Action> _readyTasks;
        private readonly IProcessService _processService;
        private readonly IChunksKeeper _chunksKeeper;
        private readonly List<Thread> _threads;

        private static readonly AutoResetEvent AutoResetEventReader = new AutoResetEvent(true);
        private static readonly AutoResetEvent AutoResetEventWriter = new AutoResetEvent(true);

        private static volatile bool _shouldWrite = true;
        private static volatile Func<bool> _shouldRead = () => true;

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
                    while (_shouldRead.Invoke() || _chunksKeeper.ChunksLength > 0)
                    {
                        if (_chunksKeeper.ChunksLength == 0)
                        {
                            AutoResetEventReader.WaitOne();
                        }

                        var chunk = _chunksKeeper.GetChunk();
                        if (chunk != null)
                        {
                            _readyTasks.Enqueue(() =>
                            {
                                var processedChunk = _processService.ProcessChunk(chunk);

                                _chunksKeeper.AddProcessedChunk(processedChunk);

                                if (_chunkIndex == chunk.Index)
                                {
                                    AutoResetEventWriter.Set();
                                    AutoResetEventWriter.Reset();
                                }
                            });
                        }
                    }
                }
                catch (Exception exception)
                {
                    handler?.Invoke(exception);
                }
            })
            { IsBackground = true };

            thread.Start();
            _threads.Add(thread);
            SetupTaskThreads(handler);
        }

        private void SetupTaskThreads(Action<Exception> handler)
        {
            int completedThreadCount = 0;
            for (var i = 0; i < _maxThreadInPool; i++)
            {
                var thread = new Thread(start: () =>
                {
                    try
                    {
                        while (_shouldRead.Invoke() || !_readyTasks.IsEmpty)
                        {
                            _readyTasks.TryDequeue(out var action);
                            action?.Invoke();
                        }

                        Interlocked.Increment(ref completedThreadCount);
                        if (completedThreadCount == _maxThreadInPool)
                        {
                            _shouldWrite = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        handler?.Invoke(exception);
                    }
                    finally
                    {
                        AutoResetEventWriter.Set();
                    }
                })
                { IsBackground = true };

                thread.Start();
                _threads.Add(thread);
            }
        }

        public void ExecuteInThreadReadChunks(Func<Func<bool>, IEnumerable<Chunk>> action, Action<Exception> handler)
        {
            var thread = new Thread(start: () =>
            {
                try
                {
                    var chunks = action.Invoke(_shouldRead);
                    foreach (var chunk in chunks)
                    {
                        _chunksKeeper.AddChunk(chunk);
                        AutoResetEventReader.Set();
                        AutoResetEventReader.Reset();
                    }

                    _shouldRead = () => false;
                }
                catch (Exception exception)
                {
                    handler?.Invoke(exception);
                }
                finally
                {
                    
                    AutoResetEventReader.Set();
                }
            })
            { IsBackground = true };

            thread.Start();
            _threads.Add(thread);
        }

        private IEnumerable<Chunk> ProcessedChunks()
        {
            while (_shouldWrite || _chunksKeeper.ProcessedChunksLength > 0)
            {
                if (!_chunksKeeper.ContainsProcessedChunk(_chunkIndex))
                {
                    AutoResetEventWriter.WaitOne();
                }

                var chunk = _chunksKeeper.GetProcessedChunk(_chunkIndex);
                if (chunk != null)
                {
                    _chunkIndex++;
                }

                yield return chunk;
            }
        }

        public void ExecuteInThreadSaveChunks(Action<IEnumerable<Chunk>> action, Action<Exception> handler)
        {
            var thread = new Thread(start: () =>
            {
                try
                {
                    var chunks = ProcessedChunks();
                    action?.Invoke(chunks);
                }
                catch (Exception exception)
                {
                    handler?.Invoke(exception);
                }
            })
            { IsBackground = true };

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
            _shouldWrite = false;
            _shouldRead = () => false;
        }
    }
}
