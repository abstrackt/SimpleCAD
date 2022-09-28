using OpenTK.Mathematics;
using SimpleCAD.Source.Geometry;

namespace SimpleCAD.Source.Intersections
{
    public class IntersectionManager
    {
        private const float SUBDIV_PRECISION = 0.001f;
        private const int SUBDIV_ITERATIONS = 6;
        private const int NEWTON_ITERATIONS = 10;

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

        private struct Solution
        {
            public float u1, v1, u2, v2;

            public void Wrap(IParametricSurface s1, IParametricSurface s2)
            {
                if ((u1 < 0 || u1 > s1.RangeU) && !s1.WrapU)
                {
                    u1 = Math.Clamp(u1, 0, s1.RangeU);
                }

                if ((v1 < 0 || v1 > s1.RangeV) && !s1.WrapV)
                {
                    v1 = Math.Clamp(v1, 0, s1.RangeV);
                }

                if ((u2 < 0 || u2 > s2.RangeU) && !s2.WrapU)
                {
                    u2 = Math.Clamp(u2, 0, s2.RangeU);
                }

                if ((v2 < 0 || v2 > s2.RangeV) && !s2.WrapV)
                {
                    v2 = Math.Clamp(v2, 0, s2.RangeV);
                }
            }
        }

        public List<Vector3> FindIntersection(IParametricSurface s1, IParametricSurface s2)
        {
            var sol = FindStartingPoint(s1, s2);

            var points = FindIntersectionPoints(sol, 0.05f, s1, s2, out var cycle);

            if (!cycle)
            {
                var other = FindIntersectionPoints(sol, -0.05f, s1, s2, out var _);
                other = other.GetRange(1, other.Count - 1);
                other.Reverse();
                other.AddRange(points);
                points = other;
            }

            return points;
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

        private List<Vector3> FindIntersectionPoints(
            Solution start, float step, 
            IParametricSurface s1, IParametricSurface s2,
            out bool hasCycle)
        {
            hasCycle = false;

            var points = new List<Vector3>();
            var startP = s1.Sample(start.u1, start.v1);

            points.Add(startP);

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

                    next = new Solution()
                    {
                        u1 = next.u1 - decr.u1,
                        v1 = next.v1 - decr.v1,
                        u2 = next.u2 - decr.u2,
                        v2 = next.v2 - decr.v2
                    };

                    if (OutsideRange(next, s1, s2))
                        break;
                }

                var lastP = points[points.Count - 1];

                // Cycle
                if ((startP - lastP).Length < step && points.Count > 6)
                {
                    hasCycle = true;
                    break;
                }

                if (OutsideRange(next, s1, s2) || (p1 - p2).Length > Math.Abs(step)) {
                    break;
                }

                var nextP = s1.Sample(next.u1, next.v1);

                points.Add(nextP);

                last = next;
            }

            return points;
        }
    }
}
