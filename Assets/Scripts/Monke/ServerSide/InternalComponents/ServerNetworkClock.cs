using MonkeNet.NetworkMessages;
using System;
using UnityEngine;
namespace MonkeNet.Server
{

    public partial class ServerNetworkClock : InternalServerComponent
    {
        public EventHandler<double> onNetworkProcessTick;
        [SerializeField] private int _netTickrate = 30;
        private double _netTickCounter = 0;
        private int _currentTick = 0;

        public override void Start()
        {
            base.Start();
        }

        public void Update()
        {
            SolveSendNetworkTickEvent(Time.deltaTime);
        }

        public int ProcessTick()
        {
            _currentTick += 1;
            return _currentTick;
        }

        public int GetNetworkTickRate()
        {
            return _netTickrate;
        }

        private void SolveSendNetworkTickEvent(double delta)
        {
            _netTickCounter += delta;
            if (_netTickCounter >= (1.0 / _netTickrate))
            {
                onNetworkProcessTick?.Invoke(this, _netTickCounter);
                _netTickCounter = 0;
            }
        }

        // When we receive a sync packet from a Client, we return it with the current Clock data
        protected override void OnServerCommandReceived(object sender, CommandReceivedArgs args)
        {
            if (args.command is ClockSyncMessage sync)
            {
                sync.ServerTime = _currentTick;
                SendCommandToClient(args.clientId, sync);
            }
        }

        public void DisplayDebugInformation()
        {
            // if (ImGui.CollapsingHeader("Clock Information"))
            // {
            //     builder.AppendLine($"Network Tickrate {GetNetworkTickRate()}hz");
            //     builder.AppendLine($"Current Tick {_currentTick}");
            // }
        }
    }
}