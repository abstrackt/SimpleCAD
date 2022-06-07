using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using OpenTK;
using OpenTK.Mathematics;
using SimpleCAD.Source.GUI;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Geometry
{
    public class C2InterpolatingCurve : AdaptiveCurve
    {
        public override int SegmentSize => 4;
        public override int SegmentOffset => 4;

        public C2InterpolatingCurve() : base() { }

        private (List<float> alfa, List<float> beta, List<Vector3> b) CreateSystemOfEquations(List<Vector3> points)
        {
            var alfa = new List<float>(points.Count - 2);
            var beta = new List<float>(points.Count - 2);
            var b = new List<Vector3>(points.Count - 2);

            var chord = new List<float>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                chord.Add((points[i + 1] - points[i]).Length);
            }

            for (int i = 1; i < points.Count - 1; i++)
            {
                alfa.Add(chord[i - 1] / (chord[i - 1] + chord[i]));
                beta.Add(chord[i] / (chord[i - 1] + chord[i]));

                b.Add(3 * ((points[i + 1] - points[i]) / chord[i] - (points[i] - points[i - 1]) / chord[i - 1]) / (chord[i - 1] + chord[i]));
            }

            return (alfa, beta, b);
        }

        private List<Vector3> SolveSplineSystem(List<float> alfa, List<float> beta, List<Vector3> R)
        {
            if (alfa.Count == 1)
            {
                R[0] /= 2;
                return R;
            }
            
            if (alfa.Count != beta.Count)
                return new List<Vector3>();

            // n is the number of unknowns
            var n = alfa.Count;

            // since we start from x0 (not x1)
            n--;

            /*
            |b0 c0 0 ||x0| |d0|
            |a1 b1 c1||x1|=|d1|
            |0  a2 b2||x2| |d2| 
            */

            beta[0] /= 2;
            R[0] /= 2;

            for (int i = 1; i < n; i++)
            {
                beta[i] /= 2 - alfa[i] * beta[i - 1];
                R[i] = (R[i] - alfa[i] * R[i - 1]) / (2 - alfa[i] * beta[i - 1]);
            }

            R[n] = (R[n] - alfa[n] * R[n - 1]) / (2 - alfa[n] * beta[n - 1]);

            for (int i = n; i-- > 0;)
            {
                R[i] -= beta[i] * R[i + 1];
            }

            return R;
        }

        private List<Vector3> GetBezierPoints(List<Vector3> cs, List<Vector3> points)
        {
            var n = points.Count - 1;

            Vector3[] a = new Vector3[points.Count];
            Vector3[] b = new Vector3[points.Count];
            Vector3[] c = new Vector3[points.Count];
            Vector3[] d = new Vector3[points.Count];

            var chord = new List<float>();

            for (int i = 0; i < n; i++)
            {
                chord.Add((points[i + 1] - points[i]).Length);
            }

            a[0] = points[0];
            c[0] = cs[0];

            for (int i = 1; i <= n; i++)
            {
                a[i] = points[i];
                c[i] = cs[i];
                d[i - 1] = 2 * (c[i] - c[i - 1]) / (6 * chord[i - 1]);
                b[i - 1] = (a[i] - a[i - 1] - c[i - 1] * (float)Math.Pow(chord[i - 1], 2) - d[i - 1] * (float)Math.Pow(chord[i - 1], 3)) / chord[i - 1];
            }

            // Do scaling
            for (int i = 0; i < n; i++)
            {
                b[i] *= (float)Math.Pow(chord[i], 1);
                c[i] *= (float)Math.Pow(chord[i], 2);
                d[i] *= (float)Math.Pow(chord[i], 3);
            }

            Matrix4 powerToBernstein = new Matrix4(
                new Vector4(1, 0, 0, 0),
                new Vector4(1, 1 / 3f, 0, 0),
                new Vector4(1, 2 / 3f, 1 / 3f, 0),
                new Vector4(1, 1, 1, 1));

            powerToBernstein = Matrix4.Transpose(powerToBernstein);

            var bezierPoints = new List<Vector3>();

            for (int i = 0; i < n; i++)
            {
                Vector4 xs = new Vector4(a[i].X, b[i].X, c[i].X, d[i].X);
                Vector4 ys = new Vector4(a[i].Y, b[i].Y, c[i].Y, d[i].Y);
                Vector4 zs = new Vector4(a[i].Z, b[i].Z, c[i].Z, d[i].Z);

                xs *= powerToBernstein;
                ys *= powerToBernstein;
                zs *= powerToBernstein;

                Vector3 p1 = new Vector3(xs.X, ys.X, zs.X);
                Vector3 p2 = new Vector3(xs.Y, ys.Y, zs.Y);
                Vector3 p3 = new Vector3(xs.Z, ys.Z, zs.Z);
                Vector3 p4 = new Vector3(xs.W, ys.W, zs.W);

                bezierPoints.Add(p1);
                bezierPoints.Add(p2);
                bezierPoints.Add(p3);
                if (i == n - 1)
                {
                    bezierPoints.Add(points[n]);
                }
                else
                {
                    bezierPoints.Add(p4);
                }

            }

            return bezierPoints;
        }

        protected override List<Vector3> ProcessControlPoints(List<Vector3> points)
        {
            List<Vector3> cs;

            // Remove duplicate interpolation nodes
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i-1] == points[i])
                {
                    points.RemoveAt(i);
                    i--;
                }
            }

            if (points.Count >= 3)
            {
                var system = CreateSystemOfEquations(points);
                cs = SolveSplineSystem(system.alfa, system.beta, system.b);

                // Add c_0 and c_n
                cs.Insert(0, Vector3.Zero);
                cs.Add(Vector3.Zero);
                return GetBezierPoints(cs, points);
            }
            
            return points;
        }
    }
}
