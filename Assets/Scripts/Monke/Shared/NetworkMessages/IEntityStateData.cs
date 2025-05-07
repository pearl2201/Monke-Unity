using LiteNetLib.Utils;

namespace MonkeNet.Shared
{

    public interface IEntityStateData : INetSerializable
    {
        public int EntityId { get; } // Entity ID this message is for
    }
}