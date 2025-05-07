using MonkeNet.NetworkMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeExample
{
    public class RoomListObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private TextMeshProUGUI nameText;
        [SerializeField]
        private TextMeshProUGUI slotsText;
        [SerializeField]
        private Button joinButton;

        public void Set(LobbyManager lobbyManager, RoomData data)
        {
            nameText.text = data.Name;
            slotsText.text = data.Slots + "/" + data.MaxSlots;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(delegate { lobbyManager.SendJoinRoomRequest(data.Name); });
        }
    }
}
