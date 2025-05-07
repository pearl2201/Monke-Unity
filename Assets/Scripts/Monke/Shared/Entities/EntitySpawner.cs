using MonkeNet.NetworkMessages;
using MonkeNet.Server;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace MonkeNet.Shared
{

    public abstract class EntitySpawner : MonoBehaviour
    {

        public EventHandler<Transform> entitySpawnedEventHandler;

        public List<INetworkedEntity> Entities { get; private set; } = new List<INetworkedEntity>(); //TODO: make dictionary for easier access

        protected abstract Transform HandleEntityCreationClientSide(EntityEventMessage @event);
        protected abstract Transform HandleEntityCreationServerSide(EntityEventMessage @event);



        //TODO: do not cast, make Entities a list of INetworkedEntity directly
        public INetworkedEntity GetEntityById(int entityId)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is INetworkedEntity networkedEntity && networkedEntity.EntityId == entityId)
                {
                    return networkedEntity;
                }
            }

            // throw new MonkeNetException($"Couldn't find entity by id {entityId}");
            return null;
        }

        // Can be called from both the server or a client, so it needs to handle both scenarios
        public Transform SpawnClientEntity(EntityEventMessage @event)
        {
            Transform instancedNode = HandleEntityCreationClientSide(@event);

            var networkedEntity = instancedNode.GetComponent<INetworkedEntity>();
            if (networkedEntity == null)
            {
                throw new MonkeNetException($"Can't spawn entity that is not a {typeof(IServerEntity).Name}");
            }
            instancedNode.SetParent(this.transform);
            return HandleEntityInitialized(@event, instancedNode, networkedEntity);
        }

        // Can be called from both the server or a client, so it needs to handle both scenarios
        public Transform SpawnServerEntity(ServerRoom room, EntityEventMessage @event)
        {
            Transform instancedNode = HandleEntityCreationServerSide(@event);

            var networkedEntity = instancedNode.GetComponent<IServerEntity>();
            if (networkedEntity == null)
            {
                throw new MonkeNetException($"Can't spawn entity that is not a {typeof(IServerEntity).Name}");
            }
            networkedEntity.Room = room;
            //instancedNode.transform.parent = room.transform;
            SceneManager.MoveGameObjectToScene(instancedNode.gameObject, room.scene);
            return HandleEntityInitialized(@event, instancedNode, networkedEntity);
        }

        public Transform HandleEntityInitialized(EntityEventMessage @event, Transform instancedNode, INetworkedEntity networkedEntity)
        {

            InitializeEntity(instancedNode, networkedEntity, @event);

            Entities.Add(networkedEntity);
            entitySpawnedEventHandler?.Invoke(this, instancedNode);
            Debug.Log($"Spawned entity:{@event.EntityId} ({@event.EntityType}) Auth:{@event.Authority}");
            return instancedNode;
        }

        public void DestroyEntity(EntityEventMessage @event)
        {
            var entity = GetEntityById(@event.EntityId);
            entity.Free();
            Entities.Remove(entity);
        }

        public List<int> GetAllEntitiesByAuthority(int authority)
        {
            List<int> entitiesGeneratedByAuthority = new List<int>();

            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].Authority == authority)
                {
                    entitiesGeneratedByAuthority.Add(Entities[i].EntityId);
                }
            }

            return entitiesGeneratedByAuthority;
        }

        public List<INetworkedEntity> GetAllEntitiesByRoom()
        {
            List<INetworkedEntity> entitiesGeneratedByAuthority = new List<INetworkedEntity>();

            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is IServerEntity serverEntity)
                {
                    entitiesGeneratedByAuthority.Add(Entities[i]);
                }
            }

            return entitiesGeneratedByAuthority;
        }

        private static void InitializeEntity(Transform node, INetworkedEntity entity, EntityEventMessage @event)
        {
            node.name = @event.EntityId.ToString();
            entity.EntityId = @event.EntityId;
            entity.EntityType = @event.EntityType;
            entity.Authority = @event.Authority;
        }
    }
}