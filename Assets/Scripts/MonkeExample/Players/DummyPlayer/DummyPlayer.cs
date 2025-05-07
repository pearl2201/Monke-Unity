using MonkeNet.Client;
using MonkeNet.Shared;
using UnityEngine;

namespace MonkeExample
{
    public class DummyPlayer : MonoBehaviour, INetworkedEntity, IInterpolatedEntity
    {
        [SerializeField] int entityId;
        [SerializeField] byte entityType;
        [SerializeField] int authority;
        public int EntityId { get => entityId; set => entityId = value; }
        public byte EntityType { get => entityType; set => entityType = value; }
        public int Authority { get => authority; set => authority = value; }

        public void Free()
        {
            Destroy(gameObject);
        }

        public void HandleStateInterpolation(IEntityStateData past, IEntityStateData future, float interpolationFactor)
        {
            var pastState = (EntityStateMessage)past;
            var futureState = (EntityStateMessage)future;

            // Interpolate position
            this.transform.position = Vector3.Lerp(pastState.Position, futureState.Position, interpolationFactor);



            // Interpolate velocity
            Vector3 velocity = Vector3.Lerp(pastState.Velocity, futureState.Velocity, interpolationFactor);
        }
    }
}