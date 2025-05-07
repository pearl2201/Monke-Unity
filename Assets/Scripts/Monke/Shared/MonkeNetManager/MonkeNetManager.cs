using MonkeNet.Client;
using MonkeNet.Server;
using UnityEngine;
using Utils;

namespace MonkeNet.Shared
{

    public class MonkeNetManager : MonoSingleton<MonkeNetManager>
    {
    
        public bool IsServer { get; private set; } = false;
        // public Rid PhysicsSpace { get; private set; }
        public string protocol;
        [SerializeField] NetworkManagerPika _networkManager;
        [SerializeField] ClientManager _clientManager;
        [SerializeField] ServerManager _serverManager;



        public void Start()
        {
            if (MonkeNetConfig.Instance == null)
                throw new MonkeNetException("Missing MonkeNetConfig instance!");


        }

        public void CreateClient(string address, int port)
        {
            IsServer = false;


            // TODO: pass configurations as struct/.ini
            _clientManager.Initialize(_networkManager, protocol, address, port);
        }

        public void CreateServer(int port)
        {
            IsServer = true;




            // TODO: pass configurations as struct/.ini
            _serverManager.Initialize(_networkManager, port);
        }
    }
}