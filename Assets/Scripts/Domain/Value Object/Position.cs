using System;

namespace Domain.ValueObject
{
    public struct Position
    {
        public float X { get;}
        public float Z { get;}

        public Position(float x, float z)
        {
            X = x;
            Z = z;
        }

        public float GetDistance(Position other)
        {
            float dx = other.X - X;
            float dz = other.Z - Z;
            return (float)Math.Sqrt((dx * dx) + (dz * dz));
        }
    }
}