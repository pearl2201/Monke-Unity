
using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class PikaClient : MonoBehaviour, IMultiplayer
{
   

    WebSocket websocket;
    public string addr;

    public List<Action<IPikaPeer>> onConnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer>> onDisconnectedListener = new List<Action<IPikaPeer>>();

    public List<Action<IPikaPeer, byte[]>> onMessagesListener = new List<Action<IPikaPeer, byte[]>>();

    public static Queue<byte[]> datas;

    public PikaClientPeer serverPeer;

    // Start is called before the first frame update
    void Start()
    {
        datas = new Queue<byte[]>();

    }


    public void StartClient(string protocol, string addr, int port)
    {
        websocket = new WebSocket($"{protocol}://{addr}:{port}/laputa");

        websocket.OnOpen += (sender, e) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("OnWebsocket open");
                serverPeer = new PikaClientPeer(this);
                onConnectedListener.ForEach(x => x(serverPeer));
            });

        };

        websocket.OnError += (e, err) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("OnWebsocket OnError");
            });
        };

        websocket.OnClose += (e, reason) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("OnWebsocket OnClose");
                onDisconnectedListener.ForEach(x => x(serverPeer));
                serverPeer = null;
            });

        };

        websocket.OnMessage += (sender, e) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                NetHelper.ParseMessage(serverPeer, e.RawData, OnPingMessage, OnPongMessage, (bin) =>
                {
                    foreach (var listener in onMessagesListener)
                    {
                        listener(serverPeer, bin);
                    }
                });

           
               
            });

        };

        // waiting for messages
        websocket.Connect();
    }
    private void OnPingMessage(IPikaPeer peer)
    {
        Invoke("SendPongMessage", NetStatic.TimePingPong);
    }

    private void SendPongMessage()
    {
        websocket.Send(NetStatic.PongMsg);
    }
    private void OnPongMessage(IPikaPeer peer)
    {
        // send ping
        Invoke("SendPingMessage", NetStatic.TimePingPong);
    }

    private void SendPingMessage()
    {
        websocket.Send(NetStatic.PingMsg);
    }

    void Update()
    {

    }

    public void SendMessage(byte[] bin)
    {
        if (websocket != null && websocket.ReadyState == WebSocketState.Open)
        {

            var arr = NetHelper.PackSocketMsg(bin);
            try
            {
                websocket.Send(arr);
                NetworkStatistic.Instance.OnSendBytes(arr.Length);
                Debug.Log($"Send {arr.Length} to server");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }
    }


    public async void Disconnect()
    {
        websocket.Close();
    }

    private void OnApplicationQuit()
    {
        websocket.Close();
    }


    public void OnDestroy()
    {
        websocket.Close();
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

    public void SendBytes(byte[] bin, IPikaPeer id)
    {
        if (websocket != null && websocket.ReadyState == WebSocketState.Open)
        {

            var arr = NetHelper.PackSocketMsg(bin);
            try
            {
                websocket.Send(arr);
                NetworkStatistic.Instance.OnSendBytes(arr.Length);
                Debug.Log($"Send {arr.Length} to server");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }
    }

    public void SendBytes(byte[] bin)
    {
        if (websocket != null && websocket.ReadyState == WebSocketState.Open)
        {

            var arr = NetHelper.PackSocketMsg(bin);
            try
            {
                websocket.Send(arr);
                NetworkStatistic.Instance.OnSendBytes(arr.Length);
                Debug.Log($"Send {arr.Length} to server");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }
    }
}
