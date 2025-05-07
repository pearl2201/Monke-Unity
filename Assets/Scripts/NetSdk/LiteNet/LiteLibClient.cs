using Assets.Scripts.NetSdk;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class LiteLibClient : MonoBehaviour, IMultiplayer
{
    private NetManager server;
    public List<Action<IPikaPeer>> onConnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer>> onDisconnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer, byte[]>> onMessagesListener = new List<Action<IPikaPeer, byte[]>>();

    public NetPeer peer;
    private LiteClientPeer serverPeer;
    private void Awake()
    {
        EventBasedNetListener listener = new EventBasedNetListener();
        server = new NetManager(listener);
     

        listener.ConnectionRequestEvent += request =>
        {
            //if (server.ConnectedPeersCount < 10 /* max connections */)
            //    request.AcceptIfKey("SomeConnectionKey");
            //else
            //    request.Reject();
            request.Accept();
        };



        listener.PeerConnectedEvent += peer =>
        {
           
            Console.WriteLine("We got connection: {0}", peer);  // Show peer ip

            this.peer = peer;
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("OnWebsocket open");
                serverPeer = new LiteClientPeer(this, peer);
                onConnectedListener.ForEach(x => x(serverPeer));
            });
        };

        listener.NetworkReceiveEvent += OnNetworkReceive;
        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("OnWebsocket OnClose");
                onDisconnectedListener.ForEach(x => x(serverPeer));
                serverPeer = null;
            });

            this.peer = null;
        };

    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        Loom.QueueOnMainThread(() =>
        {
            NetHelper.ParseMessage(serverPeer, reader.RawData, OnPingMessage, OnPongMessage, (bin) =>
            {
                foreach (var listener in onMessagesListener)
                {
                    listener(serverPeer, bin);
                }
            });



        });
    }

    private async void OnPingMessage(IPikaPeer _peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        peer.Send(NetStatic.PongMsg, DeliveryMethod.ReliableOrdered);
    }

    private async void OnPongMessage(IPikaPeer _peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        peer.Send(NetStatic.PingMsg, DeliveryMethod.ReliableOrdered);
    }

    public void StartClient(string protocol, string addr, int port)
    {
        server.Start( /* port */);
        server.Connect(addr, port, "");
    }

    private void Update()
    {
        server.PollEvents();
    }

    private void OnApplicationQuit()
    {
        server.Stop();
    }

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

    public void SendBytes(byte[] bin, int id)
    {
        if (this.peer != null)
        {
            Debug.Log($"Send {bin.Length} to {id}");
            var arr = new byte[1 + bin.Length];
            arr[0] = 2;
            Array.Copy(bin, 0, arr, 1, bin.Length);

            this.peer.Send(arr, DeliveryMethod.ReliableOrdered);
        }


    }

    public void SendBytes(byte[] bin, IPikaPeer id)
    {
     
    }

    public void SendBytes(byte[] bin)
    {
   
    }
}


