using System;
using System.Collections.Generic;

namespace MonkeNet.Shared
{

    public interface INetworkManager
    {
        public enum PacketModeEnum
        {
            Reliable, Unreliable
        }

        public enum NetworkStatisticEnum
        {
            SentBytes, ReceivedBytes, SentPackets, ReceivedPackets,
            PacketLoss, RoundTripTime
        }

        public delegate void ClientConnectedEventHandler(MonkeNetPeer id);
        public event ClientConnectedEventHandler ClientConnected;

        public delegate void ClientDisconnectedEventHandler(MonkeNetPeer id);
        public event ClientDisconnectedEventHandler ClientDisconnected;

        public delegate void PacketReceivedEventHandler(MonkeNetPeer id, byte[] bin);
        public event PacketReceivedEventHandler PacketReceived;

        public void CreateServer(int port, int maxClients = 32);
        public void CreateClient(string protocol, string address, int port);
        // for client only
        public void SendBytes(byte[] bin);
        public void SendBytes(byte[] bin, MonkeNetPeer id);

        public int GetSessionId();

        public void SetSessionId(int sessionId);

        public Type GetTypeFromHash(ulong hash);
        public ulong GetHashFromType(Type t);
        #region Statistics
        public int PopStatistic(NetworkStatisticEnum statistic);
        #endregion
    }

}