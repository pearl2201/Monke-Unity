using LiteNetLib.Utils;
using MonkeNet.Client;
using MonkeNet.NetworkMessages;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeExample
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform roomListContainerTransform;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject roomListPrefab;

        [SerializeField]
        private float autoRefreshLobbyTime;


        private float lastRefreshLobby;

        public bool refreshLobby;
        [SerializeField]
        private GameObject roomPrefab;
        void Start()
        {
            ClientManager.Instance.CommandReceived += OnMessage;
            RefreshRooms(CacheRuntime.Instance.LobbyInfoData);
        }

        private void Update()
        {
            if (!refreshLobby)
            {
                lastRefreshLobby += Time.deltaTime;
                if (lastRefreshLobby > autoRefreshLobbyTime)
                {
                    RequestRefreshLobby();
                }
            }


        }

        void OnDestroy()
        {
            ClientManager.Instance.CommandReceived -= OnMessage;
        }

        private void OnMessage(Area area, int areaId, INetSerializable command)
        {
            if (command is LobbyJoinRoomDenied lobbyJoinRoomDenied)
            {
                OnRoomJoinDenied(lobbyJoinRoomDenied);
            }
            else if (command is LobbyJoinRoomAccepted lobbyJoinRoomAccepted)
            {
                CacheRuntime.Instance.CurrentRoomId = lobbyJoinRoomAccepted.Id;
                var roomGo = Instantiate(roomListPrefab);
                var clientRoom = roomGo.GetComponent<ClientRoom>();
                CacheRuntime.Instance.CurrentRoom = clientRoom;
                clientRoom.id = lobbyJoinRoomAccepted.Id;
                clientRoom.Initialize(clientRoom.id, ClientManager.Instance.Clock);
                OnRoomJoinAcepted();
            }
        }

        public void RequestRefreshLobby()
        {
            refreshLobby = true;
            ClientManager.Instance.SendCommandToServer(new LobbyFetchRoomDataRequest());
        }
        public void SendJoinRoomRequest(string roomName)
        {
            ClientManager.Instance.SendCommandToServer(new LobbyJoinRoomRequest(roomName));
        }

        public void OnRoomJoinDenied(LobbyInfoData data)
        {
            RefreshRooms(data);
        }

        public void OnRoomJoinAcepted()
        {
            SceneManager.LoadScene("Game");
        }

        public void RefreshRooms(LobbyInfoData data)
        {
            lastRefreshLobby = 0;
            refreshLobby = false;
            RoomListObject[] roomObjects = roomListContainerTransform.GetComponentsInChildren<RoomListObject>();

            if (roomObjects.Length > data.Rooms.Length)
            {
                for (int i = data.Rooms.Length; i < roomObjects.Length; i++)
                {
                    Destroy(roomObjects[i].gameObject);
                }
            }

            for (int i = 0; i < data.Rooms.Length; i++)
            {
                RoomData d = data.Rooms[i];
                if (i < roomObjects.Length)
                {
                    roomObjects[i].Set(this, d);
                }
                else
                {
                    GameObject go = Instantiate(roomListPrefab, roomListContainerTransform);
                    go.GetComponent<RoomListObject>().Set(this, d);
                }
            }
        }
    }
}
