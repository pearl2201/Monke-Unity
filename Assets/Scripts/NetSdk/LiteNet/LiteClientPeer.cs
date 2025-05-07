using LiteNetLib;

namespace Assets.Scripts.NetSdk
{
    public class LiteClientPeer : IPikaPeer
    {
        public LiteLibClient pikaClient;
        public NetPeer peer;
        public LiteClientPeer(LiteLibClient client, NetPeer peer)
        {
            this.pikaClient = client;
            this.peer = peer;
        }

        public void Disconnect()
        {
            peer.Disconnect();
        }

        public void SendMessage(byte[] message)
        {
            peer.Send(message, DeliveryMethod.ReliableOrdered);
        }

        public object Tag { get; set; }
        public uint Id { get; set; } = 1;
    }
}
