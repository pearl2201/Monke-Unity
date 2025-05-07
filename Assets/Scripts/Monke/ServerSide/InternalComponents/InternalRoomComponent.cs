using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.Server
{

    public abstract partial class InternalRoomComponent : MonoBehaviour
    {
        protected ServerRoom _room;

        protected virtual void OnRoomCommandReceived(object sender, CommandReceivedArgs commandReceivedArgs) { }
        protected virtual void OnRoomProcessTick(object sender, int currentTick) { }
        protected virtual void OnRoomNetworkProcessTick(object sender, int currentTick) { }
        protected virtual void OnClientConnected(object sender, MonkeNetPeer peerId) { }
        protected virtual void OnClientDisconnected(object sender, MonkeNetPeer peerId) { }

        public virtual void Setup(ServerRoom room)
        {
            _room = room;
            room.onRoomTick += OnRoomProcessTick;
            room.onRoomNetworkTick += OnRoomNetworkProcessTick;
            room.onCommandReceived += OnRoomCommandReceived;
            room.onClientConnected += OnClientConnected;
            room.onClientDisconnected += OnClientDisconnected;
        }

        protected static void SendCommandToClient(MonkeNetPeer peerId, INetSerializable command)
        {
            ServerManager.Instance.SendCommandToClient(peerId, command);
        }

        protected static void SendCommandToClient(MonkeNetPeer peerId, Area area, int areaId, INetSerializable command)
        {
            ServerManager.Instance.SendCommandToClient(peerId, area, areaId, command);
        }


        protected static void SendCommandToRoom(ServerRoom room, INetSerializable command)
        {
            ServerManager.Instance.SendCommandToRoom(room, command);
        }

        protected int NetworkId
        {
            get { return ServerManager.Instance.GetNetworkId(); }
        }
    }
}
