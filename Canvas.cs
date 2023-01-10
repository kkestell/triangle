using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

using Pixels = ILGPU.Runtime.ArrayView2D<Painter.Vec3, ILGPU.Stride2D.DenseY>;

namespace Painter
{
    public readonly struct AddVec3 : IScanReduceOperation<Vec3>
    {
        public string CLCommand => "add";
       
        public Vec3 Identity => new(0, 0, 0);

        public Vec3 Apply(Vec3 first, Vec3 second)
        {
            return new Vec3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }
        
        public void AtomicApply(ref Vec3 target, Vec3 value)
        {
            Atomic.Add(ref target.X, value.X);
            Atomic.Add(ref target.Y, value.Y);
            Atomic.Add(ref target.Z, value.Z);
        }
    }
    
    public struct Canvas
    {
        readonly Accelerator device;
        readonly Action<Index2D, Pixels, Triangle> drawTriangleKernel;
        readonly Action<Index2D, Pixels> clearKernel;
        readonly Action<Index2D, Pixels, Pixels, ArrayView1D<float, Stride1D.Dense>> similarityKernel;
        readonly Action<Index2D, Pixels, Triangle, ArrayView1D<Vec3, Stride1D.Dense>, ArrayView1D<float, Stride1D.Dense>> averageColorKernel;
        readonly Action<Index2D, Pixels, Pixels> copyToKernel;
        readonly MemoryBuffer1D<float, Stride1D.Dense> pixelCache;
        readonly Pixels pixels;

        public int Width => pixels.IntExtent.X;
        public int Height => pixels.IntExtent.Y;

        Canvas(Accelerator device, MemoryBuffer2D<Vec3, Stride2D.DenseY> pixels)
        {
            this.device = device;
            this.pixels = pixels;

            pixelCache = device.Allocate1D<float>(pixels.IntExtent.X * pixels.IntExtent.Y);

            drawTriangleKernel = device.LoadAutoGroupedStreamKernel<Index2D, Pixels, Triangle>(DrawTriangleKernel);
            clearKernel = device.LoadAutoGroupedStreamKernel<Index2D, Pixels>(ClearKernel);
            similarityKernel = device.LoadAutoGroupedStreamKernel<Index2D, Pixels, Pixels, ArrayView1D<float, Stride1D.Dense>>(SimilarityKernel);
            copyToKernel = device.LoadAutoGroupedStreamKernel<Index2D, Pixels, Pixels>(CopyToKernel);
            averageColorKernel = device.LoadAutoGroupedStreamKernel<Index2D, Pixels, Triangle, ArrayView1D<Vec3, Stride1D.Dense>, ArrayView1D<float, Stride1D.Dense>>(AverageColorKernel);
        }

        public static Canvas Create(Accelerator device, int width, int height)
        {
            var pixels = device.Allocate2DDenseY<Vec3>(new Index2D(width, height));

            var c = new Canvas(device, pixels);

            c.Clear();
            
            return c;
        }

        public static Canvas Create(Accelerator device, string filename)
        {
            var image = Image.Load<Rgb24>(filename);
            var tmp = new Vec3[image.Width, image.Height];

            image.ProcessPixelRows(pixelAccessor =>
            {
                for (int y = 0; y < pixelAccessor.Height; y++)
                {
                    Span<Rgb24> row = pixelAccessor.GetRowSpan(y);
            
                    // Using row.Length helps JIT to eliminate bounds checks when accessing row[x].
                    for (int x = 0; x < row.Length; x++)
                    {
                        tmp[x, y].X = row[x].R / 256f;
                        tmp[x, y].Y = row[x].G / 256f;
                        tmp[x, y].Z = row[x].B / 256f;
                    }
                }
            });

            var pixels = device.Allocate2DDenseY(tmp);

            return new Canvas(device, pixels);
        }

