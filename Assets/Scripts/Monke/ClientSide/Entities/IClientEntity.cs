using LiteNetLib.Utils;
using MonkeNet.Shared;

namespace MonkeNet.Client{
public interface IClientEntity : INetworkedEntity
{
    public abstract void OnProcessTick(int tick, int remoteTick, INetSerializable input);
}
}