using System;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using System.Diagnostics;

/*
namespace Painter
{
    class XXX
    {
        readonly Context context;
        readonly Accelerator device;

        public XXX()
        {
            context = Context.Create(builder => builder.Default().EnableAlgorithms());
            device = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);
        }

        public void Run()
        {
            // Load an image and draw a triangle

            {
                var a = new Canvas(device, "/Users/kyle/source/repos/Triangle/a.png");
                var t = Painter.Random(a.Width, a.Height);

                var sw = new Stopwatch();
                sw.Start();

                for (var i = 0; i < 10000; i++)
                {
                    t = Painter.Random(a.Width, a.Height);
                    DrawTriangle(device, a, t);
                }

                Console.WriteLine($"Triangle in {sw.ElapsedMilliseconds}ms");

                a.Save("c.png");
            }

            // Root-mean-square deviation similarity

            {
                var a = new Canvas(device, "/Users/kyle/source/repos/Triangle/a.png");
                var b = new Canvas(device, "/Users/kyle/source/repos/Triangle/b.png");

                var sw = new Stopwatch();
                sw.Start();

                var s = Similarity(device, a, b);

                Console.WriteLine($"{s} similar in {sw.ElapsedMilliseconds}ms");
            }
        }

        public static void DrawTriangle(Accelerator device, Canvas canvas, Triangle triangle)
        {
            var drawTriangle = device.LoadAutoGroupedStreamKernel<Index2D, Canvas, Painter>(DrawTriangleKernel);

            drawTriangle(canvas.Extent, canvas, triangle);
            device.Synchronize();
        }

        public static double Similarity(Accelerator device, Canvas a, Canvas b)
        {
            var similarity = device.LoadAutoGroupedStreamKernel<Index2D, Canvas, Canvas, ArrayView1D<double, Stride1D.Dense>>(SimilarityKernel);

            var pixels = device.Allocate1D<double>(a.Width * a.Height);
            similarity(a.Extent, a, b, pixels.View);
            device.Synchronize();

            var sum = device.Allocate1D(new[] { 0.0f });
            device.Reduce<double, Adddouble>(device.DefaultStream, pixels.View, sum.View);
            device.Synchronize();

            var s = new double[1];
            sum.CopyToCPU(s);

            return (double)Math.Sqrt(s[0] / (a.Width * a.Height));
        }

        public static void DrawTriangleKernel(Index2D index, Canvas canvas, Triangle triangle)
        {
            static bool Edge(Vec2 a, Vec2 b, Index2D p) =>
                (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X) >= 0;

            var inside = Edge(triangle.V1, triangle.V2, index) &&
                         Edge(triangle.V2, triangle.V3, index) &&
                         Edge(triangle.V3, triangle.V1, index);

            if (inside)
                SetColorKernel(index, canvas, new Vec3(1, 1, 1));
        }

        public static void SetColorKernel(Index2D index, Canvas canvas, Vec3 color)
        {
            if ((index.X >= 0) && (index.X < canvas.Pixels.IntExtent.X) && (index.Y >= 0) && (index.Y < canvas.Pixels.IntExtent.Y))
            {
                canvas.Pixels[index] = color;
            }
        }

        public static void SimilarityKernel(Index2D index, Canvas a, Canvas b, ArrayView1D<double, Stride1D.Dense> result)
        {
            // Root-mean-square deviation

            result[index.Y * a.Width + index.X] =
                XMath.Pow(b.Pixels[index].X - a.Pixels[index].X, 2) +
                XMath.Pow(b.Pixels[index].Y - a.Pixels[index].Y, 2) +
                XMath.Pow(b.Pixels[index].Z - a.Pixels[index].Z, 2);
        }
    }
}
*/