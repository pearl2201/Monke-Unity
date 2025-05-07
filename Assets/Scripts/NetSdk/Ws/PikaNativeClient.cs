using NativeWebSocket;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.NetSdk.Ws
{
    public class PikaNativeClient : MonoBehaviour, IMultiplayer
    {
        WebSocket websocket;
        public string addr;

        public List<Action<IPikaPeer>> onConnectedListener = new List<Action<IPikaPeer>>();

        public List<Action<IPikaPeer>> onDisconnectedListener = new List<Action<IPikaPeer>>();

        public List<Action<IPikaPeer, byte[]>> onMessagesListener = new List<Action<IPikaPeer, byte[]>>();

        private PikaNativeClientPeer serverPeer;

        // Start is called before the first frame update
        void Start()
        {


        }


        public async void StartClient(string protocol, string addr, int port)
        {
            websocket = new WebSocket($"{protocol}://{addr}:{port}/laputa");

            websocket.OnOpen += () =>
            {
                Loom.QueueOnMainThread(() =>
                {
                    Debug.Log("OnWebsocket open");
                    serverPeer = new PikaNativeClientPeer(this);
                    onConnectedListener.ForEach(x => x(serverPeer));
                });
            };

            websocket.OnError += (e) =>
            {

            };

            websocket.OnClose += (e) =>
            {
                Loom.QueueOnMainThread(() =>
                {
                    Debug.Log("OnWebsocket OnClose");
                    onDisconnectedListener.ForEach(x => x(serverPeer));
                    serverPeer = null;
                });
            };

            websocket.OnMessage += (bytes) =>
            {
               
                Loom.QueueOnMainThread(() =>
                {
                    NetHelper.ParseMessage(serverPeer, bytes, OnPingMessage, OnPongMessage, (bin) =>
                    {
                        foreach (var listener in onMessagesListener)
                        {
                            listener(serverPeer, bin);
                        }
                    });



                });
            };

            // waiting for messages
            await websocket.Connect();
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
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
#endif
        }

        public void SendMessage(byte[] message)
        {
            if (websocket != null)
            {
                var arr = new byte[1 + message.Length];
                arr[0] = 2;
                Array.Copy(message, 0, arr, 1, message.Length);
                websocket.Send(arr);
            }
        }


        public async void Disconnect()
        {
            await websocket.Close();
        }

        private async void OnApplicationQuit()
        {
            await websocket.Close();
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

        public async void SendBytes(byte[] bin, IPikaPeer id)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {

                var arr = NetHelper.PackSocketMsg(bin);
                try
                {
                    await websocket.Send(arr);
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

        public async void SendBytes(byte[] bin)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {

                var arr = NetHelper.PackSocketMsg(bin);
                try
                {
                    await websocket.Send(arr);
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
}