        public Vec3 AverageColor(Triangle triangle)
        {
            using var sample = device.Allocate1D<Vec3>(Width * Height);
            using var mask = device.Allocate1D<float>(Width * Height);

            averageColorKernel(pixels.IntExtent, pixels, triangle, sample, mask);
            device.Synchronize();

            using var sum = device.Allocate1D(new[] { new Vec3(0, 0, 0) });
            device.Reduce<Vec3, AddVec3>(device.DefaultStream, sample.View, sum.View);
            device.Synchronize();

            var sTmp = new[] { new Vec3(0, 0, 0) };
            sum.CopyToCPU(sTmp);
            var s = sTmp[0];

            using var num = device.Allocate1D(new[] { 0.0f });
            device.Reduce<float, AddFloat>(device.DefaultStream, mask.View, num.View);
            device.Synchronize();

            var nTmp = new[] { 0.0f };
            num.CopyToCPU(nTmp);
            var n = nTmp[0];

            var color = new Vec3(s.X / n, s.Y / n, s.Z / n);
            return color;
        }
        
        public float Similarity(Canvas other)
        {
            similarityKernel(pixels.IntExtent, pixels, other.pixels, pixelCache.View);
            device.Synchronize();

            var sum = device.Allocate1D<float>(1);
            device.Reduce<float, AddFloat>(device.DefaultStream, pixelCache.View, sum.View);

            var reduced = sum.GetAsArray1D();

            return (float)Math.Sqrt(reduced[0] / (Width * Height));
        }

        public void DrawTriangle(Triangle triangle)
        {
            drawTriangleKernel(pixels.IntExtent, pixels, triangle);
            device.Synchronize();
        }

        public void Clear()
        {
            clearKernel(pixels.IntExtent, pixels);
            device.Synchronize();
        }

        public Image ToImage()
        {
            // var data = new Vec3[Width, Height];
            // pixels.CopyToCPU(data);
            // device.Synchronize();

            var data = pixels.GetAsArray2D();

            var idx = 0;
            var flat = new byte[Width * Height * 3];
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    flat[idx + 0] = (byte)(data[x, y].X * 255);
                    flat[idx + 1] = (byte)(data[x, y].Y * 255);
                    flat[idx + 2] = (byte)(data[x, y].Z * 255);
                    idx += 3;
                }
            }

            var image = Image.LoadPixelData<Rgb24>(flat, Width, Height);

            return image;
        }

        public void Save(string filename)
        {
            var image = ToImage();
            image.SaveAsPng(filename);
        }

        public void CopyTo(Canvas dest)
        {
            copyToKernel(pixels.IntExtent, pixels, dest.pixels);
            device.Synchronize();
        }

        static void DrawTriangleKernel(Index2D i, Pixels p, Triangle t)
        {
            if (PointInTriangle(i, t))
                SetColorKernel(i, p, t.Color);
        }
        
        static bool Edge(Vec2 a, Vec2 b, Index2D p) =>
            (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X) >= 0;

        static bool PointInTriangle(Index2D i, Triangle t) =>
            Edge(t.V1, t.V2, i) && Edge(t.V2, t.V3, i) && Edge(t.V3, t.V1, i);

        static void ClearKernel(Index2D i, Pixels p)
        {
            SetColorKernel(i, p, new Vec3(0, 0, 0));
        }
        
        static void SetColorKernel(Index2D i, Pixels p, Vec3 c)
        {
            if ((i.X >= 0) && (i.X < p.IntExtent.X) && (i.Y >= 0) && (i.Y < p.IntExtent.Y))
            {
                p[i] = c;
            }
        }

        static void SimilarityKernel(Index2D i, Pixels a, Pixels b, ArrayView1D<float, Stride1D.Dense> r)
        {
            r[i.Y * a.IntExtent.X + i.X] = 
                XMath.Pow(b[i].X - a[i].X, 2) + XMath.Pow(b[i].Y - a[i].Y, 2) + XMath.Pow(b[i].Z - a[i].Z, 2);
        }

        static void AverageColorKernel(
            Index2D i,
            Pixels p,
            Triangle t,
            ArrayView1D<Vec3, Stride1D.Dense> s,
            ArrayView1D<float, Stride1D.Dense> m)
        {
            var o = i.Y * p.IntExtent.X + i.X;

            if (PointInTriangle(i, t))
            {
                s[new Index1D(o)] = p[i];
                m[new Index1D(o)] = 1;
            }
            else
            {
                s[new Index1D(o)] = new Vec3(0, 0, 0);
                m[new Index1D(o)] = 0;
            }
        }

        static void CopyToKernel(Index2D i, Pixels s, Pixels d)
        {
            d[i] = s[i];
        }
    }
}
