using ILGPU;
using Painter;
using System;
using System.Diagnostics;

var context = Context.Create(builder => builder.Default().EnableAlgorithms());

// foreach (var d in context.Devices)
// {
//     using var a = d.CreateAccelerator(context);
//     Console.WriteLine(a.Name);
// }
//
// return;

var device = context.GetPreferredDevice(preferCPU: true).CreateAccelerator(context);

// Triangles
{
    var canvas = Canvas.Create(device, 256, 256);

    var sw = new Stopwatch();
    sw.Start();
    for (var i = 0; i < 8; i++)
    {
         var t = Triangle.Random(canvas.Width, canvas.Height);
         canvas.DrawTriangle(t);
    }
    var elapsed = sw.Elapsed;
    Console.WriteLine(elapsed.TotalMilliseconds);
    
    canvas.Save("triangles.png");
}

// Similarity
// {
//     var leena1 = Canvas.Create(device, "b.png");
//     var leena2 = Canvas.Create(device, "a.png");
//     
//     var s1 = leena1.Similarity(leena2);
//     var s2 = leena2.Similarity(leena1);
//
//     Debug.Assert(Math.Abs(s1 - s2) < float.Epsilon);
//
//     Console.WriteLine(s1);
// }
