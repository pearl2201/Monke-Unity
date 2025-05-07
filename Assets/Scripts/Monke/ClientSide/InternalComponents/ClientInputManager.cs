
using LiteNetLib.Utils;
using MonkeExample;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System.Collections.Generic;
using System.Text;

namespace MonkeNet.Client
{

    /// <summary>
    /// Reads and transmits inputs to the server. Will adjust and send redundant inputs to compensate for bad network conditions.
    /// </summary>

    public partial class ClientInputManager : InternalRoomClientComponent
    {
        public InputProducerComponent inputProducerComponent;
        private readonly List<ProducedInput> _producedInputs = new List<ProducedInput>();
        private int _lastReceivedTick = 0;

        public INetSerializable GenerateAndTransmitInputs(int currentTick)
        {
            INetSerializable input = inputProducerComponent?.GenerateCurrentInput();

            if (input == null)
            {
                return null;
            }

            ProducedInput producedInput = new()
            {
                Tick = currentTick,
                Input = input
            };

            _producedInputs.Add(producedInput);
            SendInputsToServer(currentTick);
            return input;
        }

        // Pack and send current input + all non acked inputs (redundant inputs).
        private void SendInputsToServer(int currentTick)
        {
            var userCmd = new PackedClientInputMessage
            {
                Tick = currentTick,
                Inputs = new INetSerializable[_producedInputs.Count]
            };

            for (int i = 0; i < _producedInputs.Count; i++)
            {
                userCmd.Inputs[i] = _producedInputs[i].Input;
            }

            SendCommandToRoom(CacheRuntime.Instance.CurrentRoomId, userCmd);
        }

        // When we receive a snapshot back, we delete all inputs prior/equal to it since those were already processed.
        protected override void OnRoomCommandReceived(object sender, INetSerializable command)
        {
            if (command is GameSnapshotMessage snapshot && snapshot.Tick > _lastReceivedTick)
            {
                _lastReceivedTick = snapshot.Tick;
                _producedInputs.RemoveAll(input => input.Tick <= snapshot.Tick);
            }
        }

        public void DisplayDebugInformation(StringBuilder builder)
        {
            // if (ImGui.CollapsingHeader("Input Manager"))
            // {
            builder.AppendLine($"Redundant Inputs: {_producedInputs.Count}");
            // }
        }

        private struct ProducedInput
        {
            public int Tick;
            public INetSerializable Input;
        }
    }
}