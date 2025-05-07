using MonkeNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MonkeExample
{
    public class GameManager : MonoBehaviour
    {

        private void Start()
        {
            ClientManager.Instance.MakeEntityRequest((byte)GameEntityManager.EntityType.Player);
            CacheRuntime.Instance.GameStart = true;
        }
    }
}
