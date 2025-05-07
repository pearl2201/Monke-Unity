using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using Utils;

namespace MonkeNet.Server
{
    public class AuthManager : MonoSingleton<AuthManager>
    {
        public delegate void AuthEvent(MonkeNetPeer peer);

        public event AuthEvent onUserSignIn;
        public event AuthEvent onUserSignOut;

        private void OnEnable()
        {
            ServerManager.Instance.onCommandReceived += CommandReceived;
            ServerManager.Instance.onClientDisconnected += HandleClientDisconnected;
        }

        private void HandleClientDisconnected(object sender, MonkeNetPeer clientId)
        {
            onUserSignOut?.Invoke(clientId);
        }

        private void CommandReceived(object sender, CommandReceivedArgs args)
        {
            if (args.command is LoginAnonymouseRequestData loginRequestData)
            {
                //var user = DbContext.Instance.Users.FirstOrDefault(x => x.DeviceId == loginRequestData.DeviceId);
                //if (user == null)
                //{
                //    user = new Data.Entities.User
                //    {
                //        DeviceId = loginRequestData.DeviceId,
                //        Username = $"User_{loginRequestData.DeviceId.Substring(0, 5)}"
                //    };
                //}
                //client.SetUser(user);
                //onUserSignIn?.Invoke(client);
                //ServerManager.Instance.SendCommandToClient(client, new LoginInfoData()
                //{
                //    Id = (ushort)client.SessionId,
                //    Data = new LobbyInfoData(RoomManager.Instance.GetRoomDataList())
                //});
            }
        }

        private void OnDisable()
        {
            ServerManager.Instance.onCommandReceived -= CommandReceived;
        }
    }
}
