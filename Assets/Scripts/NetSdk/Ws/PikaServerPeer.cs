using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public interface IPikaPeer
{
    uint Id { get; set; }
    object Tag { get; set; }
    void Disconnect();

    void SendMessage(byte[] message);
}
public class PikaServerPeer : WebSocketBehavior, IPikaPeer
{
    private PikaServer _server;

    public PikaServer Server
    {
        get
        {
            return _server;
        }
        set
        {
            _server = value;
        }
    }
    private uint _id;

    public uint Id
    {
        get { return _id; }
        set { _id = value; }
    }
    public PikaServerPeer()
    {
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Loom.QueueOnMainThread(() =>
        {
            Debug.Log("OnClose");
            base.OnClose(e);
            _server.OnWebsocketClose(this);
        });
    }


    protected override void OnError(ErrorEventArgs e)
    {
        Loom.QueueOnMainThread(() =>
        {
            base.OnError(e);
            _server.OnWebsocketError(this, e.Exception);
        });
    }

    protected override void OnOpen()
    {
        Loom.QueueOnMainThread(() =>
        {
            base.OnOpen();
            Debug.Log("OnOpen");
            _server.OnWebsocketOpen(this);
        });
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        Loom.QueueOnMainThread(() =>
        {
            if (e.IsBinary)
            {
                _server.OnMessage(this, e.RawData);
                NetworkStatistic.Instance.OnReceiveBytes(e.RawData.Length);
            }
        });
    }



    void IPikaPeer.Disconnect()
    {
        Close();
    }

    public void SendMessage(byte[] message)
    {
        if (this.ReadyState == WebSocketState.Open)
        {
            var arr = NetHelper.PackSocketMsg(message);
            Send(arr);
            NetworkStatistic.Instance.OnSendBytes(message.Length);
        }

    }

    public object Tag { get; set; }
}
