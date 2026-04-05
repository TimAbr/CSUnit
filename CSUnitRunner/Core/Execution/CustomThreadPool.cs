using System;
using System.Collections.Generic;
using System.Threading;

namespace CSUnitRunner.Core.Execution;

public class CustomThreadPool : IDisposable
{
    private readonly int _coreSize;
    private readonly int _maxSize;
    private readonly TimeSpan _keepAliveTime;
    private readonly Queue<Action> _workQueue = new();
    private readonly List<Thread> _workers = new();
    private readonly object _lock = new();
    
    private int _busyThreads;
    private int _idleThreads;
    private bool _isShuttingDown;

    public CustomThreadPool(int coreSize, int maxSize, TimeSpan keepAliveTime)
    {
        _coreSize = Math.Max(1, coreSize);
        _maxSize = Math.Max(_coreSize, maxSize);
        _keepAliveTime = keepAliveTime;

        lock (_lock)
        {
            for (int i = 0; i < _coreSize; i++)
            {
                CreateWorker(true);
            }
        }
    }

    public void Enqueue(Action task)
    {
        lock (_lock)
        {
            if (_isShuttingDown) return;

            _workQueue.Enqueue(task);

            // Scale if: we haven't reached max, and either no one is idle or the queue is growing.
            if (_workers.Count < _maxSize && _idleThreads == 0)
            {
                CreateWorker(false);
            }

            Monitor.Pulse(_lock);
        }
    }

    private void CreateWorker(bool isCore)
    {
        var thread = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = isCore ? $"Pool-Core-{_workers.Count}" : $"Pool-Extra-{_workers.Count}"
        };
        _workers.Add(thread);
        thread.Start();
    }

    private void WorkerLoop()
    {
        while (true)
        {
            Action? task = null;

            lock (_lock)
            {
                while (_workQueue.Count == 0)
                {
                    if (_isShuttingDown) return;

                    _idleThreads++;
                    try
                    {
                        if (_workers.Count > _coreSize)
                        {
                            if (!Monitor.Wait(_lock, _keepAliveTime))
                            {
                                _workers.Remove(Thread.CurrentThread);
                                return;
                            }
                        }
                        else
                        {
                            Monitor.Wait(_lock);
                        }
                    }
                    finally
                    {
                        _idleThreads--;
                    }
                }

                if (_isShuttingDown && _workQueue.Count == 0) return;
                
                task = _workQueue.Dequeue();
            }

            if (task != null)
            {
                Interlocked.Increment(ref _busyThreads);
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pool Error]: {ex.Message}");
                }
                finally
                {
                    Interlocked.Decrement(ref _busyThreads);
                }
            }
        }
    }

    public ThreadPoolStatus GetStatus()
    {
        lock (_lock)
        {
            return new ThreadPoolStatus
            {
                TotalThreads = _workers.Count,
                BusyThreads = _busyThreads,
                QueueSize = _workQueue.Count,
                CoreSize = _coreSize,
                MaxSize = _maxSize
            };
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _isShuttingDown = true;
            Monitor.PulseAll(_lock);
        }
    }
}

public struct ThreadPoolStatus
{
    public int TotalThreads;
    public int BusyThreads;
    public int QueueSize;
    public int CoreSize;
    public int MaxSize;
}
