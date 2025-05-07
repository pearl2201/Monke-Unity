using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class NetworkStatistic : MonoSingleton<NetworkStatistic>
{

    public ConcurrentCircularBuffer<int> SendBytes = new ConcurrentCircularBuffer<int>(60);
    public ConcurrentCircularBuffer<int> ReceiveBytes = new ConcurrentCircularBuffer<int>(60);

    public ConcurrentCircularBuffer<int> SendPackages = new ConcurrentCircularBuffer<int>(60);

    public ConcurrentCircularBuffer<int> ReceivePackages = new ConcurrentCircularBuffer<int>(60);

    public int FrameReceiveBytes = 0;

    public int FrameSendBytes = 0;

    public int FrameSendPackages = 0;

    public int FrameReceivePackages = 0;


    public float SentBytesInSecond = 0;

    public float SentPackagesInSecond = 0;

    public float ReceivedBytesInSecond = 0;

    public float ReceivedPackagesInSecond = 0;


    void Start()
    {
        StartCoroutine(CollectNetworkStatistic());
    }

    IEnumerator CollectNetworkStatistic()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            SendBytes.Enqueue(FrameSendBytes);
            ReceiveBytes.Enqueue(FrameReceiveBytes);
            SendPackages.Enqueue(FrameSendPackages);
            ReceivePackages.Enqueue(FrameReceivePackages);
            FrameSendBytes = 0;
            FrameReceivePackages = 0;
            FrameSendPackages = 0;
            FrameReceivePackages = 0;
            SentBytesInSecond = SmoothAverage(SendBytes, SendBytes.Min());
            SentPackagesInSecond = SmoothAverage(SendPackages, SendPackages.Min());
            ReceivedBytesInSecond = SmoothAverage(ReceivePackages, ReceivePackages.Min());
            ReceivedPackagesInSecond = SmoothAverage(ReceivePackages, ReceivePackages.Min());
        }
    }

    private static float SmoothAverage(IEnumerable<int> samples, int minValue)
    {
        int sampleSize = samples.Count();
        int middleValue = samples.ElementAt(samples.Count() / 2);
        int sampleCount = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            int value = samples.ElementAt(i);

            // If the value is way too high, we discard that value because its probably just a random occurrance
            if (value > (2 * middleValue) && value > minValue)
            {
                continue;
            }
            else
            {
                sampleCount += value;
            }
        }

        return sampleCount / samples.Count();
    }

    public void OnSendBytes(int bytes)
    {
        FrameSendBytes += bytes;
        FrameSendPackages++;
    }

    public void OnReceiveBytes(int bytes)
    {
        FrameReceiveBytes += bytes;
        FrameReceivePackages++;
    }

    public void OnSendPackages()
    {

    }

    public void OnReceivePackages()
    {

    }
}

public class ConcurrentCircularBuffer<T> : IEnumerable<T>
{
    private readonly object _locker = new();
    private readonly int _capacity;
    private Node _head;
    private Node _tail;
    private int _count = 0;

    private class Node
    {
        public readonly T Item;
        public Node Next;
        public Node(T item) => Item = item;
    }

    public ConcurrentCircularBuffer(int capacity)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
    }

    public int Count => System.Threading.Volatile.Read(ref _count);

    public void Enqueue(T item)
    {
        Node node = new(item);
        lock (_locker)
        {
            if (_head is null) _head = node;
            if (_tail is not null) _tail.Next = node;
            _tail = node;
            if (_count < _capacity) _count++; else _head = _head.Next;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        Node node; int count;
        lock (_locker) { node = _head; count = _count; }
        for (int i = 0; i < count && node is not null; i++, node = node.Next)
            yield return node.Item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}