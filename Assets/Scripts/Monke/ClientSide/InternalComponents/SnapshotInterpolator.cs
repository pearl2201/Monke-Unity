using LiteNetLib.Utils;
using MonkeNet.NetworkMessages;
using MonkeNet.Shared;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MonkeNet.Client
{

    /// <summary>
    /// Receives and presents the Player the snapshots emmited by the server
    /// </summary>
    public partial class SnapshotInterpolator : InternalRoomClientComponent
    {
        [SerializeField] private int _minBufferTime = 3;

        private const int RecentPast = 0, NextFuture = 1;
        private double _interpolationFactor = 0;
        private int _bufferTime = 0;                // How many ticks in the past we are rendering the world state
        private double _currentTick = 0;            // Current local tick
        private readonly List<GameSnapshotMessage> _snapshotBuffer = new();

        public override void Start()
        {
            _bufferTime = 6; //TODO: magic number
            base.Start();
        }

        public void Update()
        {
            _currentTick += Time.deltaTime / PhysicsUtils.DeltaTime;
            double tickToProcess = _currentTick - _bufferTime; // (Current tick - _bufferTime) the point in time in the past which we want to render
            InterpolateStates(tickToProcess);
        }

        protected override void OnRoomProcessTick(int currentTick, int currentRemoteTick)
        {
            _currentTick = currentTick;
        }

        protected override void OnRoomCommandReceived(object sender, INetSerializable command)
        {
            if (command is GameSnapshotMessage snapshot)
            {
                // Add snapshot tu buffer if we don't have any or if it is a future one
                if (_snapshotBuffer.Count <= 0 || snapshot.Tick > _snapshotBuffer[^1].Tick)
                    _snapshotBuffer.Add(snapshot);
            }
        }

        protected override void OnServerLatencyCalculated(int latencyAverageTicks, int jitterAverageTicks)
        {
            SetBufferTime(latencyAverageTicks + jitterAverageTicks);
        }

        /* Example:
         * Current Render Time  = 15
         * Last Snapshot        = 13
         * Future Snapshot      = 20
         * |--------|--------------------|
         * 13(0)    15(x)                20(1)
         * Interpolation factor (x) = 0.28
         */
        private void InterpolateStates(double renderTick)
        {
            if (_snapshotBuffer.Count <= 1)
            {
                return; // We need at least 2 stated to interpolate
            }

            // Clear any unwanted (past) states
            while (_snapshotBuffer.Count > 2 && renderTick > _snapshotBuffer[1].Tick)
            {
                _snapshotBuffer.RemoveAt(0);
            }

            var nextSnapshot = _snapshotBuffer[NextFuture];
            var pastSnapshot = _snapshotBuffer[RecentPast];

            int diffBetweenStates = nextSnapshot.Tick - pastSnapshot.Tick;  // How "long" is the "line" between past and future states
            double currentRenderPoint = renderTick - pastSnapshot.Tick;     // Where in this "line" we are located based on current clock

            _interpolationFactor = currentRenderPoint / diffBetweenStates;  // Where in the line we are represented as a coefficient

            var futureStateCount = nextSnapshot.States.Length;

            for (int i = 0; i < futureStateCount; i++)
            {
                if (nextSnapshot.States.Length > i && pastSnapshot.States.Length > i)
                {
                    IEntityStateData futureState = nextSnapshot.States[i];
                    IEntityStateData pastState = pastSnapshot.States[i];

                    var entity = Room.entityManager.EntitySpawner.GetEntityById(futureState.EntityId); // FIXME: remove GetNode for the love of god

                    if (entity != null && entity is IInterpolatedEntity interpolatedEntity)
                    {
                        interpolatedEntity.HandleStateInterpolation(pastState, futureState, (float)_interpolationFactor);
                    }
                }
            }
        }

        public void SetBufferTime(int bufferTime)
        {
            _bufferTime = bufferTime + _minBufferTime;
        }

        public void DisplayDebugInformation(StringBuilder builder)
        {
            //if (ImGui.CollapsingHeader("Snapshot Interpolator"))
            {

                builder.AppendLine($"Interp. Factor {_interpolationFactor:0.00}");


                builder.AppendLine($"Buffer Size {_snapshotBuffer.Count} snapshots");
                builder.AppendLine($"Buffer Time {_bufferTime} ticks");

                int bufferTimeMs = (int)(_bufferTime * PhysicsUtils.DeltaTime * 1000);
                builder.AppendLine($"World State is {bufferTimeMs}ms in the past");
            }
        }
    }
}