using LiteNetLib.Utils;
using MonkeNet;
using MonkeNet.NetworkMessages;
using MonkeNet.Server;
using MonkeNet.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerRoom : MonoBehaviour
{
    public int id;
    public Scene scene;
    private PhysicsScene physicsScene;

    [SerializeField] RoomEntityManager _entityManager;
    public RoomEntityManager EntityManager => _entityManager;

    public List<MonkeNetPeer> Peers = new List<MonkeNetPeer>();


    public EventHandler<MonkeNetPeer> onClientConnected;
    public EventHandler<CommandReceivedArgs> onCommandReceived;

    public EventHandler<MonkeNetPeer> onClientDisconnected;
    public EventHandler<int> onRoomTick;

    public string Name { get; private set; }
    public byte Slots { get; private set; }
    public byte MaxSlots { get; private set; }

    public int dispatchRate;

    float _cooldownDispatch = 0;

    bool _initialized = false;

    int _currentTick;

    [SerializeField] ServerInputReceiver _inputReceiver;

    private void Start()
    {
        ServerManager.Instance.onClientConnected += OnClientConnected;
        ServerManager.Instance.onClientDisconnected += OnClientDisconnected;
        ServerManager.Instance.onCommandReceived += OnCommandReceived;
    }

    private void OnDestroy()
    {
        ServerManager.Instance.onClientConnected -= OnClientConnected;
        ServerManager.Instance.onClientDisconnected -= OnClientDisconnected;
        ServerManager.Instance.onCommandReceived -= OnCommandReceived;
    }

    private void OnCommandReceived(object sender, CommandReceivedArgs e)
    {
        if (e.area == Area.Room && e.areaId == id)
        {
            onCommandReceived?.Invoke(this, e);
        }
    }

    private void OnClientDisconnected(object sender, MonkeNetPeer clientId)
    {
        if (Peers.Contains(clientId))
        {
            RemovePlayerFromRoom(clientId);
        }
    }

    private void OnClientConnected(object sender, MonkeNetPeer clientId)
    {

    }

    public void Broadcast(byte[] bin)
    {
        foreach (var peer in Peers)
        {
            peer.Send(bin);
        }

    }

    public void RoomUpdate(int currentTick)
    {
        physicsScene.Simulate(Time.fixedDeltaTime);
        _cooldownDispatch += 1;
        if (_cooldownDispatch >= dispatchRate)
        {
            _cooldownDispatch = 0;
            _entityManager.SendSnapshotData(currentTick);
        }

    }

    public void FixedUpdate()
    {

        if (!_initialized)
        {
            return;
        }
        _currentTick += 1;

        onRoomTick?.Invoke(this, _currentTick);
        EntitiesCallProcessTick(_currentTick);

        _inputReceiver.DropOutdatedInputs(_currentTick); // Delete all inputs that we don't need anymore

        // JoltPhysicsServer3D.GetSingleton().SpaceStep(MonkeNetManager.Instance.PhysicsSpace, PhysicsUtils.DeltaTime);
        // JoltPhysicsServer3D.GetSingleton().SpaceFlushQueries(MonkeNetManager.Instance.PhysicsSpace);
        RoomUpdate(_currentTick);
    }

    private void EntitiesCallProcessTick(int currentTick)
    {
        foreach (var node in _entityManager._entitySpawner.Entities)
        {
            if (node is IServerEntity serverEntity)
            {
                INetSerializable input = _inputReceiver.GetInputForEntityTick(serverEntity, currentTick);

                if (input != null)
                {
                    serverEntity.OnProcessTick(currentTick, input);
                }
            }
        }
    }

    public void Initialize(int id, string name, byte maxslots)
    {
        this.id = id;
        Name = name;
        MaxSlots = maxslots;

        CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        scene = SceneManager.CreateScene("Room_" + name, csp);
        physicsScene = scene.GetPhysicsScene();
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        _entityManager._room = this;
        _initialized = true;
    }

    public void AddPlayerToRoom(MonkeNetPeer clientConnection)
    {
        Peers.Add(clientConnection);
        onClientConnected?.Invoke(this, clientConnection);
        ServerManager.Instance.SendCommandToClient(clientConnection, MonkeNet.NetworkMessages.Area.Lobby, NetworkAreaId.Default, new LobbyJoinRoomAccepted { Id = this.id });
    }

    public void RemovePlayerFromRoom(MonkeNetPeer clientConnection)
    {

        onClientDisconnected?.Invoke(this, clientConnection);
        Peers.Remove(clientConnection);
    }

    // Note: client tu dong request element
    public void JoinPlayerToGame(MonkeNetPeer clientConnection)
    {
        //GameObject go = Instantiate(playerPrefab, transform);
        //ServerPlayer player = go.GetComponent<ServerPlayer>();
        //serverPlayers.Add(player);
        //playerStateData.Add(default);
        //player.Initialize(Vector3.zero, clientConnection);

        //playerSpawnData.Add(player.GetPlayerSpawnData());
    }

    public void Close()
    {
        foreach (var p in Peers)
        {
            RemovePlayerFromRoom(p);
        }
        Destroy(gameObject);
    }


}
