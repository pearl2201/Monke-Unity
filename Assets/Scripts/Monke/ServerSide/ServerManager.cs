using LiteNetLib;
using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace MonkeNet.Server
{

    public class CommandReceivedArgs
    {
        public MonkeNetPeer clientId;
        public Area area;
        public int areaId;
        public INetSerializable command;
    }

    public partial class ServerManager : MonoSingleton<ServerManager>
    {
        public EventHandler<int> onServerTick;

        public EventHandler<int> onServerNetworkTick;

        public EventHandler<MonkeNetPeer> onClientConnected;

        public EventHandler<MonkeNetPeer> onClientDisconnected;

        public EventHandler<CommandReceivedArgs> onCommandReceived;



        private INetworkManager _networkManager;
        [SerializeField] ServerNetworkClock _serverClock;
        [SerializeField] Dictionary<NetPeer, ServerRoom> peers2Room = new Dictionary<NetPeer, ServerRoom>();

        private int _currentTick = 0;

        private bool _initialized;
        private List<ServerRoom> _rooms = new List<ServerRoom>();


        public void Start()
        {

        }

        void Update()
        {
            DisplayDebugInformation();
        }

        // TODO: I don't know if manually stepping physics inside _PhysicsProcess is a good idea,
        // as internally _PhysicsProcess will call _step() and _flush_queries() the same way I'm doing right now...
        // causing multiple calls to the same PhysicsServer methods
        void FixedUpdate()
        {
            if (!_initialized)
            {
                return;
            }
            _currentTick = _serverClock.ProcessTick();

            onServerTick.Invoke(this, _currentTick);

        }



        private void OnTimerTimeout()
        {
            // Debug.Log($"Server Status: Tick {_currentTick}, Framerate {Engine.GetFramesPerSecond()}, Physics Tick {Engine.PhysicsTicksPerSecond}hz");
        }


        public void Initialize(INetworkManager networkManager, int port)
        {
            _networkManager = networkManager;

            _networkManager.CreateServer(port);
            _networkManager.ClientConnected += OnClientConnected;
            _networkManager.ClientDisconnected += OnClientDisconnected;
            _networkManager.PacketReceived += OnPacketReceived;
            _initialized = true;

            _serverClock.onNetworkProcessTick += OnNetworkProcess;
            Debug.Log("Initialized Server Manager");
        }

        private void OnNetworkProcess(object sender, double delta)
        {
            onServerNetworkTick.Invoke(this, _currentTick);
        }

        public void SendCommandToClient(MonkeNetPeer clientId, INetSerializable command)
        {
            SendCommandToClient(clientId, Area.None, NetworkAreaId.Default, command);
        }

        public void SendCommandToClient(MonkeNetPeer clientId, Area area, int areaId, INetSerializable command)
        {
            _networkManager.SendBytes(NetHelper.PackMessage(_networkManager, area, areaId, command), clientId);
        }

        public void SendCommandToRoom(ServerRoom room, INetSerializable command)
        {
            room.Broadcast(NetHelper.PackMessage(_networkManager, Area.Room, room.id, command));
        }


        public int GetNetworkId()
        {
            return _networkManager.GetSessionId();
        }

        // Route received Input package to the correspondant Network ID
        private void OnPacketReceived(MonkeNetPeer id, byte[] bin)
        {
            Debug.Log("OnPackageReceived: " + bin.Length);
            NetDataReader reader = new NetDataReader(bin);
            var pid = reader.GetULong();
            var area = (Area)reader.GetByte();
            var areaId = reader.GetInt();

            var type = _networkManager.GetTypeFromHash(pid);
            if (type != null)
            {
                var command = (INetSerializable)Activator.CreateInstance(type);
                command.Deserialize(reader);
                Debug.Log($"OnReceiveCommand from {id.SessionId}: " + command.GetType().Name);
                onCommandReceived?.Invoke(this, new CommandReceivedArgs
                {
                    clientId = id,
                    area = area,
                    areaId = areaId,
                    command = command
                });
            }
            else
            {
                Debug.Log("Cannot get type from hash: " + pid);
            }
        }

        private void OnClientConnected(MonkeNetPeer clientId)
        {
            SendCommandToClient(clientId, new AcceptConnection
            {
                SessionId = clientId.SessionId,
            });
            onClientConnected?.Invoke(this, clientId);
            Debug.Log($"Client {clientId} connected");

        }

        private void OnClientDisconnected(MonkeNetPeer clientId)
        {
            onClientDisconnected.Invoke(this, clientId);
            Debug.Log($"Client {clientId} disconnected");
        }

        private void DisplayDebugInformation()
        {
            // ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero);
            // if (ImGui.Begin("Server Information",
            //     ImGuiWindowFlags.NoMove
            //         | ImGuiWindowFlags.NoResize
            //         | ImGuiWindowFlags.AlwaysAutoResize))
            // {
            //     builder.AppendLine($"Framerate {Engine.GetFramesPerSecond()}fps");
            //     builder.AppendLine($"Physics Tick {Engine.PhysicsTicksPerSecond}hz");
            //     _serverClock.DisplayDebugInformation();
            //     _inputReceiver.DisplayDebugInformation();
            //     ImGui.End();
            // }

        }
    }

}

public static class NetHelper
{

    public static byte[] PackMessage(INetworkManager networkManager, Area area, int areaId, INetSerializable command)
    {
        NetDataWriter writer = new NetDataWriter();
        var hash = networkManager.GetHashFromType(command.GetType());
        writer.Put(hash);
        writer.Put((byte)area);
        writer.Put(areaId);
        command.Serialize(writer);
        Debug.Log($"Send command with hash {hash}: " + command.GetType().ToString() + ", " + writer.Length);
        return writer.Data;
    }

    public static void ParseMessage(IPikaPeer peer, byte[] data, Action<IPikaPeer> onPingMsg, Action<IPikaPeer> onPongMsg, Action<byte[]> cb)
    {
        NetworkStatistic.Instance.OnReceiveBytes(data.Length);
        var bytes = data;
        if (bytes.Length == 1 && bytes[0] == 0)
        {
            onPingMsg(peer);
        }
        else if (bytes.Length == 1 && bytes[0] == 1)
        {
            onPongMsg(peer);
        }
        else if (bytes.Length >= 1)
        {
            var arr = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, arr, 0, bytes.Length - 1);
            cb(arr);

        }
    }

    public static byte[] PackSocketMsg(byte[] bin)
    {
        var arr = new byte[1 + bin.Length];
        arr[0] = 2;
        Array.Copy(bin, 0, arr, 1, bin.Length);
        return arr;
    }
}