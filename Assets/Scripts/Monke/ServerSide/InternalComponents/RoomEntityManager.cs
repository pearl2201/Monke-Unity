
using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace MonkeNet.Server
{

    /// <summary>
    /// Handles creation/deletion of entities
    /// </summary>
    public class RoomEntityManager : InternalServerComponent
    {
        public ServerRoom _room;
        EntitySpawner _entitySpawner;
        private int _entityIdCount = 0;

        private void Awake()
        {
            _room = GetComponent<ServerRoom>();
            _room.onClientConnected += OnRoomClientConnected;
            _room.onClientDisconnected += OnRoomClientDisconnected;
            _entitySpawner = EntitySpawner.Instance;
        }

        private void OnDestroy()
        {
            _room.onClientConnected -= OnRoomClientConnected;
            _room.onClientDisconnected -= OnRoomClientDisconnected;
        }
        public void SendSnapshotData(int currentTick)
        {
            var snapshotCommand = PackSnapshot(currentTick);
            SendCommandToRoom(_room, snapshotCommand);
        }

        protected override void OnServerCommandReceived(object sender, CommandReceivedArgs receivedArgs)
        {

            if (receivedArgs.command is EntityRequestMessage entityRequest)
            {
                if (receivedArgs.area == Area.Room && receivedArgs.areaId == _room.id)
                {
                    SpawnEntity(++_entityIdCount, entityRequest.EntityType, receivedArgs.clientId.SessionId);
                }
            }
        }



        protected void OnRoomClientConnected(object sender, MonkeNetPeer clientId)
        {
            SyncWorldState(clientId);
        }

        protected void OnRoomClientDisconnected(object sender, MonkeNetPeer clientId)
        {
            //TODO: this will send 1 packet for each entity, do in bulk, same as sync should be done
            List<int> entitiesGeneratedByAuthority = _entitySpawner.GetAllEntitiesByAuthority(clientId.SessionId);
            foreach (int entityId in entitiesGeneratedByAuthority)
            {
                DestroyEntity(entityId, (int)NetworkManagerPika.AudienceMode.Broadcast);
            }
        }

        /// <summary>
        /// Packs the current game state for a tick (Snapshot)
        /// </summary>
        /// <param name="currentTick"></param>
        private GameSnapshotMessage PackSnapshot(int currentTick)
        {
            // Solve which entities we should include in this snapshot
            List<IServerEntity> includedEntities = new List<IServerEntity>();
            foreach (INetworkedEntity entity in _entitySpawner.GetAllEntitiesByRoom(_room.id))
            {
                if (entity is IServerEntity serverEntity)
                {
                    includedEntities.Add(serverEntity);
                }
            }

            // Pack entity data into snapshot
            var entityCount = includedEntities.Count;

            var snapshot = new GameSnapshotMessage
            {
                Tick = currentTick,
                States = new IEntityStateData[entityCount]
            };

            for (int i = 0; i < entityCount; i++)
            {
                snapshot.States[i] = includedEntities[i].GenerateCurrentStateMessage();
            }

            return snapshot;
        }

        /// <summary>
        /// Notifies all clients that an Entity has spawned
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityType"></param>
        /// <param name="targetId"></param>
        /// <param name="authority"></param>
        private void SpawnEntity(int entityId, byte entityType, int authority)
        {
            var entityEvent = new EntityEventMessage
            {
                Event = EntityEventEnum.Created,
                EntityId = entityId,
                EntityType = entityType,
                Authority = authority
            };

            // Execute event locally and retrieve position and rotation data
            Transform instancedEntity = _entitySpawner.SpawnServerEntity(_room, entityEvent);
            entityEvent.Position = instancedEntity.position;
            //entityEvent.Rotation = instancedEntity.Rotation;

            SendCommandToRoom(_room, entityEvent);
        }

        /// <summary>
        /// Notifies all clients that an Entity has been destroyed
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="targetId"></param>
        private void DestroyEntity(int entityId, int targetId)
        {
            var entityEvent = new EntityEventMessage
            {
                Event = EntityEventEnum.Destroyed,
                EntityId = entityId,
                EntityType = 0,
                Authority = 0
            };

            _entitySpawner.DestroyEntity(entityEvent);  // Execute event locally

            SendCommandToRoom(_room, entityEvent);
        }

        /// <summary>
        /// Sends the whole game state to a specific clientId, used when the client connects to replicate world state
        /// </summary>
        /// <param name="clientId"></param>
        private void SyncWorldState(MonkeNetPeer clientId)
        {
            foreach (INetworkedEntity entity in _entitySpawner.GetAllEntitiesByRoom(_room.id))
            {
                var entityEvent = new EntityEventMessage
                {
                    Event = EntityEventEnum.Created,
                    EntityId = entity.EntityId,
                    EntityType = entity.EntityType,
                    Authority = entity.Authority,
                };

                SendCommandToClient(clientId, entityEvent);
            }

        }
    }
}