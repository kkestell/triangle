using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Painter
{
    static class PseudoRandom
    {
        static readonly Random rng = new(Guid.NewGuid().GetHashCode());

        public static float Next() => (float)rng.NextDouble();
        public static float Next(float max) => (float)rng.NextDouble() * max;
        public static float Next(float min, float max) => (float)(rng.NextDouble() * (max - min) + min);
    }
}
