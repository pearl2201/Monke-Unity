using MonkeNet.NetworkMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeExample
{
    public class CacheRuntime
    {
        public static CacheRuntime Instance { get; set; } = new CacheRuntime();

        public LobbyInfoData LobbyInfoData { get; set; }
        public int PlayerId { get; set; }

        public int CurrentRoomId { get; set; } = -1;

        public bool GameStart { get; set; }
        
        public CacheRuntime() { 
        
        
        }
    }
}
