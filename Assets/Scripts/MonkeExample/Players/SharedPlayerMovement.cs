using UnityEngine;

namespace MonkeExample
{
    public class SharedPlayerMovement : MonoBehaviour
    {
        [SerializeField] public Rigidbody jbox;

        public void AdvancePhysics(CharacterInputMessage input)
        {
            var velocity = PlayerMovementCalculator.CalculateVelocity(input);
            jbox.velocity = velocity;
        }

        public void SetPosAndVel(Vector3 pos, Vector3 vel)
        {
            transform.position = pos;
            jbox.velocity = vel;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Vector3 GetVelocity()
        {
            return jbox.velocity;
        }

        public void Update()
        {
            //this.transform.position = jbox.body.Position.ToUVector3();
        }
    }
}