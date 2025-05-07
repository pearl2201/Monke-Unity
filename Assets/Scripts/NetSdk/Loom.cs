﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static Loom _current;
    static bool initialized;
    static int numThreads;
    List<Action> _actions = new List<Action>();
    List<Action> _currentActions = new List<Action>();
    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
    List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
    public static Loom Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }
    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }
    public static void QueueOnMainThread(Action action, float time)
    {
        if (!Mathf.Approximately(time, 0))
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }
    public static Thread RunAsync(Action a)
    {
        Initialize();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }
    static void Initialize()
    {
        if (!initialized)
        {
            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("Loom");
            _current = g.AddComponent<Loom>();
        }
    }
    static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }
    void Awake()
    {
        _current = this;
        initialized = true;
    }
    void OnDisable()
    {
        if (_current == this)
        {
            _current = null;
        }
    }
    void Update()
    {
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions);
            _actions.Clear();
        }
        foreach (var a in _currentActions)
        {
            a();
        }
        lock (_delayed)
        {
            _currentDelayed.Clear();
            _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
            foreach (var item in _currentDelayed)
                _delayed.Remove(item);
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();
        }
    }
    public struct DelayedQueueItem
    {
        public Action action;
        public float time;
    }
}
