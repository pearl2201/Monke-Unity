using LiteNetLib.Utils;
using MonkeExample;
using MonkeNet.Client;
using UnityEngine;
namespace MonkeExample
{
    public class PlayerInputProducer : InputProducerComponent
    {
        [SerializeField] private FirstPersonCameraController _cameraController;

        public override void Start()
        {
            base.Start();
            _cameraController = FindFirstObjectByType<FirstPersonCameraController>();
        }

        public override INetSerializable GenerateCurrentInput()
        {
            return new CharacterInputMessage
            {
                Velocity = GetCurrentPressedKeys(),
                CameraYaw = _cameraController.GetLateralRotationAngle()
            };
        }


        public static Vector3 GetCurrentPressedKeys()
        {
            byte keys = 0;
            var ver = Input.GetAxis("Vertical");
            var hoz = Input.GetAxis("Horizontal");
            return new Vector3(hoz, 0, ver);
        }

    }
}