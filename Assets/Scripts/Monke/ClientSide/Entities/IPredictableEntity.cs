
using LiteNetLib.Utils;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.Client
{

    public interface IPredictableEntity : IClientEntity
    {
        IRecocilationState GetEntityStateData();
        public bool HasMisspredicted(IEntityStateData receivedState, IRecocilationState savedState);
        public void HandleReconciliation(IEntityStateData receivedState);
        public void ResimulateTick(INetSerializable input);
    }

    public interface IRecocilationState
    {

    }
}