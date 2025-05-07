public class PikaClientPeer : IPikaPeer
{
    public PikaClient pikaClient;

    public PikaClientPeer(PikaClient client)
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
