
using MonkeExample;
using MonkeNet.Client;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using UnityEngine;

public class GameEntityManager : EntitySpawner
{

    [SerializeField] LocalPlayer localPlayerPrefab;
    [SerializeField] DummyPlayer dummyPlayerPrefab;
    [SerializeField] GameObject serverPlayerPrefab;
    public enum EntityType : byte
    {
        Player,
        Prop
    }

    public Transform[] spawnPoints;

    protected override Transform HandleEntityCreationClientSide(EntityEventMessage @event)
    {
        if (@event.EntityType == (byte)EntityType.Player)
        {
            // TODO: use Authority/Owner herem not EntityId, as EntityId will not always be the same as the network id of the client who spawned it
            GameObject playerScene = @event.Authority == ClientManager.Instance.GetNetworkId() ?
                 localPlayerPrefab.gameObject :
                dummyPlayerPrefab.gameObject;
            var obj = GameObject.Instantiate(playerScene).transform;
            obj.position = @event.Position;
            return obj; // Spawn player scene
        }
        throw new System.Exception("NotKnowType");

    }

    protected override Transform HandleEntityCreationServerSide(EntityEventMessage @event)
    {
        if (@event.EntityType == (byte)EntityType.Player)
        {
            var obj = serverPlayerPrefab.gameObject;
            var temp = GameObject.Instantiate(obj); // Spawn player scene
            var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            temp.transform.position = point.position;
            return temp.transform;
        }
        throw new System.Exception("NotKnowType");
    }
}
