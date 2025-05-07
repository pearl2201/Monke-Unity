
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

        [SerializeField] public INetworkManager networkManager; // FIXME: dependency injection? this looks out of place

        public void Start()
        {

        }

        private void FixedUpdate()
        {
            if (networkManager != null)
            {
                _sentPerSecond = networkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.SentBytes);
                _recPerSecond = networkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.ReceivedBytes);
                _receivedPacketsPerSecond = networkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.ReceivedPackets);
                _sentPacketsPerSecond = networkManager.PopStatistic(INetworkManager.NetworkStatisticEnum.SentPackets);
            }
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