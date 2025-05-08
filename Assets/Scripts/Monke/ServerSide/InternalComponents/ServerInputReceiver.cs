
using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using System.Collections.Generic;
using System.Linq;

namespace MonkeNet.Server
{


    public partial class ServerInputReceiver : InternalRoomComponent
    {
        private readonly Dictionary<int, Dictionary<IServerEntity, INetSerializable>> _pendingInputs = new Dictionary<int, Dictionary<IServerEntity, INetSerializable>>();
        private readonly Dictionary<IServerEntity, INetSerializable> _lastInputStored = new Dictionary<IServerEntity, INetSerializable>(); // Used for re-running old inputs in case no new inputs are received

        public INetSerializable GetInputForEntityTick(IServerEntity serverEntity, int tick)
        {

            // TODO: use something else, not try/catch
            try
            {
                var input = _pendingInputs[tick][serverEntity];
                _lastInputStored[serverEntity] = input; // Mark this input as the last processed input, so we can re-use it if no inputs are received from the client
                return input;
            }
            catch
            {
                // Reuse last input
                if (_lastInputStored.TryGetValue(serverEntity, out INetSerializable input))
                {
                    return input;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void OnRoomCommandReceived(object sender, CommandReceivedArgs args)
        {
            if (args.command is not PackedClientInputMessage inputCommand)
                return;

            // Find the ServerEntity target for this input command
            foreach (var entity in _room.EntityManager._entitySpawner.Entities)
            {
                if (entity is IServerEntity serverEntity && args.clientId.SessionId == serverEntity.Authority)
                {
                    RegisterCommand(serverEntity, inputCommand);
                }
            }
        }

        private void RegisterCommand(IServerEntity serverEntity, PackedClientInputMessage inputCommand)
        {
            int offset = inputCommand.Inputs.Length - 1;
            foreach (INetSerializable input in inputCommand.Inputs)
            {
                int tick = inputCommand.Tick - (offset--);

                // Check if we have an entry for this tick
                if (!_pendingInputs.TryGetValue(tick, out Dictionary<IServerEntity, INetSerializable> value))
                {
                    value = (new Dictionary<IServerEntity, INetSerializable>());
                    _pendingInputs.Add(tick, value);
                }

                value.TryAdd(serverEntity, input);
            }
        }

        public void DropOutdatedInputs(int currentTick)
        {
            var keys = _pendingInputs.Select(x => x.Key).ToList();
            for (int temp = 0; temp < keys.Count; temp++)
            {
                var key = keys[temp];
                if (key <= currentTick)
                {
                    _pendingInputs.Remove(key);
                }
            }
        }

        public void DisplayDebugInformation()
        {
            // if (ImGui.CollapsingHeader("Input Receiver"))
            // {
            //     builder.AppendLine($"Input Queue {_pendingInputs.Count}");
            // }
        }
    }

}