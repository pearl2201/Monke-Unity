using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp.Server;

public class PikaServer : MonoBehaviour, IMultiplayer
{
    private WebSocketServer wssv;
    private Dictionary<uint, IPikaPeer> peers;
    private IdGeneratorUInt _idGenerator;
    [SerializeField] int port;

    public List<Action<IPikaPeer>> onConnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer>> onDisconnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer, byte[]>> onMessagesListener = new List<Action<IPikaPeer, byte[]>>();

    public Queue<(IPikaPeer, byte[])> datas = new Queue<(IPikaPeer, byte[])>();

    void Awake()
    {
        peers = new Dictionary<uint, IPikaPeer>();
        _idGenerator = new IdGeneratorUInt(1, int.MaxValue);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    public void StartServer(int port, int maxClients)
    {
        wssv = new WebSocketServer(port);
        wssv.Log.Output = (enabled, a) =>
        {

            Debug.Log(a);
        };
        wssv.AddWebSocketService<PikaServerPeer>("/laputa", (s) => s.Server = this);
        wssv.Start();
    }

    public void OnWebsocketOpen(PikaServerPeer behavior)
    {
      
            uint id = _idGenerator.GetNewId();
            peers[id] = behavior;
            behavior.Id = id;
            foreach (var item in onConnectedListener)
            {
                item(behavior);
            }
        

    }

    public void OnWebsocketClose(PikaServerPeer behavior)
    {
      
            foreach (var item in onDisconnectedListener)
            {
                item(behavior);
            }
            peers.Remove(behavior.Id);
            _idGenerator.ReuseId(behavior.Id);

    }

    public void OnWebsocketError(PikaServerPeer behaviour, Exception ex)
    {

    }
    public void OnMessage(PikaServerPeer behaviour, byte[] data)
    {
        Debug.Log("[*] Server receive: " + data.Length);

        
            NetHelper.ParseMessage(behaviour, data, OnPingMessage, OnPongMessage, (bin) =>
        {
            foreach (var listener in onMessagesListener)
            {
                listener(behaviour, bin);
            }
        });



    }

    private async void OnPingMessage(IPikaPeer peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        peer.SendMessage(NetStatic.PongMsg);
    }

    private async void OnPongMessage(IPikaPeer peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        peer.SendMessage(NetStatic.PingMsg);
    }

    private void OnDestroy()
    {
        wssv.Stop();
    }
    // Update is called once per frame

    public void SubscribeConnected(Action<IPikaPeer> cb)
    {
        onConnectedListener.Add(cb);
    }

    public void SubscribeDisconnected(Action<IPikaPeer> cb)
    {
        onDisconnectedListener.Add(cb);
    }

    public void SubscribeMessage(Action<IPikaPeer, byte[]> cb)
    {
        onMessagesListener.Add(cb);
    }




    public void SendBytes(byte[] bin, IPikaPeer peer)
    {


        var arr = NetHelper.PackSocketMsg(bin);
        Debug.Log($"Send {arr.Length} to {peer.Id}");
        peer.SendMessage(arr);
    }

    public void SendBytes(byte[] bin)
    {
        var arr = NetHelper.PackSocketMsg(bin);
        foreach (var peer in peers)
        {

            Debug.Log($"Send {arr.Length} to {peer.Value.Id}");
            peer.Value.SendMessage(arr);
        }
    }
}
