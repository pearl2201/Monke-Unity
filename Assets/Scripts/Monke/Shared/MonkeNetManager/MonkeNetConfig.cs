
using MonkeNet.Client;
using MonkeNet.Shared;
using UnityEngine;
using Utils;

namespace MonkeNet
{

    /// <summary>
    /// Main MonkeNet configuration singleton.
    /// </summary>

    public class MonkeNetConfig : MonoSingleton<MonkeNetConfig>
    {
        public static MonkeNetConfig Instance { get; set; } = null;

        [Header("Shared")]
        /// <summary>
        /// Controls how different entities are spawned on both the client and server.
        /// </summary>
        [SerializeField] public EntitySpawner EntitySpawner;

        [Header("Client")]
        /// <summary>
        /// If set, CustomClientScene will be instantiated on this node's scene upon starting the Client, useful for managers, singletons, etc.
        /// </summary>
        [SerializeField] public UnityEngine.SceneManagement.Scene CustomClientScene;

        /// <summary>
        /// Local input producer when running on the client.
        /// </summary>
        [SerializeField] public InputProducerComponent InputProducer;

        [Header("Server")]
        /// <summary>
        /// If set, CustomServerScene will be instantiated on this node's scene upon starting the Server, useful for managers, singletons, etc.
        /// </summary>
        [SerializeField] public UnityEngine.SceneManagement.Scene CustomServerScene;

        void Awake()
        {
            if (Instance != null) { throw new MonkeNetException($"There are multiple {typeof(MonkeNetConfig).Name} instances!"); }
            Instance = this;
        }
    }

}