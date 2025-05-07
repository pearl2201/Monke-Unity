
using System.Text;
using System.Timers;
using TMPro;
using UnityEngine;

namespace MonkeNet.Shared
{

    /// <summary>
    /// This node calculates and displays some general Network data.
    /// </summary>
    public class NetworkDebug : MonoBehaviour
    {

        private int _sentPerSecond = 0,
                        _recPerSecond = 0,
                        _receivedPacketsPerSecond = 0,
                        _sentPacketsPerSecond = 0;

        public INetworkManager NetworkManager { get; set; } // FIXME: dependency injection? this looks out of place

        public void Start()
        {

        }

        private void FixedUpdate()
        {
            _sentPerSecond = NetworkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.SentBytes);
            _recPerSecond = NetworkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.ReceivedBytes);
            _receivedPacketsPerSecond = NetworkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.ReceivedPackets);
            _sentPacketsPerSecond = NetworkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.SentPackets);
        }

        public void DisplayDebugInformation(StringBuilder builder)
        {

            builder.AppendLine($"Sent Bytes {_sentPerSecond}");
            builder.AppendLine($"Rec. Bytes {_recPerSecond}");
            builder.AppendLine($"Packets Sent {_sentPacketsPerSecond}");
            builder.AppendLine($"Packets Rec. {_receivedPacketsPerSecond}");

        }
    }

}