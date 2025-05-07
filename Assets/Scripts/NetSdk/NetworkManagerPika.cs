using LiteNetLib.Utils;
using MonkeNet.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;

public class NetworkManagerPika : MonoBehaviour, INetworkManager
{
    public enum AudienceMode : int
    {
        Broadcast = 0,
        Server = 1
    }

    private int _networkId = 0;

    public event INetworkManager.ClientConnectedEventHandler ClientConnected;
    public event INetworkManager.ClientDisconnectedEventHandler ClientDisconnected;
    public event INetworkManager.PacketReceivedEventHandler PacketReceived;

    protected Dictionary<ulong, Type> hashToTypes;
    protected Dictionary<Type, ulong> typesToHash;

    protected IdGeneratorUShort _idGenerator;

    protected IMultiplayer _multiplayer;

    public void Awake()
    {
        _idGenerator = new IdGeneratorUShort(1, ushort.MaxValue);


        hashToTypes = new Dictionary<ulong, Type>();
        typesToHash = new Dictionary<Type, ulong>();
        RegisterNetworkMessages();
    }

    public void RegisterNetworkMessages()
    {
        Type[] registeredMessages = GetTypesImplementingInterface(typeof(INetSerializable));

        foreach (Type t in registeredMessages)
        {
            NetPacketAttribute MyAttribute = (NetPacketAttribute)Attribute.GetCustomAttribute(t, typeof(NetPacketAttribute));
            if (MyAttribute != null)
            {
                var hash = CalculateHash(t);
                hashToTypes[hash] = t;
                typesToHash[t] = hash;
            }
        }
    }

    public ulong CalculateHash(Type t)
    {
        ulong hash = 14695981039346656037UL; //offset
        string typeName = t.ToString();
        for (var i = 0; i < typeName.Length; i++)
        {
            hash ^= typeName[i];
            hash *= 1099511628211UL; //prime
        }
        return hash;
    }

    private static Type[] GetTypesImplementingInterface(Type type)
    {
        return Assembly.GetExecutingAssembly()
                       .GetTypes()
                       .Where(t => type.IsAssignableFrom(t) && !t.IsAbstract)
                       .ToArray();
    }


    public void CreateServer(int port, int maxClients = 32)
    {
        GameObject go = new GameObject("PikaServer");
        var client = go.AddComponent<PikaServer>();
        client.StartServer(port, maxClients);
        client.SubscribeConnected(OnPeerConnected);
        client.SubscribeDisconnected(OnPeerDisconnected);
        client.SubscribeMessage(OnPacketReceived);
        DontDestroyOnLoad(client);
        _networkId = _idGenerator.GetNewId();
        _multiplayer = client;
        Debug.Log($"Created server, Port:{port} Max Clients:{maxClients}");
    }


    public void CreateClient(string protocol, string address, int port)
    {
        GameObject go = new GameObject("PikaServer");
        var client = go.AddComponent<PikaClient>();
        client.StartClient(protocol, address, port);
        client.SubscribeConnected(OnPeerConnected);
        client.SubscribeDisconnected(OnPeerDisconnected);
        client.SubscribeMessage(OnPacketReceived);
        DontDestroyOnLoad(client);
        _networkId = _idGenerator.GetNewId();
        _multiplayer = client;
        Debug.Log($"Client connected to {address}:{port}");
    }

    public void SendBytes(byte[] bin, MonkeNetPeer peer)
    {
        peer.Send(bin);
    }

    public int PopStatistic(INetworkManager.NetworkStatisticEnum statistic)
    {
        // var enetHost = (_multiplayer.MultiplayerPeer as ENetMultiplayerPeer).Host;

        return statistic switch
        {
            INetworkManager.NetworkStatisticEnum.SentBytes => (int)NetworkStatistic.Instance.SentBytesInSecond,
            INetworkManager.NetworkStatisticEnum.ReceivedBytes => (int)NetworkStatistic.Instance.ReceivedBytesInSecond,
            INetworkManager.NetworkStatisticEnum.SentPackets => (int)NetworkStatistic.Instance.SentPackagesInSecond,
            INetworkManager.NetworkStatisticEnum.ReceivedPackets => (int)NetworkStatistic.Instance.ReceivedPackagesInSecond,
            _ => throw new MonkeNetException("Undefined statistic"),
        };
        return 1;
    }

    public int GetSessionId()
    {
        return _networkId;
    }

    public void SetSessionId(int networkId)
    {
        _networkId = networkId;
    }
    private void OnPeerConnected(IPikaPeer id)
    {
        Debug.Log("OnPeerConnected: " + id.Id);
        MonkeNetPeer peer = new MonkeNetPeer(id);
        ClientConnected?.Invoke(peer);
    }

    private void OnPeerDisconnected(IPikaPeer id)
    {
        ClientDisconnected?.Invoke((MonkeNetPeer)id.Tag);
        Debug.Log("OnPeerDisconnected: " + id.Id);
       
    }

    private void OnPacketReceived(IPikaPeer id, byte[] bin)
    {
        PacketReceived?.Invoke((MonkeNetPeer)id.Tag, bin);


    }

    public Type GetTypeFromHash(ulong hash)
    {
        if (hashToTypes.TryGetValue(hash, out var type))
        {
            return type;
        }
        return null;
    }

    public ulong GetHashFromType(Type t)
    {
        if (typesToHash.TryGetValue(t, out var type))
        {
            return type;
        }
        Debug.Log("Coult not find hash for type:  " + t.ToString());
        return default(ulong);
    }

    public void SendBytes(byte[] bin, List<MonkeNetPeer> peers)
    {
        foreach(var id in peers)
        {
            id.Send(bin);
        }    
    }

    public void SendBytes(byte[] bin)
    {
        _multiplayer.SendBytes(bin);
    }
}
