using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.Server
{


    public abstract partial class InternalServerComponent : MonoBehaviour
    {

        protected virtual void OnServerCommandReceived(object sender, CommandReceivedArgs commandReceivedArgs) { }
        protected virtual void OnServerProcessTick(object sender, int currentTick) { }
        protected virtual void OnServerNetworkProcessTick(object sender, int currentTick) { }
        protected virtual void OnServerClientConnected(object sender, MonkeNetPeer peerId) { }
        protected virtual void OnServerClientDisconnected(object sender, MonkeNetPeer peerId) { }

        public virtual void Start()
        {
            ServerManager.Instance.onServerTick += OnServerProcessTick;
            ServerManager.Instance.onServerNetworkTick += OnServerNetworkProcessTick;
            ServerManager.Instance.onCommandReceived += OnServerCommandReceived;
            ServerManager.Instance.onClientConnected += OnServerClientConnected;
            ServerManager.Instance.onClientDisconnected += OnServerClientDisconnected;
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