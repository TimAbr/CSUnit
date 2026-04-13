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
    private readonly Dictionary<Thread, DateTime> _activeTasks = new();
    private readonly List<Thread> _workers = new();
    private readonly object _lock = new();
    private readonly TimeSpan _hangThreshold = TimeSpan.FromSeconds(30);
    
    private int _busyThreads;
    private int _idleThreads;
    private bool _isShuttingDown;

    public event Action<Thread>? OnThreadCreated;
    public event Action<Thread>? OnThreadRemoved;
    public event Action<Thread, Action>? OnTaskStarted;
    public event Action<Thread, Action>? OnTaskCompleted;
    public event Action<Thread, TimeSpan>? OnThreadHung;

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

            CheckForHungThreads();

            _workQueue.Enqueue(task);

            if (_workers.Count < _maxSize && _idleThreads == 0)
            {
                CreateWorker(false);
            }

            Monitor.Pulse(_lock);
        }
    }

    private void CheckForHungThreads()
    {
        lock (_lock)
        {
            var now = DateTime.Now;
            List<Thread>? toReplace = null;

            foreach (var kvp in _activeTasks)
            {
                if ((now - kvp.Value) > _hangThreshold)
                {
                    toReplace ??= new List<Thread>();
                    toReplace.Add(kvp.Key);
                }
            }

            if (toReplace != null)
            {
                foreach (var hungThread in toReplace)
                {
                    OnThreadHung?.Invoke(hungThread, now - _activeTasks[hungThread]);
                    
                    _workers.Remove(hungThread);
                    _activeTasks.Remove(hungThread);
        
                    CreateWorker(isCore: _workers.Count < _coreSize);
                }
            }
        }
    }

    private void CreateWorker(bool isCore)
    {
        var thread = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = isCore ? $"Pool-Core-{Guid.NewGuid().ToString()[..4]}" : $"Pool-Extra-{Guid.NewGuid().ToString()[..4]}"
        };
        _workers.Add(thread);
        thread.Start();
        OnThreadCreated?.Invoke(thread);
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
                                OnThreadRemoved?.Invoke(Thread.CurrentThread);
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
                _activeTasks[Thread.CurrentThread] = DateTime.Now;
            }

            if (task != null)
            {
                Interlocked.Increment(ref _busyThreads);
                OnTaskStarted?.Invoke(Thread.CurrentThread, task);
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
                    OnTaskCompleted?.Invoke(Thread.CurrentThread, task);
                    Interlocked.Decrement(ref _busyThreads);
                    lock (_lock)
                    {
                        _activeTasks.Remove(Thread.CurrentThread);
                    }
                }
            }
        }
    }

    public ThreadPoolStatus GetStatus()
    {
        lock (_lock)
        {
            CheckForHungThreads();
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
