using LiteNetLib.Utils;
using UnityEngine;



namespace MonkeNet.Client
{
    public abstract partial class InternalRoomClientComponent : MonoBehaviour
    {
        protected virtual void OnRoomCommandReceived(object sender, INetSerializable command) { }
        protected virtual void OnServerLatencyCalculated(int latencyAverageTicks, int jitterAverageTicks) { }
        protected virtual void OnRoomProcessTick(int currentTick, int currentRemoteTick) { }

        private bool _networkReady = false;

        private ClientRoom _room;

        public ClientRoom Room { get { return _room; } }

        public void Setup(ClientRoom room)
        {
            _room = room;
        }

        public virtual void Start()
        {
            _room.onClientTick += OnRoomProcessTick;
            _room.onCommandReceived += OnRoomCommandReceived;
            ClientManager.Instance.onLatencyCalculated += OnServerLatencyCalculated;
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