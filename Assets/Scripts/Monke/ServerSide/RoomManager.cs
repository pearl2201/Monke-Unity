using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace MonkeNet.Server
{

    public class RoomManager : InternalServerComponent
    {
        public Dictionary<string, ServerRoom> rooms = new Dictionary<string, ServerRoom>();

        public static RoomManager Instance;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject roomPrefab;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
            CreateRoom(0, "Main", 25);
            //CreateRoom(1, "Main 2", 15);
        }
        public override void Start()
        {
            base.Start();
            ServerManager.Instance.onCommandReceived += CommandReceived;
        }

        private void CommandReceived(object sender, CommandReceivedArgs args)
        {
            if (args.command is LobbyJoinRoomRequest lobbyJoinReq)
            {
                RoomManager.Instance.TryJoinRoom(args.clientId, lobbyJoinReq);
            }
        }

        private void OnDestroy()
        {
            ServerManager.Instance.onCommandReceived -= CommandReceived;
        }

        public RoomData[] GetRoomDataList()
        {
            RoomData[] data = new RoomData[rooms.Count];
            int i = 0;
            foreach (KeyValuePair<string, ServerRoom> kvp in rooms)
            {
                ServerRoom r = kvp.Value;
                data[i] = new RoomData(r.Name, (byte)r.Peers.Count, r.MaxSlots);
                i++;
            }
            return data;
        }

        public void TryJoinRoom(MonkeNetPeer client, JoinRoomRequest data)
        {
            bool canJoin = true;

            if (!rooms.TryGetValue(data.RoomName, out var room))
            {
                canJoin = false;
            }
            else if (room.Peers.Count >= room.MaxSlots)
            {
                canJoin = false;
            }

            if (canJoin)
            {
                room.AddPlayerToRoom(client);
            }
            else
            {
                SendCommandToClient(client, Area.Lobby, NetworkAreaId.Default, new LobbyJoinRoomDenied(GetRoomDataList()));
            }
        }

        public void CreateRoom(int id, string roomName, byte maxSlots)
        {
            GameObject go = Instantiate(roomPrefab);
            ServerRoom room = go.GetComponent<ServerRoom>();
            room.Initialize(id, roomName, maxSlots);
            rooms.Add(roomName, room);
        }

        public void RemoveRoom(string roomName)
        {
            ServerRoom r = rooms[roomName];
            r.Close();
            rooms.Remove(roomName);
        }

    }
}
