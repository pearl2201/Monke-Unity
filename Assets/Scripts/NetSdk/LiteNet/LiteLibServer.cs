using Assets.Scripts.NetSdk;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class LiteLibServer : MonoBehaviour, IMultiplayer
{
    private NetManager server;
    public List<Action<IPikaPeer>> onConnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer>> onDisconnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer, byte[]>> onMessagesListener = new List<Action<IPikaPeer, byte[]>>();

    public List<LiteServerPeer> Peers = new List<LiteServerPeer>();
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
            var litePeer = new LiteServerPeer(this, peer);
            peer.Tag = litePeer;
            foreach (var item in onConnectedListener)
            {
                item(litePeer);
            }
            Peers.Add(litePeer);
        };

        listener.NetworkReceiveEvent += (peer, data, channel, deliveryMethod) =>
        {


            NetHelper.ParseMessage((LiteServerPeer)peer.Tag, data.RawData, OnPingMessage, OnPongMessage, (bin) =>
            {
                foreach (var listener in onMessagesListener)
                {
                    listener((LiteServerPeer)peer.Tag, bin);
                }
            });


        };
        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            foreach (var item in onDisconnectedListener)
            {
                item((LiteServerPeer)peer.Tag);
            }
            Peers.Remove((LiteServerPeer)peer.Tag);


        };

    }


    private async void OnPingMessage(IPikaPeer peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        ((LiteServerPeer)peer.Tag).Peer.Send(NetStatic.PongMsg,DeliveryMethod.ReliableOrdered);
    }

    private async void OnPongMessage(IPikaPeer peer)
    {
        await Task.Delay(TimeSpan.FromSeconds(NetStatic.TimePingPong));
        ((LiteServerPeer)peer.Tag).Peer.Send(NetStatic.PingMsg, DeliveryMethod.ReliableOrdered);
    }

    public void StartServer(int port, int maxClients)
    {
        server.Start(port /* port */);
    }

    private void Update()
    {
        server.PollEvents();
    }

    private void OnApplicationQuit()
    {
        server.Stop();
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
        foreach (var peer in Peers)
        {

            Debug.Log($"Send {arr.Length} to {peer.Id}");
            peer.SendMessage(arr);
        }
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


}

