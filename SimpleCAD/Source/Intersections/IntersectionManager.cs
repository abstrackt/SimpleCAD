using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Intersections
{
    public class IntersectionManager
    {
        private const float SUBDIV_PRECISION = 0.001f;
        private const int SUBDIV_ITERATIONS = 6;
        private const int NEWTON_ITERATIONS = 10;
        public const int DEFAULT_TEXTURE_RES = 512;

        private static IntersectionManager instance = null;
        public static IntersectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IntersectionManager();
                }
                return instance;
            }
        }

        public IntersectionData FindIntersection(IParametricSurface s1, IParametricSurface s2)
        {
            var sol = FindStartingPoint(s1, s2);

            var side1 = FindIntersectionPoints(sol, 0.02f, s1, s2, out var cycle);

            if (!cycle)
            {
                var side2 = FindIntersectionPoints(sol, -0.02f, s1, s2, out var _);

                side2.points = side2.points.GetRange(1, side2.points.Count - 1);
                side2.points.Reverse();
                side2.points.AddRange(side1.points);
                side1.points = side2.points;

                side2.parameters = side2.parameters.GetRange(1, side2.parameters.Count - 1);
                side2.parameters.Reverse();
                side2.parameters.AddRange(side1.parameters);
                side1.parameters = side2.parameters;
            }

            return side1;
        }

        private Solution FindStartingPoint(IParametricSurface s1, IParametricSurface s2)
        {
            Solution min = new Solution()
            {
                u1 = 0,
                v1 = 0,
                u2 = 0,
                v2 = 0
            };

            var it = 0;
            var minDist = float.MaxValue;
            var bestMin = min;
            var bestSol = min;

            float incrU1 = s1.RangeU;
            float incrV1 = s1.RangeV;
            float incrU2 = s2.RangeU;
            float incrV2 = s2.RangeV;

            while (it < SUBDIV_ITERATIONS && minDist > SUBDIV_PRECISION)
            {
                var currMin = bestMin;

                incrU1 /= 4;
                incrV1 /= 4;
                incrU2 /= 4;
                incrV2 /= 4;

                for (int u1 = 0; u1 < 4; u1++)
                {
                    var minU1 = currMin.u1 + u1 * incrU1;
                    var maxU1 = minU1 + incrU1;

                    for (int v1 = 0; v1 < 4; v1++)
                    {
                        var minV1 = currMin.v1 + v1 * incrV1;
                        var maxV1 = minV1 + incrV1;

                        for (int u2 = 0; u2 < 4; u2++)
                        {
                            var minU2 = currMin.u2 + u2 * incrU2;
                            var maxU2 = minU2 + incrU2;

                            for (int v2 = 0; v2 < 4; v2++)
                            {
                                var minV2 = currMin.v2 + v2 * incrV2;
                                var maxV2 = minV2 + incrV2;

                                var sol = new Solution()
                                {
                                    u1 = (minU1 + maxU1) / 2,
                                    v1 = (minV1 + maxV1) / 2,
                                    u2 = (minU2 + maxU2) / 2,
                                    v2 = (minV2 + maxV2) / 2
                                };

                                var p1 = s1.Sample(sol.u1, sol.v1);
                                var p2 = s2.Sample(sol.u2, sol.v2);

                                var dist = (p2 - p1).Length;

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    bestMin = new Solution()
                                    {
                                        u1 = minU1,
                                        v1 = minV1,
                                        u2 = minU2,
                                        v2 = minV2
                                    };
                                    bestSol = sol;
                                }
                            }
                        }
                    }
                }
                it++;
            }

            return bestSol;
        }

        private Vector3 Tangent(Solution p, IParametricSurface s1, IParametricSurface s2)
        {
            var n1 = Vector3.Cross(s1.DerivU(p.u1, p.v1), s1.DerivV(p.u1, p.v1)).Normalized();
            var n2 = Vector3.Cross(s2.DerivU(p.u2, p.v2), s2.DerivV(p.u2, p.v2)).Normalized();

            return Vector3.Cross(n1, n2).Normalized();
        }

        private Vector4 F(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 t, float step)
        {
            return new Vector4((p1 - p2), Vector3.Dot(p1 - p0, t) - step);
        }

        private Solution Newton(Solution p, float step, Vector3 p0, IParametricSurface s1, IParametricSurface s2)
        {
            var h = 0.001f;

            var p1 = s1.Sample(p.u1, p.v1);
            var p2 = s2.Sample(p.u2, p.v2);

            var t = Tangent(p, s1, s2);

            var f = F(p0, p1, p2, t, step);

            var r1 = (F(p0, s1.Sample(p.u1 + h, p.v1), p2, t, step) - f) / h;
            var r2 = (F(p0, s1.Sample(p.u1, p.v1 + h), p2, t, step) - f) / h;
            var r3 = (F(p0, p1, s2.Sample(p.u2 + h, p.v2), t, step) - f) / h;
            var r4 = (F(p0, p1, s2.Sample(p.u2, p.v2 + h), t, step) - f) / h;

            var m = new Matrix4(r1, r2, r3, r4);
            m.Invert();

            var v = f * m;

            return new Solution()
            {
                u1 = v.X,
                v1 = v.Y,
                u2 = v.Z,
                v2 = v.W
            };
        }

        private bool OutsideRange(Solution s, IParametricSurface s1, IParametricSurface s2)
        {
            return (!s1.WrapU && (s.u1 > s1.RangeU || s.u1 < 0)) ||
                   (!s1.WrapV && (s.v1 > s1.RangeV || s.v1 < 0)) ||
                   (!s2.WrapU && (s.u2 > s2.RangeU || s.u2 < 0)) ||
                   (!s2.WrapV && (s.v2 > s2.RangeV || s.v2 < 0));
        }

        private IntersectionData FindIntersectionPoints(
            Solution start, float step, 
            IParametricSurface s1, IParametricSurface s2,
            out bool hasCycle)
        {
            hasCycle = false;

            var points = new List<Vector3>();
            var parameters = new List<Solution>();
            var startP = s1.Sample(start.u1, start.v1);

            points.Add(startP);
            parameters.Add(start);

            var it = 0;

            var last = start;

            while (it < 1000)
            {
                it++;

                var next = last;

                var p0 = s1.Sample(next.u1, next.v1);

                Vector3 p1 = default, p2 = default;

                for (int i = 0; i < NEWTON_ITERATIONS; i++)
                {
                    var decr = Newton(next, step, p0, s1, s2);

                    next -= decr;

                    if (OutsideRange(next, s1, s2))
                    {
                        var clamped = next;
                        clamped.Clamp(s1, s2);
                        var clampedP = s1.Sample(clamped.u1, clamped.v1);
                        points.Add(clampedP);
                        parameters.Add(clamped);
                        break;
                    }
                }

                var lastP = points[points.Count - 1];

                if (OutsideRange(next, s1, s2) || (p1 - p2).Length > Math.Abs(step)) 
                {
                    break;
                }

                var nextP = s1.Sample(next.u1, next.v1);
                points.Add(nextP);
                parameters.Add(next);

                // Cycle
                if ((startP - lastP).Length < step && points.Count > 4)
                {
                    hasCycle = true;
                    points.AddRange(points.GetRange(0, Math.Min(100, points.Count)));
                    break;
                }

                last = next;
            }

            return new IntersectionData()
            {
                points = points,
                parameters = parameters
            };
        }

        private void Bresenham(Vector2 from, Vector2 to, byte value, int texRes, ref byte[] t)
        {
            var x1 = (int)(from.X * texRes);
            var y1 = (int)(from.Y * texRes);
            var x2 = (int)(to.X * texRes);
            var y2 = (int)(to.Y * texRes);

            int w = x2 - x1;
            int h = y2 - y1;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int max = Math.Abs(w);
            int min = Math.Abs(h);
            if (!(max > min))
            {
                max = Math.Abs(h);
                min = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int n = max >> 1;
            for (int i = 0; i <= max; i++)
            {
                var idx = (texRes * y1 + x1);
                if (idx >= 0 && idx < t.Length)
                {
                    t[idx] = value;
                }
                n += min;
                if (!(n < max))
                {
                    n -= max;
                    x1 += dx1;
                    y1 += dy1;
                }
                else
                {
                    x1 += dx2;
                    y1 += dy2;
                }
            }
        }

        private void FloodFill(Vector2 p, byte targetValue, byte replacementValue, int texRes, ref byte[] t)
        {
            var x = (int)p.X;
            var y = (int)p.Y;

            Stack<Vector2> pixels = new Stack<Vector2>();
            pixels.Push(new Vector2(x, y));

            while (pixels.Count > 0)
            {
                Vector2 a = pixels.Pop();
                if (a.X < texRes && a.X >= 0 && a.Y < texRes && a.Y >= 0)
                {
                    if (t[(int)a.X + texRes * (int)a.Y] == targetValue)
                    {
                        t[(int)a.X + texRes * (int)a.Y] = replacementValue;
                        pixels.Push(new Vector2(a.X - 1, a.Y));
                        pixels.Push(new Vector2(a.X + 1, a.Y));
                        pixels.Push(new Vector2(a.X, a.Y - 1));
                        pixels.Push(new Vector2(a.X, a.Y + 1));
                    }
                }
            }
            
            return;
        }

        public (byte[] t1, byte[] t2) GetIntersectTexture(
            IParametricSurface s1, 
            IParametricSurface s2, 
            List<Solution> parameters, 
            int texRes = DEFAULT_TEXTURE_RES)
        {
            var texSize = texRes * texRes;

            var t1 = new byte[texSize];
            var t2 = new byte[texSize];

            for (int p = 1; p < parameters.Count; p++)
            {
                var from1 = parameters[p - 1].FirstScaled(s1.RangeU, s1.RangeV);
                var from2 = parameters[p - 1].SecondScaled(s2.RangeU, s2.RangeV);
                var to1 = parameters[p].FirstScaled(s1.RangeU, s1.RangeV);
                var to2 = parameters[p].SecondScaled(s2.RangeU, s2.RangeV);
                Bresenham(from1, to1, 128, texRes, ref t1);
                Bresenham(from2, to2, 128, texRes, ref t2);
            }

            FloodFill(Vector2.Zero, 0, 255, texRes, ref t1);
            FloodFill(Vector2.Zero, 0, 255, texRes, ref t2);

            return (t1, t2);
        }
    }
}
