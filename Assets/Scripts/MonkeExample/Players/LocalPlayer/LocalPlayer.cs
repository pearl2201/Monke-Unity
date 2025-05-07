using LiteNetLib.Utils;
using MonkeNet.Client;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeExample
{
    public class CharacterStateData : IRecocilationState
    {
        public Vector3 Position { get; set; }

    }
    public class LocalPlayer : MonoBehaviour, IPredictableEntity
    {
        [SerializeField] private float _maxDeviationAllowedSquared = 0.001f;
        [SerializeField] private SharedPlayerMovement _playerMovement;

        [SerializeField] int entityId;
        [SerializeField] byte entityType;
        [SerializeField] int authority;
        public int EntityId { get => entityId; set => entityId = value; }
        public byte EntityType { get => entityType; set => entityType = value; }
        public int Authority { get => authority; set => authority = value; }


        public void Free()
        {
            Destroy(gameObject);
        }

        public IRecocilationState GetEntityStateData()
        {
            return new CharacterStateData
            {
                Position = _playerMovement.GetPosition()
            };
        }

        public void HandleReconciliation(IEntityStateData receivedState)
        {
            EntityStateMessage state = (EntityStateMessage)receivedState;
            
            this._playerMovement.SetPosAndVel(state.Position, state.Velocity);
        }

        public bool HasMisspredicted(IEntityStateData receivedState, IRecocilationState savedPosition)
        {
            EntityStateMessage state = (EntityStateMessage)receivedState;
            return (state.Position - ((CharacterStateData)savedPosition).Position).magnitude > _maxDeviationAllowedSquared;
        }

        public void OnDestroy()
        {

        }

        public void OnProcessTick(int tick, int remoteTick, INetSerializable input)
        {
            _playerMovement.AdvancePhysics((CharacterInputMessage)input);
        }

        public void ResimulateTick(INetSerializable input)
        {
            _playerMovement.AdvancePhysics((CharacterInputMessage)input);
        }
    }
}