using OpenTK.Mathematics;
using SimpleCAD.Source.Environment;
using SimpleCAD.Source.Geometry;
using SimpleCAD.Source.Utils;

namespace SimpleCAD.Source.Intersections
{
    public class IntersectionManager
    {
        private const float START_PRECISION = 0.05f;
        private const float NEWTON_PRECISION = 0.001f;
        private const float GRADIENT_STEP = 0.02f;
        private const int SUBDIV_ITERATIONS = 100;
        private const int CURSOR_ITERATIONS = 1000;
        private const int GRADIENT_ITERATIONS = 100;
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

        public bool TryFindIntersection(IParametricSurface s1, IParametricSurface s2, float normalOffset, float surfaceOffset, out IntersectionData result)
        {
            if (TryFindStartSubdiv(s1, s2, out var sol, surfaceOffset))
            {
                result = FindIntersection(sol, s1, s2, normalOffset, surfaceOffset);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool TryFindIntersection(IParametricSurface s1, IParametricSurface s2, Vector3 cursor, float normalOffset, float surfaceOffset, out IntersectionData result)
        {
            if (TryFindStart(cursor, s1, s2, out var sol, surfaceOffset))
            {
                result = FindIntersection(sol, s1, s2, normalOffset, surfaceOffset);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        private IntersectionData FindIntersection(Solution sol, IParametricSurface s1, IParametricSurface s2, float normalOffset, float surfaceOffset)
        {
            var side1 = FindIntersectionPoints(sol, 0.02f, s1, s2, normalOffset, surfaceOffset, out var cycle);

            if (!cycle)
            {
                var side2 = FindIntersectionPoints(sol, -0.02f, s1, s2, normalOffset, surfaceOffset, out var _);

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

        private bool TryFindStartSubdiv(IParametricSurface s1, IParametricSurface s2, out Solution solution, float surfaceOffset)
        {
            Solution min = new Solution(0, 0, 0, 0);

            var it = 0;
            var minDist = float.MaxValue;
            var bestMin = min;
            var bestSol = min;

            float incrU1 = s1.RangeU;
            float incrV1 = s1.RangeV;
            float incrU2 = s2.RangeU;
            float incrV2 = s2.RangeV;

            while (it < SUBDIV_ITERATIONS)
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

                                var sol = new Solution(
                                    (minU1 + maxU1) / 2, 
                                    (minV1 + maxV1) / 2, 
                                    (minU2 + maxU2) / 2, 
                                    (minV2 + maxV2) / 2
                                    );

                                var p1 = s1.Sample(sol.u1, sol.v1, surfaceOffset);
                                var p2 = s2.Sample(sol.u2, sol.v2, surfaceOffset);

                                var dist = (p2 - p1).Length;

                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    bestMin = new Solution(minU1, minV1, minU2, minV2);
                                    bestSol = sol;
                                }
                            }
                        }
                    }
                }
                it++;
            }

            solution = bestSol;

            if (minDist <= START_PRECISION)
            {
                return true;
            }
            else
            {
                return TryFindStart(s2.Sample(bestSol.u2, bestSol.v2, surfaceOffset), s1, s2, out solution, surfaceOffset);
            }
        }

        private bool TryFindStart(Vector3 startPos, IParametricSurface s1, IParametricSurface s2, out Solution solution, float surfaceOffset)
        {
            var start = FindClosest(startPos, s1, s2, surfaceOffset);

            var it = 0;
            var step = GRADIENT_STEP;
            var lastDist = float.MaxValue;
            var currentDist = float.MaxValue;
            var current = start;
           
            while (it < GRADIENT_ITERATIONS)
            {
                var S1 = s1.Sample(current.u1, current.v1, surfaceOffset);
                var S2 = s2.Sample(current.u2, current.v2, surfaceOffset);

                currentDist = (S1 - S2).Length;

                if (currentDist > lastDist)
                {
                    step /= 2;
                }

                var grad = GradientAtPoint(current, s1, s2, surfaceOffset);
                current -= grad * step;

                lastDist = currentDist;
                it++;
            }

            current.Clamp(s1, s2);
            solution = current;
            return lastDist <= START_PRECISION;
        }

