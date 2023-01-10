using ILGPU.Algorithms;

namespace Painter
{
    public struct Vec3
    {
        public float X;
        public float Y;
        public float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vec3 operator *(Vec3 v1, float v)
        {
            return new Vec3(v1.X * v, v1.Y * v, v1.Z * v);
        }

        public float Length()
        {
            return XMath.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}
