using LiteNetLib.Utils;
using MonkeNet.Shared;

namespace MonkeNet.Server
{

    /// <summary>
    /// Implement this on your server entity, the server entity manager will pick up all IServerEntity and broadcasts their states to clients
    /// </summary>
    public interface IServerEntity : INetworkedEntity
    {
        public ServerRoom Room { get; set; }
        public abstract void OnProcessTick(int tick, INetSerializable input);
        public IEntityStateData GenerateCurrentStateMessage();
    }
}