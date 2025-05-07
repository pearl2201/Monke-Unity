namespace Assets.Scripts.NetSdk.Ws
{

    public class PikaNativeClientPeer : IPikaPeer
    {
        public PikaNativeClient pikaClient;

        public PikaNativeClientPeer(PikaNativeClient client)
        {
            this.pikaClient = client;
        }

        public void Disconnect()
        {
            pikaClient.Disconnect();
        }

        public void SendMessage(byte[] message)
        {
            pikaClient.SendMessage(message);
        }

        public object Tag { get; set; }
        public uint Id { get; set; } = 1;
    }

}
