namespace Painter
{
    public struct Triangle
    {
        public Vec2 V1;
        public Vec2 V2;
        public Vec2 V3;
        public Vec3 Color;

        public Triangle(Vec2 v1, Vec2 v2, Vec2 v3, Vec3 color)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;

            if (V2.Y < V1.Y) Swap(ref V1, ref V2);
            if (V3.Y < V1.Y) Swap(ref V1, ref V3);
            if (V3.Y < V2.Y) Swap(ref V2, ref V3);
        }

        public static Triangle Random(float width, float height)
        {
            var v1 = new Vec2(
                PseudoRandom.Next(width),
                PseudoRandom.Next(height));

            var v2 = new Vec2(
                PseudoRandom.Next(width),
                PseudoRandom.Next(height));

            var v3 = new Vec2(
                PseudoRandom.Next(width),
                PseudoRandom.Next(height));

            var color = new Vec3(PseudoRandom.Next(), PseudoRandom.Next(), PseudoRandom.Next());

            return new Triangle(v1, v2, v3, color);
        }

        /*
        public static Triangle Perturb(Triangle triangle, int radius, int width, int height)
        {
            var v1 = triangle.V1;
            var v2 = triangle.V2;
            var v3 = triangle.V3;

            switch (PseudoRandom.Next(0, 3))
            {
                case 0:
                    v1.X += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    v1.Y += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    break;
                case 1:
                    v2.X += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    v2.Y += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    break;
                case 2:
                    v3.X += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    v3.Y += (double)(PseudoRandom.NextDouble() * radius - radius / 2);
                    break;
            }

            v1.Clamp(width, height);
            v2.Clamp(width, height);
            v3.Clamp(width, height);

            return new Triangle(v1, v2, v3);
        }
        */

        static void Swap(ref Vec2 a, ref Vec2 b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}
