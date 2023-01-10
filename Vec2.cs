using ILGPU.Algorithms;

namespace Painter
{
    public struct Vec2
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vec2 operator -(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vec2 operator *(Vec2 v1, float v)
        {
            return new Vec2(v1.X * v, v1.Y * v);
        }

        public float Length()
        {
            return XMath.Sqrt(X * X + Y * Y);
        }

        public void Clamp(int width, int height)
        {
            if (X > width - 1)
                X = width - 1;

            if (X < 0)
                X = 0;

            if (Y > height - 1)
                Y = height - 1;

            if (Y < 0)
                Y = 0;
        }
    }
}
