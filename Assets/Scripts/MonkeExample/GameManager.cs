using UnityEngine;

namespace MonkeExample
{
    public class GameManager : MonoBehaviour
    {

        private void Start()
        {
            CacheRuntime.Instance.CurrentRoom.MakeEntityRequest((byte)GameEntityManager.EntityType.Player);
            CacheRuntime.Instance.GameStart = true;
        }
    }
}
