using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace MonkeNet.Client
{

    /// <summary>
    /// Syncs the clients clock with the servers one, in the process it calculates latency and other debug information.
    /// </summary>
    public partial class ClientNetworkClock : InternalClientComponent
    {
        // Called every time latency is calculated
        public delegate void LatencyCalculatedEventHandler(int latencyAverageTicks, int jitterAverageTicks);

        public event LatencyCalculatedEventHandler onLatencyCalculated;
        [SerializeField]
        bool isDebug;
        [SerializeField] private int _sampleSize = 11;
        [SerializeField] private float _sampleRateMs = 1000;
        [SerializeField] private int _minLatency = 50;
        [SerializeField] private int _fixedTickMargin = 3;

        private int _currentTick = 0;               // Client/Server Synced Tick
        private int _immediateLatencyMsec = 0;      // Latest Calculated Latency in Milliseconds
        private int _averageLatencyInTicks = 0;     // Average Latency in Ticks
        private int _jitterInTicks = 0;             // Latency Jitter in ticks
        private int _averageOffsetInTicks = 0;      // Average Client to Server clock offset in Ticks
        private int _lastOffset = 0;
        private int _minLatencyInTicks = 0;

        private readonly List<int> _offsetValues = new();
        private readonly List<int> _latencyValues = new();

        private bool offsetCalculated = false;
        public override void Start()
        {
            base.Start();
            // FindFirstObjectByType<Timer>().WaitTime = _sampleRateMs / 1000.0f;
            _minLatencyInTicks = PhysicsUtils.MsecToTick(_minLatency);
            StartCoroutine(SyncClock());

            ClientManager.Instance.onClientDisconnected += OnClientDisconnected;
        }

        private void OnClientDisconnected()
        {
            StopAllCoroutines();
        }

        IEnumerator SyncClock()
        {
            while (true)
            {
                OnTimerOut();
                if (offsetCalculated)
                {
                    yield return new WaitForSeconds(_sampleRateMs / 1000);
                }
                else
                {
                    // fast forward
                    yield return new WaitForSeconds(_sampleRateMs / 10000);
                }


            }

        }
        protected override void OnCommandReceived(Area area, int areaId, INetSerializable command)
        {
            if (command is ClockSyncMessage sync)
            {
                Debug.Log("Receive ClockSyncMessage");
                SyncReceived(sync);
            }
        }

        public void ProcessTick()
        {
            _currentTick += 1 + _lastOffset;
            _lastOffset = 0;
        }

        public int GetCurrentTick()
        {
            return _currentTick;
        }

        public int GetCurrentRemoteTick()
        {
            return _currentTick + _averageLatencyInTicks + _jitterInTicks + _fixedTickMargin;
        }

        public int GetRoomRemoteTick(int roomTick)
        {
            return roomTick + _averageLatencyInTicks + _jitterInTicks + _fixedTickMargin;
        }

        public int GetRoomTickFromServerTick(int serverTick)
        {
            return serverTick + _averageLatencyInTicks;
        }

        private static int GetLocalTimeMs()
        {
            return (int)Time.realtimeSinceStartup;
        }

        private void SyncReceived(ClockSyncMessage sync)
        {
            // Latency as the difference between when the packet was sent and when it came back divided by 2
            _immediateLatencyMsec = (GetLocalTimeMs() - sync.ClientTime) / 2;
            int immediateLatencyInTicks = PhysicsUtils.MsecToTick(_immediateLatencyMsec);

            // Time difference between our clock and the server clock accounting for latency
            int _immediateOffsetInTicks = (sync.ServerTime - _currentTick) + immediateLatencyInTicks;

            _offsetValues.Add(_immediateOffsetInTicks);
            _latencyValues.Add(immediateLatencyInTicks);

            if (_offsetValues.Count >= _sampleSize)
            {
                // Calculate average clock offset for the lasts n samples
                _offsetValues.Sort();
                _averageOffsetInTicks = SimpleAverage(_offsetValues);
                _lastOffset = _averageOffsetInTicks; // For adjusting the clock

                // Calculate average latency for the lasts n samples
                _latencyValues.Sort();
                _jitterInTicks = _latencyValues[^1] - _latencyValues[0];
                _averageLatencyInTicks = SmoothAverage(_latencyValues, _minLatencyInTicks);


                onLatencyCalculated(_averageLatencyInTicks, _averageLatencyInTicks);
                Debug.Log($"At tick {_currentTick}, latency calculations done. Avg. Latency {_averageLatencyInTicks} ticks, Jitter {_jitterInTicks} ticks, Clock Offset {_lastOffset} ticks");
                offsetCalculated = true;
                _offsetValues.Clear();
                _latencyValues.Clear();
            }
        }

        //FIXME: Can be done with samples.Average() I believe but im too lazy to check
        private static int SimpleAverage(List<int> samples)
        {
            if (samples.Count <= 0)
            {
                return 0;
            }

            int count = 0;
            samples.ForEach(s => count += s);
            return count / samples.Count;
        }

        private static int SmoothAverage(List<int> samples, int minValue)
        {
            int sampleSize = samples.Count;
            int middleValue = samples[samples.Count / 2];
            int sampleCount = 0;

            for (int i = 0; i < sampleSize; i++)
            {
                int value = samples[i];

                // If the value is way too high, we discard that value because its probably just a random occurrance
                if (value > (2 * middleValue) && value > minValue)
                {
                    samples.RemoveAt(i);
                    sampleSize--;
                }
                else
                {
                    sampleCount += value;
                }
            }

            return sampleCount / samples.Count;
        }

        //Called every _sampleRateMs
        private void OnTimerOut()
        {
            Debug.Log("Send Clock Sync Message to Server");
            var sync = new ClockSyncMessage
            {
                ClientTime = GetLocalTimeMs(),
                ServerTime = 0
            };

            SendCommandToServer(sync);
        }

        public void DisplayDebugInformation(StringBuilder builder)
        {
            if (isDebug)
            {
                builder.AppendLine($"Synced Tick {GetCurrentRemoteTick()}");
                builder.AppendLine($"Local Tick {GetCurrentTick()}");
                builder.AppendLine($"Immediate Latency {_immediateLatencyMsec}ms");
                builder.AppendLine($"Average Latency {_averageLatencyInTicks} ticks");
                builder.AppendLine($"Latency Jitter {_jitterInTicks} ticks");
                builder.AppendLine($"Average Offset {_averageOffsetInTicks} ticks");
            }
        }
    }
}