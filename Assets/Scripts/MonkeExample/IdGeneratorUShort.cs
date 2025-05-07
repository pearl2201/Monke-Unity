using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IdGeneratorUShort
{
    private readonly ushort _maxValue;

    private readonly Queue<ushort> _queue = new Queue<ushort>();

    private ushort _counter;

    public int AvailableIds => _maxValue - _counter + _queue.Count;

    public ushort GetNewId()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        if (_counter == _maxValue)
        {
            throw new Exception("IdGenerator overflow");
        }

        return _counter++;
    }

    public void ReuseId(ushort id)
    {
        _queue.Enqueue(id);
    }

    public IdGeneratorUShort(ushort initialValue, ushort maxValue)
    {
        _counter = initialValue;
        _maxValue = maxValue;
    }

    public void Reset()
    {
        _queue.Clear();
        _counter = 0;
    }
}



public class IdGeneratorUInt
{
    private readonly uint _maxValue;

    private readonly Queue<uint> _queue = new Queue<uint>();

    private uint _counter;

    public uint AvailableIds => (uint)(_maxValue - _counter + _queue.Count);

    public uint GetNewId()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        if (_counter == _maxValue)
        {
            throw new Exception("IdGenerator overflow");
        }

        return _counter++;
    }

    public void ReuseId(uint id)
    {
        _queue.Enqueue(id);
    }

    public IdGeneratorUInt(uint initialValue, uint maxValue)
    {
        _counter = initialValue;
        _maxValue = maxValue;
    }

    public void Reset()
    {
        _queue.Clear();
        _counter = 0;
    }
}
