using LiteNetLib.Utils;
using MonkeNet.Shared;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MonkeExample
{
    // Entity state sent by the server to all clients every time a snapshot is produced
    public struct EntityStateMessage : IEntityStateData
    {
        public int EntityId { get; set; } // Entity Id
        public Vector3 Position { get; set; } // Entity Position
        public Vector3 Velocity { get; set; } // Entity velocity
        public float Yaw { get; set; } // Looking angle


        public readonly INetSerializable GetCopy() => this;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EntityId);
            writer.Put(Position);
            writer.Put(Velocity);
            writer.Put(Yaw);
        }

        public void Deserialize(NetDataReader reader)
        {
            EntityId = reader.GetInt();
            Position = reader.GetVector3();
            Velocity = reader.GetVector3();
            Yaw = reader.GetFloat();
        }
    }

    // Character inputs sent to the server by a local player every time a key is pressed
    public struct CharacterInputMessage : INetSerializable
    {
        public Vector3 Velocity { get; set; }

        public float CameraYaw { get; set; } // Yaw (were are we looking at)

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Velocity);
            writer.Put(CameraYaw);
        }

        public void Deserialize(NetDataReader reader)
        {
            Velocity = reader.GetVector3();
            CameraYaw = reader.GetFloat();
        }
    }
}

public static class NetDataExtensions
{
    public static void Put(this NetDataWriter writer, Vector3 v)
    {
        writer.Put(v.x);
        writer.Put(v.y);
        writer.Put(v.z);
    }

    public static Vector3 GetVector3(this NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PutSingleTypeArray<T>(this NetDataWriter writer, T[] e) where T : INetSerializable
    {
        if (e == null)
        {
            writer.Put(0);
        }
        else
        {

            writer.Put(e.Length);
            string typeName = typeof(T).FullName;
            if (e.Length > 0)
            {
                typeName = e[0].GetType().FullName;
            }
            writer.Put(typeName);

            foreach (var elem in e)
            {

                elem.Serialize(writer);
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetSingleTypeArray<T>(this NetDataReader writer) where T : INetSerializable
    {

        var n = writer.GetInt();
        if (n == 0)
        {
            return new T[0];
        }
        string typeName = writer.GetString();
        var t = CacheTypeNames.GetType(typeName);
        var arr = new T[n];
        for (int i = 0; i < n; i++)
        {

            var tt = Activator.CreateInstance(t);
            var temp = (T)tt;
            temp.Deserialize(writer);
            arr[i] = temp;

        }
        return arr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PutArray<T>(this NetDataWriter writer, T[] e) where T : INetSerializable
    {
        writer.Put(e.Length);
        foreach (var elem in e)
        {
            var typeName = elem.GetType().FullName;
            writer.Put(typeName);
            elem.Serialize(writer);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetArray<T>(this NetDataReader writer) where T : INetSerializable
    {

        var n = writer.GetInt();
        var arr = new T[n];
        for (int i = 0; i < n; i++)
        {
            var typeName = writer.GetString();
            var t = CacheTypeNames.GetType(typeName);
            var temp = (T)Activator.CreateInstance(t);
            temp.Deserialize(writer);
            arr[i] = temp;

        }
        return arr;
    }

}