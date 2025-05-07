namespace MonkeNet.Shared
{
    public class MonkeNetPeer
    {
        public int SessionId { get; private set; }
        public IPikaPeer Peer { get; private set; }

        public int Id { get; private set; }



        public MonkeNetPeer(IPikaPeer peer)
        {
            SessionId = (int)peer.Id;
            Peer = peer;
            peer.Tag = this;
        }

        public void Send(byte[] data)
        {
            Peer.SendMessage(data);
        }


    }
}
