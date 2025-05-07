using UnityEngine;

namespace MonkeExample
{

    public enum InputFlags
    {
        Forward = 0b_0000_0001,
        Backward = 0b_0000_0010,
        Left = 0b_0000_0100,
        Right = 0b_0000_1000,
        Space = 0b_0001_0000,
        Shift = 0b_0010_0000,
    }

    // Helper class to calculate how the players CharacterBody3D should move
    public static class PlayerMovementCalculator
    {
        public static readonly float MoveSpeed = 5;

        public static Vector3 CalculateVelocity(CharacterInputMessage input)
        {
            var velocity = input.Velocity.normalized * MoveSpeed;
            return velocity;
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