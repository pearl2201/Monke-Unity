using LiteNetLib.Utils;
using MonkeExample;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace MonkeNet.Client
{

    /// <summary>
    /// Main Client-side node, communicates with the server and other components of the client
    /// </summary>
    public class ClientManager : MonoBehaviour
    {
        public delegate void ClientTickEventHandler(int currentTick, int currentRemoteTick);
        public delegate void LatencyCalculatedEventHandler(int latencyAverageTicks, int jitterAverageTicks);
        public delegate void NetworkReadyEventHandler();

        public event ClientTickEventHandler onClientTick;
        public event LatencyCalculatedEventHandler onLatencyCalculated;
        public event NetworkReadyEventHandler onNetworkReady;

        public delegate void ClientConnectEventHandler();

        public event ClientConnectEventHandler onClientConnected;
        public event ClientConnectEventHandler onClientDisconnected;

        public delegate void CommandReceivedEventHandler(Area area, int areaId, INetSerializable command); // Using a C# signal here because the Godot signal wouldn't accept NetworkMessages.INetSerializable
        public event CommandReceivedEventHandler CommandReceived;

        public static ClientManager Instance { get; private set; }

        private INetworkManager _networkManager;

        public INetworkManager NetworkManager => _networkManager;
        [SerializeField] TextMeshProUGUI _debugTextArea;
        [SerializeField] bool _debugNetworking;
        [SerializeField] ClientNetworkClock _clock;
        [SerializeField] NetworkDebug _networkDebug;
        public PhysicsScene physicsScene;
        private bool _networkReady = false;
        public bool Connected { get; private set; }

        
        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            physicsScene = gameObject.scene.GetPhysicsScene();


            CommandReceived += OnCommandReceived;
        }

        private void OnCommandReceived(Area area, int areaId, INetSerializable command)
        {
            if (command is AcceptConnection ac)
            {
                _networkManager.SetSessionId(ac.SessionId);
            }
        }

        public void Update()
        {
            DisplayDebugInformation();
        }

        // TODO: I don't know if manually stepping physics inside _PhysicsProcess is a good idea,
        // as internally _PhysicsProcess will call _step() and _flush_queries() the same way I'm doing right now...
        // causing multiple calls to the same PhysicsServer methods
        void FixedUpdate()
        {

            // Advance Clock
            _clock.ProcessTick();
            int currentTick = _clock.GetCurrentTick();
            int currentRemoteTick = _clock.GetCurrentRemoteTick();
            if (CacheRuntime.Instance.GameStart)
            {
                onClientTick(currentTick, currentRemoteTick);
                //physicsScene.Simulate(Time.fixedDeltaTime);
            }

        }



        public void Initialize(INetworkManager networkManager, string protocol, string address, int port)
        {
            _networkManager = networkManager;
            _networkDebug.NetworkManager = _networkManager;

            _clock.onLatencyCalculated += OnLatencyCalculated;

            _networkManager.PacketReceived += OnPacketReceived;
            _networkManager.ClientConnected += OnClientConnected;
            _networkManager.ClientDisconnected += OnClientDisconnected;
            _networkManager.CreateClient(protocol, address, port);

            Debug.Log("Client Manager Initialized");
        }

        private void OnClientConnected(MonkeNetPeer id)
        {
            Connected = true;
            onClientConnected?.Invoke();
        }

        private void OnClientDisconnected(MonkeNetPeer id)
        {
            Connected = false;
            onClientDisconnected.Invoke();
        }

        public void SendCommandToServer(INetSerializable command)
        {
            SendCommandToServer(Area.None, NetworkAreaId.Default, command);
        }


        public void SendCommandToServer(Area area, int areaId, INetSerializable command)
        {

            _networkManager.SendBytes(NetHelper.PackMessage(_networkManager, area, areaId, command));
        }

        public void SendCommandToRoom(int roomId, INetSerializable command)
        {
            SendCommandToServer(Area.Room, roomId, command);
        }

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
                Debug.Log("OnReceiveCommand: " + command.GetType().Name);
                CommandReceived?.Invoke(area, areaId, command);
            }
            else
            {
                Debug.Log("Cannot get type from hash: " + pid);
            }

        }

        //public void MakeEntityRequest(byte entityType) //TODO: This should NOT be here
        //{
        //    _entityManager.MakeEntityRequest(entityType);
        //}

        public int GetNetworkId()
        {
            return _networkManager.GetSessionId();
        }

        private void OnLatencyCalculated(int latencyAverageTicks, int jitterAverageTicks)
        {

            onLatencyCalculated(latencyAverageTicks, jitterAverageTicks);
            //TODO: calculate this in other way, this should only be emmited once and
            //right now it will be emitted every time the colck calculates latency
            onNetworkReady();
            _networkReady = true;
            Debug.Log("On network ready");
        }

        private void DisplayDebugInformation()
        {
            if (_debugNetworking)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Network ID {_networkManager.GetSessionId()}");
                builder.AppendLine($"Framerate {Time.deltaTime}fps");
                builder.AppendLine($"Physics Tick {Time.fixedDeltaTime}hz");
                _clock.DisplayDebugInformation(builder);
                _networkDebug.DisplayDebugInformation(builder);
              

                _debugTextArea.text = builder.ToString();
            }
        }
    }

}