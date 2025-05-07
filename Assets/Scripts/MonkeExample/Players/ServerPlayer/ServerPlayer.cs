using LiteNetLib.Utils;
using MonkeNet.Server;
using MonkeNet.Shared;
using UnityEngine;
namespace MonkeExample
{
    public class ServerPlayer : MonoBehaviour, INetworkedEntity, IServerEntity
    {
        [SerializeField] private SharedPlayerMovement _playerMovement;
        [SerializeField] int entityId;
        [SerializeField] byte entityType;
        [SerializeField] int authority;
        public int EntityId { get => entityId; set => entityId = value; }
        public byte EntityType { get => entityType; set => entityType = value; }
        public int Authority { get => authority; set => authority = value; }
        public ServerRoom Room { get; set; }
        public float Yaw { get; set; }

        public void Free()
        {
            Destroy(gameObject);
        }

        public IEntityStateData GenerateCurrentStateMessage()
        {
            return new EntityStateMessage
            {
                EntityId = this.EntityId,
                Yaw = this.Yaw,
                Position = _playerMovement.GetPosition(),
                Velocity = _playerMovement.GetVelocity(),
            };
        }

        public void OnProcessTick(int tick, INetSerializable genericInput)
        {
            CharacterInputMessage input = (CharacterInputMessage)genericInput;
            Yaw = input.CameraYaw;
            Debug.Log("Handle input: " + input.Velocity);
            _playerMovement.AdvancePhysics(input);
        }


    }

}