        private Solution FindClosest(Vector3 pos, IParametricSurface s1, IParametricSurface s2, float surfaceOffset)
        {
            var bestDist = float.MaxValue;
            var bestSol = new Solution(0, 0, 0, 0);

            for (int u1 = 0; u1 < CURSOR_ITERATIONS; u1++)
            {
                for (int v1 = 0; v1 < CURSOR_ITERATIONS; v1++)
                {
                    var u1s = u1 / (float)CURSOR_ITERATIONS * s1.RangeU;
                    var v1s = v1 / (float)CURSOR_ITERATIONS * s2.RangeV;
                    var S1 = s1.Sample(u1s, v1s, surfaceOffset);

                    var d = (pos - S1).Length;

                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestSol.u1 = u1s;
                        bestSol.v1 = v1s;
                    }
                }
            }

            bestDist = float.MaxValue;

            for (int u2 = 0; u2 < CURSOR_ITERATIONS; u2++)
            {
                for (int v2 = 0; v2 < CURSOR_ITERATIONS; v2++)
                {
                    var u2s = u2 / (float)CURSOR_ITERATIONS * s2.RangeU;
                    var v2s = v2 / (float)CURSOR_ITERATIONS * s2.RangeV;
                    var S2 = s1.Sample(u2s, v2s, surfaceOffset);

                    var d = (pos - S2).Length;

                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestSol.u2 = u2s;
                        bestSol.v2 = v2s;
                    }
                }
            }

