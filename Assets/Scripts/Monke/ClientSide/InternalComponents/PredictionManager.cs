using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonkeNet.Client
{

    /// <summary>
    /// Stores predicted game states for entities, upon receiving an snapshot, will check for deviation and perform rollback and re-simulation if needed.
    /// </summary>
    public partial class PredictionManager : InternalClientComponent
    {
        private readonly List<PredictedState> _predictedStates = new List<PredictedState>();
        private int _lastTickReceived = 0;
        private int _misspredictionsCount = 0;
        private int _missedLocalState = 0;
        private static object _lock = new object();
        protected override void OnCommandReceived(Area area, int areaId, INetSerializable command)
        {
            if (!NetworkReady)
                return;

            if (command is GameSnapshotMessage snapshot)
            {
                if (snapshot.Tick > _lastTickReceived)
                {
                    _lastTickReceived = snapshot.Tick;
                    ProcessServerState(snapshot);
                }
            }
        }

        public void RegisterPrediction(int tick, INetSerializable input)
        {
            var predictedState = new PredictedState
            {
                Tick = tick,
                Input = input,
                Entities = new Dictionary<IPredictableEntity, IRecocilationState>()
            };

            _predictedStates.Add(predictedState);

            lock (_lock)
            {
                //TODO: use array of IPredictableEntity that updates each time a new entity is spawned/despawned
                //TODO: store entity state inside entity itself instead of having everything here on PredictionManager
                MonkeNetConfig.Instance.EntitySpawner.Entities.ForEach(entity =>
                {
                    if (entity is IPredictableEntity predictableEntity)
                    {
                        predictedState.Entities.Add(predictableEntity, predictableEntity.GetEntityStateData());
                    }
                });
            }
        }

        private void ProcessServerState(GameSnapshotMessage receivedSnapshot)
        {
            var predictedStateData = _predictedStates.Find(prediction => prediction.Tick == receivedSnapshot.Tick);
            _predictedStates.RemoveAll(predictedState => predictedState.Tick <= receivedSnapshot.Tick);

            if (predictedStateData == default(PredictedState) || predictedStateData.Tick != receivedSnapshot.Tick)
            {
                _missedLocalState++;
                return;
            }

            // Iterate all entities saved for the tick
            foreach (IPredictableEntity predictableEntity in predictedStateData.Entities.Keys)
            {
                // Get predicted and authoritative state for the entity
                var predictedState = predictedStateData.Entities[predictableEntity];
                var authoritativeState = FindStateForEntityId(predictableEntity.EntityId, receivedSnapshot.States);

                if (predictableEntity.HasMisspredicted(authoritativeState, predictedState))
                {
                    _misspredictionsCount++;
                    RollbackAndResimulate(receivedSnapshot.States, predictedStateData);
                    return;
                }
            }
        }

        private void RollbackAndResimulate(IEntityStateData[] authoritativeStates, PredictedState predictedStateData)
        {
            List<IPredictableEntity> keys = null;
            lock (_lock)
            {
                // han che bi thay doi trong qua trinh rollback
                keys = predictedStateData.Entities.Keys.ToList();
            }
            // Set all entities to authoritative state
            foreach (IPredictableEntity predictableEntity in keys)
            {
                var authoritativeState = FindStateForEntityId(predictableEntity.EntityId, authoritativeStates);
                predictableEntity.HandleReconciliation(authoritativeState);
            }

            // Advance simulation forward for all remaining inputs
            for (int i = 0; i < _predictedStates.Count; i++)
            {
                var remainingInput = _predictedStates[i];
                foreach (IPredictableEntity predictableEntity in keys)
                {
                    predictableEntity.ResimulateTick(remainingInput.Input);
                }

                // JoltPhysicsServer3D.GetSingleton().SpaceStep(MonkeNetManager.Instance.PhysicsSpace, PhysicsUtils.DeltaTime);
                // JoltPhysicsServer3D.GetSingleton().SpaceFlushQueries(MonkeNetManager.Instance.PhysicsSpace);
                ClientManager.Instance.physicsScene.Simulate(Time.fixedDeltaTime);
                foreach (IPredictableEntity predictableEntity in keys)
                {
                    Debug.Log("On update predictableEntity: " + predictableEntity.EntityId);
                    remainingInput.Entities[predictableEntity] = predictableEntity.GetEntityStateData();
                }
            }
        }

        private static IEntityStateData FindStateForEntityId(int entityId, IEntityStateData[] authStates)
        {
            foreach (IEntityStateData state in authStates) { if (state.EntityId == entityId) { return state; } }
            return null;
        }

        public void DisplayDebugInformation(StringBuilder builder)
        {
            // if (ImGui.CollapsingHeader("Prediction Manager"))
            // {
            builder.AppendLine($"Misspredictions: {_misspredictionsCount}");
            builder.AppendLine($"Missed Local States: {_missedLocalState}");
            builder.AppendLine($"Prediction History: {_predictedStates.Count}");
            // }
        }

        private class PredictedState
        {
            public int Tick;                                            // Tick at which the input was taken
            public INetSerializable Input;                              // Input message sent to the server
            public Dictionary<IPredictableEntity, IRecocilationState> Entities;
        }
    }

}