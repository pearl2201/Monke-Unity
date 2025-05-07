
using LiteNetLib.Utils;
using MonkeExample;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.Client
{

    public partial class ClientEntityManager : InternalRoomClientComponent
    {
        [SerializeField] EntitySpawner _entitySpawner;

        public EntitySpawner EntitySpawner { get { return _entitySpawner; } }
        public void OnEnable()
        {

        }

        /// <summary>
        /// Requests the server to spawn an entity
        /// </summary>
        /// <param name="entityType"></param>
        public void MakeEntityRequest(byte entityType)
        {
            var req = new EntityRequestMessage
            {
                EntityType = entityType
            };

            SendCommandToRoom(CacheRuntime.Instance.CurrentRoomId, req);
        }

        protected override void OnRoomCommandReceived(INetSerializable command)
        {
            if (command is EntityEventMessage entityEvent)
            {
                switch (entityEvent.Event)
                {
                    case EntityEventEnum.Created:
                        _entitySpawner.SpawnClientEntity(entityEvent);
                        break;
                    case EntityEventEnum.Destroyed:
                        _entitySpawner.DestroyEntity(entityEvent);
                        break;
                    default:
                        break;
                }
            }
        }
    }

}