
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using LiteNetLib.Utils;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeNet.NetworkMessages
{
    public class NetworkAreaId
    {
        public const int Default = 1;
    }
    public enum Area : byte
    {
        None,
        Lobby,
        Room
    }
    public enum EntityEventEnum : byte //TODO: move somewhere else
    {
        Created,
        Destroyed

    }
    public enum ChannelEnum : int
    {
        Snapshot,
        Clock,
        EntityEventMessage,
        ClientInput,
        GameReliable,
        GameUnreliable
    }

    [NetPacket]
    public struct EntityRequestMessage : INetSerializable
    {

        public byte EntityType { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            EntityType = reader.GetByte();
        }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityType);
        }
    }
    [NetPacket]
    public struct ClockSyncMessage : INetSerializable
    {
        public int ClientTime { get; set; }
        public int ServerTime { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ClientTime = reader.GetInt();
            ServerTime = reader.GetInt();
        }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(ClientTime);
            writer.Put(ServerTime);
        }
    }
    [NetPacket]
    public struct EntityEventMessage : INetSerializable
    {
        public EntityEventEnum Event { get; set; }
        public int EntityId { get; set; }
        public byte EntityType { get; set; }
        public int Authority { get; set; }
        public Vector3 Position { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Event = (EntityEventEnum)reader.GetByte();
            EntityId = reader.GetInt();
            EntityType = reader.GetByte();
            Authority = reader.GetInt();
            Position = reader.GetVector3();
        }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Event);
            writer.Put(EntityId);
            writer.Put(EntityType);
            writer.Put(Authority);
            writer.Put(Position);
        }

    }
    [NetPacket]
    public struct GameSnapshotMessage : INetSerializable
    {
        public int Tick { get; set; }
        public IEntityStateData[] States { get; set; }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.PutArray(States);

        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            States = reader.GetArray<IEntityStateData>();

        }


    }

    [NetPacket]
    public struct PackedClientInputMessage : INetSerializable
    {
        public int Tick { get; set; } // This is the Tick stamp for the latest generated input (Inputs[Inputs.Length]), all other Ticks are (Tick - index)
        public INetSerializable[] Inputs { get; set; }

        public readonly void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            writer.PutSingleTypeArray(Inputs);
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetInt();
            Inputs = reader.GetSingleTypeArray<INetSerializable>();
        }
    }

    [NetPacket]
    public struct AcceptConnection : INetSerializable
    {
        public int SessionId { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            SessionId = (int)reader.GetLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((long)SessionId);
        }
    }
    [NetPacket]
    public struct LobbyJoinRoomAccepted : INetSerializable
    {
        public int Id { get; set; }

        public int CurrentTick { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(CurrentTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetInt();
            CurrentTick = reader.GetInt();
        }
    }
    [NetPacket]
    public class LoginAnonymouseRequestData : INetSerializable
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }

        public LoginAnonymouseRequestData(string deviceId)
        {
            DeviceId = deviceId;
        }

        public LoginAnonymouseRequestData()
        {

        }

        public LoginAnonymouseRequestData(string name, string deviceId)
        {
            Name = name;
        }


        public void Deserialize(NetDataReader reader)
        {
            Name = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Name);
        }
    }

    [NetPacket]
    public class LoginWithProviderRequestData : INetSerializable
    {

        public string Provider { get; set; }
        public string AccessToken { get; set; }
        public string UserId { get; set; }

        public LoginWithProviderRequestData()
        {

        }
        public LoginWithProviderRequestData(string provider, string userId, string accessToken)
        {
            Provider = provider;
            AccessToken = accessToken;
            UserId = userId;
        }

        public void Deserialize(NetDataReader reader)
        {
            Provider = reader.GetString();
            AccessToken = reader.GetString();
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Provider);
            writer.Put(AccessToken);
            writer.Put(UserId);
        }
    }

    public struct LoginRequestDenied : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {

        }

        public void Deserialize(NetDataReader reader)
        {

        }
    }
    [NetPacket]
    public struct LoginInfoData : INetSerializable
    {
        public ushort Id;
        public LobbyInfoData Data;

        public LoginInfoData(ushort id, LobbyInfoData data)
        {
            Id = id;
            Data = data;
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Data = new LobbyInfoData();
            Data.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Data);
        }
    }

    [NetPacket]
    public class LobbyFetchRoomDataRequest : LobbyInfoData
    {
        public LobbyFetchRoomDataRequest()
        {

        }
    }

    [NetPacket]
    public class LobbyJoinRoomDenied : LobbyInfoData
    {
        public LobbyJoinRoomDenied(RoomData[] rooms) : base(rooms)
        {

        }
    }

    [NetPacket]
    public class LobbyInfoData : INetSerializable
    {
        public RoomData[] Rooms;

        public LobbyInfoData(RoomData[] rooms)
        {
            Rooms = rooms;
        }

        public LobbyInfoData()
        {
            Rooms = new RoomData[0];
        }

        public void Deserialize(NetDataReader reader)
        {
            Rooms = reader.GetSingleTypeArray<RoomData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutSingleTypeArray<RoomData>(Rooms);
        }
    }
    [NetPacket]
    public class RoomData : INetSerializable
    {
        public string Name;
        public byte Slots;
        public byte MaxSlots;

        public RoomData(string name, byte slots, byte maxSlots)
        {
            Name = name;
            Slots = slots;
            MaxSlots = maxSlots;
        }

        public RoomData()
        {

        }
        public void Deserialize(NetDataReader reader)
        {
            Name = reader.GetString();
            Slots = reader.GetByte();
            MaxSlots = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Name);
            writer.Put(Slots);
            writer.Put(MaxSlots);
        }
    }

    [NetPacket]
    public class LobbyJoinRoomRequest : JoinRoomRequest
    {
        public LobbyJoinRoomRequest()
        {
        }

        public LobbyJoinRoomRequest(string name) : base(name)
        {
        }
    }
    [NetPacket]
    public class JoinRoomRequest : INetSerializable
    {
        public string RoomName;

        public JoinRoomRequest(string name)
        {
            RoomName = name;
        }

        public JoinRoomRequest() { }
        public void Deserialize(NetDataReader reader)
        {
            RoomName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(RoomName);
        }
    }
    [NetPacket]
    public class ClientFillInProfileValues : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {

        }

        public void Deserialize(NetDataReader reader)
        {

        }
    }

    [NetPacket]
    public class ClientFillInProfileValuesResponse : INetSerializable
    {
        public byte[] Data { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.PutBytesWithLength(Data);
        }

        public void Deserialize(NetDataReader reader)
        {
            Data = reader.GetBytesWithLength();
        }
    }

    [NetPacket]
    public class UpdateClientProfileResponse : INetSerializable
    {
        public byte[] Data { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.PutBytesWithLength(Data);
        }

        public void Deserialize(NetDataReader reader)
        {
            Data = reader.GetBytesWithLength();
        }
    }



}

public static class CacheTypeNames
{
    private static Dictionary<string, Type> typeNames = new Dictionary<string, Type>();

    public static Type GetType(string name)
    {
        if (typeNames.TryGetValue(name, out var type))
        {
            return type;
        }
        type = ByName(name);
        if (type != null)
        {
            typeNames[name] = type;
        }
        return type;
    }

    private static Type ByName(string name)
    {
        var temp = AppDomain.CurrentDomain.GetAssemblies()
                .Reverse()
                .Select(assembly => assembly.GetType(name));
        var ret =
                temp.FirstOrDefault(t => t != null);
        if (ret == null)
        {
            ret = temp
                .FirstOrDefault(t => t.Name.Contains(name));
        }

        return ret;

    }
}