using LiteNetLib;

namespace Assets.Scripts.NetSdk
{
    public class LiteServerPeer : IPikaPeer
    {
        private LiteLibServer _server;

        public LiteLibServer Server
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

        private NetPeer _peer;

        public NetPeer Peer
        {
            get { return _peer; }
            set { _peer = value; }
        }
        public LiteServerPeer(LiteLibServer server, NetPeer peer)
        {
            _server = server;
            _peer = peer;
        }

        void IPikaPeer.Disconnect()
        {
            _peer.Disconnect();
        }

        public void SendMessage(byte[] message)
        {
            if (_peer.ConnectionState == ConnectionState.Connected)
            {
                var arr = NetHelper.PackSocketMsg(message);
                _peer.Send(arr, DeliveryMethod.ReliableOrdered);
                NetworkStatistic.Instance.OnSendBytes(message.Length);
            }

        }

        public object Tag { get; set; }
    }
}