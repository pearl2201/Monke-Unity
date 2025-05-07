using System;

public interface IMultiplayer
{
    public void SubscribeConnected(Action<IPikaPeer> cb);

    public void SubscribeDisconnected(Action<IPikaPeer> cb);

    public void SubscribeMessage(Action<IPikaPeer, byte[]> cb);

    public void SendBytes(byte[] bin, IPikaPeer id);

    public void SendBytes(byte[] bin);
}