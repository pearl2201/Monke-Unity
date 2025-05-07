
using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.Client
{
    public abstract partial class InternalClientComponent : MonoBehaviour
    {
        protected virtual void OnCommandReceived(Area area, int areaId, INetSerializable command) { }
        protected virtual void OnLatencyCalculated(int latencyAverageTicks, int jitterAverageTicks) { }
        protected virtual void OnProcessTick(int currentTick, int currentRemoteTick) { }

        private bool _networkReady = false;

        public virtual void Start()
        {
            ClientManager.Instance.onClientTick += OnProcessTick;
            ClientManager.Instance.onNetworkReady += OnNetworkReady;
            ClientManager.Instance.CommandReceived += OnCommandReceived;
            ClientManager.Instance.onLatencyCalculated += OnLatencyCalculated;
        }

        protected static void SendCommandToServer(INetSerializable command)
        {
            ClientManager.Instance.SendCommandToServer(command);
        }

        protected static void SendCommandToRoom(int roomId, INetSerializable command)
        {
            ClientManager.Instance.SendCommandToRoom(roomId, command);
        }


        private void OnNetworkReady()
        {
            _networkReady = true;
        }

        protected static int NetworkId
        {
            get { return ClientManager.Instance.GetNetworkId(); }
        }

        protected bool NetworkReady
        {
            get { return _networkReady; }
        }
    }
}