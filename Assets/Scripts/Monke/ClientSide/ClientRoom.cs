using LiteNetLib.Utils;
using MonkeExample;
using MonkeNet.NetworkMessages;
using UnityEngine;

namespace MonkeNet.Client
{
    public class ClientRoom : MonoBehaviour
    {
        public int id;

        public ClientEntityManager entityManager;

        public delegate void RoomTickEventHandler(int currentTick, int currentRemoteTick);

        public event RoomTickEventHandler onClientTick;

        public delegate void CommandReceivedEventHandler(Area area, int areaId, INetSerializable command); // Using a C# signal here because the Godot signal wouldn't accept NetworkMessages.INetSerializable
        public event CommandReceivedEventHandler CommandReceived;

        [SerializeField] bool _debugNetworking;
        [SerializeField] SnapshotInterpolator _snapshotInterpolator;
        [SerializeField] ClientEntityManager _entityManager;
        [SerializeField] ClientInputManager _inputManager;
        [SerializeField] PredictionManager _predictionManager;

        ClientNetworkClock _clock;
        public PhysicsScene physicsScene;
        private int _currentTick = 0;
        public void Awake()
        {

        }

        public void Start()
        {
            physicsScene = gameObject.scene.GetPhysicsScene();


            CommandReceived += OnCommandReceived;
        }

        private void OnCommandReceived(Area area, int areaId, INetSerializable command)
        {
            if (area == Area.Room && areaId == id)
            {

            }
        }

        public void Update()
        {
            DisplayDebugInformation();
        }

        // TODO: I don't know if manually stepping physics inside _PhysicsProcess is a good idea,
        // as internally _PhysicsProcess will call _step() and _flush_queries() the same way I'm doing right now...
        // causing multiple calls to the same PhysicsServer methods
        void FixedUpdate()
        {

            // Advance Clock
            _currentTick++;
            int currentTick = _currentTick;
            int currentRemoteTick = _clock.GetRoomRemoteTick(_currentTick);
            if (CacheRuntime.Instance.GameStart)
            {
                var input = _inputManager.GenerateAndTransmitInputs(currentRemoteTick);         // Read and send produced input to the server
                EntitiesCallProcessTick(currentTick, currentRemoteTick, input);                 // Call OnProcessTick on all entities, pass current input so they can simulate

                onClientTick(currentTick, currentRemoteTick);
                // JoltPhysicsServer3D.GetSingleton().SpaceStep(MonkeNetManager.Instance.PhysicsSpace, PhysicsUtils.DeltaTime);
                // JoltPhysicsServer3D.GetSingleton().SpaceFlushQueries(MonkeNetManager.Instance.PhysicsSpace);

                physicsScene.Simulate(Time.fixedDeltaTime);
                _predictionManager.RegisterPrediction(currentRemoteTick, input);               // Register all local predictions
            }

        }

        // Calls OnProcessTick on all entities
        private static void EntitiesCallProcessTick(int currentTick, int remoteTick, INetSerializable input)
        {
            foreach (var node in MonkeNetConfig.Instance.EntitySpawner.Entities)
            {
                if (node is IClientEntity clientEntity)
                {
                    clientEntity.OnProcessTick(currentTick, remoteTick, input);
                }
            }
        }

        public void Initialize(int roomId, ClientNetworkClock clock)
        {
            id = roomId;
            _clock = clock;
            Debug.Log("Client Manager Initialized");
        }

        public void MakeEntityRequest(byte entityType) //TODO: This should NOT be here
        {
            _entityManager.MakeEntityRequest(entityType);
        }

        private void DisplayDebugInformation()
        {
            //if (_debugNetworking)
            //{
            //    StringBuilder builder = new StringBuilder();
            //    builder.AppendLine($"Network ID {_networkManager.GetSessionId()}");
            //    builder.AppendLine($"Framerate {Time.deltaTime}fps");
            //    builder.AppendLine($"Physics Tick {Time.fixedDeltaTime}hz");
            //    _clock.DisplayDebugInformation(builder);
            //    _networkDebug.DisplayDebugInformation(builder);
            //    _snapshotInterpolator.DisplayDebugInformation(builder);
            //    _inputManager.DisplayDebugInformation(builder);
            //    _predictionManager.DisplayDebugInformation(builder);

            //    _debugTextArea.text = builder.ToString();
            //}
        }
    }
}