            return bestSol;
        }

        private Solution GradientAtPoint(Solution p, IParametricSurface s1, IParametricSurface s2, float surfaceOffset)
        {
            var S1 = s1.Sample(p.u1, p.v1, surfaceOffset);
            var S2 = s2.Sample(p.u2, p.v2, surfaceOffset);

            var duS1 = s1.DerivU(p.u1, p.v1, surfaceOffset);
            var dvS1 = s1.DerivV(p.u1, p.v1, surfaceOffset);
            var duS2 = s2.DerivU(p.u2, p.v2, surfaceOffset);
            var dvS2 = s2.DerivV(p.u2, p.v2, surfaceOffset);

            return new Solution(
                (2 * S1.X * duS1.X - 2 * duS1.X * S2.X) + (2 * S1.Y * duS1.Y - 2 * duS1.Y * S2.Y) + (2 * S1.Z * duS1.Z - 2 * duS1.Z * S2.Z),
                (2 * S1.X * dvS1.X - 2 * dvS1.X * S2.X) + (2 * S1.Y * dvS1.Y - 2 * dvS1.Y * S2.Y) + (2 * S1.Z * dvS1.Z - 2 * dvS1.Z * S2.Z),
                (2 * S2.X * duS2.X - 2 * duS2.X * S1.X) + (2 * S2.Y * duS2.Y - 2 * duS2.Y * S1.Y) + (2 * S2.Z * duS2.Z - 2 * duS2.Z * S1.Z),
                (2 * S2.X * dvS2.X - 2 * dvS2.X * S1.X) + (2 * S2.Y * dvS2.Y - 2 * dvS2.Y * S1.Y) + (2 * S2.Z * dvS2.Z - 2 * dvS2.Z * S1.Z)
            );
        }

        private Solution NewtonStep(Solution p, float step, Vector3 p0, IParametricSurface s1, IParametricSurface s2, float surfaceOffset)
        {
            Vector4 F(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 t, float step)
            {
                return new Vector4((p1 - p2), Vector3.Dot(p1 - p0, t) - step);
            }

            var h = 0.001f;

            var p1 = s1.Sample(p.u1, p.v1, surfaceOffset);
            var p2 = s2.Sample(p.u2, p.v2, surfaceOffset);

            var n1 = Vector3.Cross(s1.DerivU(p.u1, p.v1, surfaceOffset), s1.DerivV(p.u1, p.v1, surfaceOffset)).Normalized(); 
            var n2 = Vector3.Cross(s2.DerivU(p.u2, p.v2, surfaceOffset), s2.DerivV(p.u2, p.v2, surfaceOffset)).Normalized();

            var t = Vector3.Cross(n1, n2).Normalized();

            var f = F(p0, p1, p2, t, step);

            var r1 = (F(p0, s1.Sample(p.u1 + h, p.v1, surfaceOffset), p2, t, step) - f) / h;
            var r2 = (F(p0, s1.Sample(p.u1, p.v1 + h, surfaceOffset), p2, t, step) - f) / h;
            var r3 = (F(p0, p1, s2.Sample(p.u2 + h, p.v2, surfaceOffset), t, step) - f) / h;
            var r4 = (F(p0, p1, s2.Sample(p.u2, p.v2 + h, surfaceOffset), t, step) - f) / h;

            var m = new Matrix4(r1, r2, r3, r4);
            m.Invert();

            var v = f * m;

            return new Solution(v.X, v.Y, v.Z, v.W);
        }

        private bool OutsideRange(Solution s, IParametricSurface s1, IParametricSurface s2)
        {
            return (!s1.WrapU && (s.u1 > s1.RangeU || s.u1 < 0)) ||
                   (!s1.WrapV && (s.v1 > s1.RangeV || s.v1 < 0)) ||
                   (!s2.WrapU && (s.u2 > s2.RangeU || s.u2 < 0)) ||
                   (!s2.WrapV && (s.v2 > s2.RangeV || s.v2 < 0));
        }

        private IntersectionData FindIntersectionPoints(
            Solution start, 
            float step, 
            IParametricSurface s1, 
            IParametricSurface s2,
            float perimeterOffset,
            float surfaceOffset,
            out bool hasCycle)
        {
            hasCycle = false;

            var points = new List<Vector3>();
            var parameters = new List<Solution>();
            var startP = s1.Sample(start.u1, start.v1, surfaceOffset);

            points.Add(startP);
            parameters.Add(start);

            var it = 0;

            var last = start;

            while (it < 1000)
            {
                it++;

                var next = last;

                var p0 = s1.Sample(next.u1, next.v1, surfaceOffset);

                for (int i = 0; i < NEWTON_ITERATIONS; i++)
                {
                    var decr = NewtonStep(next, step, p0, s1, s2, surfaceOffset);

                    if (float.IsNaN(next.u1) || 
                        float.IsNaN(next.u2) || 
                        float.IsNaN(next.v1) || 
                        float.IsNaN(next.v2) ||
                        float.IsNaN(decr.u1) ||
                        float.IsNaN(decr.u2) ||
                        float.IsNaN(decr.v1) ||
                        float.IsNaN(decr.v2))
                    {
                        // Numerical failure occured
                        return new IntersectionData()
                        {
                            points = points,
                            parameters = parameters
                        };
                    }

                    next -= decr;

                    if (OutsideRange(next, s1, s2))
                    {
                        var clamped = next;
                        clamped.Clamp(s1, s2);
                        var clampedP = s1.Sample(clamped.u1, clamped.v1, surfaceOffset);
                        points.Add(clampedP);
                        parameters.Add(clamped);
                        break;
                    }

                    if ((s1.Sample(next.u1, next.v1, surfaceOffset) - s2.Sample(next.u2, next.v2, surfaceOffset)).Length < NEWTON_PRECISION)
                        break;
                }

                var lastP = points[points.Count - 1];

                if (OutsideRange(next, s1, s2)) 
                {
                    break;
                }

                var nextP = s1.Sample(next.u1, next.v1, surfaceOffset);
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

            if (perimeterOffset != 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    //Find normal for point
                    var p = parameters[i % parameters.Count];

                    var dU = s1.DerivU(p.u1, p.v1, surfaceOffset).Normalized();
                    var dV = s1.DerivV(p.u1, p.v1, surfaceOffset).Normalized();

                    var n = Vector3.Cross(dU, dV);

                    points[i] += n * perimeterOffset;
                }
            }

            return new IntersectionData()
            {
                points = points,
                parameters = parameters
            };
        }

        private void Bresenham(Vector2 from, Vector2 to, byte value, int texRes, ref byte[] t, bool wrapX = false, bool wrapY = false)
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
                var finalX = x1;
                var finalY = y1;
                
                if (wrapX)
                {
                    finalX = (x1 + texRes) % texRes;    
                }

                if (wrapY)
                {
                    finalY = (y1 + texRes) % texRes;
                }

                var idx = (texRes * finalY + finalX);
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

        private void Bresenham(Vector2 from, Vector2 to, byte value, int texRes, ref byte[,] t, bool wrapX = false, bool wrapY = false)
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
                var finalX = x1;
                var finalY = y1;

                if (wrapX)
                {
                    finalX = (x1 + texRes) % texRes;
                }

                if (wrapY)
                {
                    finalY = (y1 + texRes) % texRes;
                }

                if (finalX >= 0 && finalX < t.GetLength(0) &&
                    finalY >= 0 && finalY < t.GetLength(1))
                {
                    t[finalX, finalY] = value;
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

        private void FloodFill(Vector2 p, byte targetValue, byte replacementValue, int texRes, bool wrapX, bool wrapY, ref byte[] t)
        {
            var x = (int)p.X;
            var y = (int)p.Y;

            Stack<Vector2> pixels = new Stack<Vector2>();
            pixels.Push(new Vector2(x, y));

            while (pixels.Count > 0)
            {
                Vector2 a = pixels.Pop();
                
                if (((a.X < texRes && a.X >= 0) || wrapX) && 
                    ((a.Y < texRes && a.Y >= 0) || wrapY))
                {
                    var xWrap = (a.X + texRes) % texRes;
                    var yWrap = (a.Y + texRes) % texRes;
                    if (t[(int)xWrap + texRes * (int)yWrap] == targetValue)
                    {
                        t[(int)xWrap + texRes * (int)yWrap] = replacementValue;
                        pixels.Push(new Vector2(xWrap - 1, yWrap));
                        pixels.Push(new Vector2(xWrap + 1, yWrap));
                        pixels.Push(new Vector2(xWrap, yWrap - 1));
                        pixels.Push(new Vector2(xWrap, yWrap + 1));
                    }
                }
            }
            
            return;
        }

        private void FloodFill(Vector2 p, byte targetValue, byte replacementValue, int texRes, bool wrapX, bool wrapY, ref byte[,] t)
        {
            var x = (int)p.X;
            var y = (int)p.Y;

            Stack<Vector2> pixels = new Stack<Vector2>();
            pixels.Push(new Vector2(x, y));

            while (pixels.Count > 0)
            {
                Vector2 a = pixels.Pop();

                if (((a.X < texRes && a.X >= 0) || wrapX) &&
                    ((a.Y < texRes && a.Y >= 0) || wrapY))
                {
                    var xWrap = (a.X + texRes) % texRes;
                    var yWrap = (a.Y + texRes) % texRes;
                    if (t[(int)xWrap, (int)yWrap] == targetValue)
                    {
                        t[(int)xWrap, (int)yWrap] = replacementValue;
                        pixels.Push(new Vector2(xWrap - 1, yWrap));
                        pixels.Push(new Vector2(xWrap + 1, yWrap));
                        pixels.Push(new Vector2(xWrap, yWrap - 1));
                        pixels.Push(new Vector2(xWrap, yWrap + 1));
                    }
                }
            }

            return;
        }

        public byte[,] GetIntersectTexture(
            IParametricSurface s,
            List<Solution> parameters,
            bool first,
            int texRes = DEFAULT_TEXTURE_RES)
        {
            var t = new byte[texRes, texRes];

            for (int p = 1; p < parameters.Count; p++)
            {
                Vector2 from, to;
                if (first)
                {
                    from = parameters[p - 1].FirstScaled(s.RangeU, s.RangeV);
                    to = parameters[p].FirstScaled(s.RangeU, s.RangeV);
                }
                else
                {
                    from = parameters[p - 1].SecondScaled(s.RangeU, s.RangeV);
                    to = parameters[p].SecondScaled(s.RangeU, s.RangeV);
                }
                
                Bresenham(from, to, 255, texRes, ref t, s.WrapU, s.WrapV);
            }

            FloodFill(Vector2.Zero, 0, 255, texRes, s.WrapU, s.WrapV, ref t);

            return t;
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
                Bresenham(from1, to1, 255, texRes, ref t1, s1.WrapU, s1.WrapV);
                Bresenham(from2, to2, 255, texRes, ref t2, s2.WrapU, s2.WrapV);
            }

            FloodFill(Vector2.Zero, 0, 255, texRes, s1.WrapU, s1.WrapV, ref t1);
            FloodFill(Vector2.Zero, 0, 255, texRes, s2.WrapU, s2.WrapV, ref t2);

            return (t1, t2);
        }
    }
}
