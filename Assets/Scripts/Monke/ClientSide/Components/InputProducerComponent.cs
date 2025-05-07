using System;
using LiteNetLib.Utils;
using UnityEngine;
namespace MonkeNet.Client
{

    /// <summary>
    /// Input Producer Component can be linked with the ClientManager and will read and send inputs to the server each frame.
    /// </summary>
    public abstract partial class InputProducerComponent : ClientComponent
    {
        [SerializeField] private bool _current = true;

        public virtual void Start()
        {
            Current = true;
        }

        /// <summary>
        /// Return INetSerializable with input data.
        /// </summary>
        /// <returns></returns>
        public abstract INetSerializable GenerateCurrentInput();

        public bool Current
        {
            get { return _current; }
            set
            {
                
                _current = value;
            }
        }
    }

}